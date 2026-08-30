using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using IEditor.Core.Models;

namespace IEditor.Core.Services;

public sealed class MagickPhotoStudioService : IPhotoStudioService
{
    public Task<PhotoProcessingResult> ProcessAsync(PhotoProcessingRequest request, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ProcessInternal(request, cancellationToken), cancellationToken);
    }

    private static PhotoProcessingResult ProcessInternal(PhotoProcessingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath))
        {
            throw new ArgumentException("源图片路径不能为空。", nameof(request));
        }

        if (!File.Exists(request.SourcePath))
        {
            throw new FileNotFoundException("未找到源图片。", request.SourcePath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var widthPx = request.TargetSize.WidthPx;
        var heightPx = request.TargetSize.HeightPx;

        using var source = new MagickImage(request.SourcePath);
        source.AutoOrient();
        if (request.RotationDegrees != 0)
        {
            source.Rotate(request.RotationDegrees);
        }

        if (request.FlipHorizontal)
        {
            source.Flop();
        }

        if (request.EnableSmartCutout)
        {
            ApplySmartCutout(source);
        }

        source.Density = new Density(request.TargetSize.Dpi, request.TargetSize.Dpi);

        var baseScale = Math.Min(widthPx / (double)source.Width, heightPx / (double)source.Height);
        var zoomFactor = Math.Clamp(request.ZoomFactor, 0.05d, 8d);
        var scale = baseScale * zoomFactor;
        var drawWidth = Math.Max(1u, (uint)Math.Round(source.Width * scale, MidpointRounding.AwayFromZero));
        var drawHeight = Math.Max(1u, (uint)Math.Round(source.Height * scale, MidpointRounding.AwayFromZero));

        source.Resize(new MagickGeometry(drawWidth, drawHeight) { IgnoreAspectRatio = true });
        source.ResetPage();

        using var canvas = new MagickImage(request.BackgroundColor.ToMagickColor(), (uint)widthPx, (uint)heightPx);
        canvas.Density = new Density(request.TargetSize.Dpi, request.TargetSize.Dpi);
        var offsetX = (int)Math.Round((widthPx - drawWidth) / 2d + request.OffsetX, MidpointRounding.AwayFromZero);
        var offsetY = (int)Math.Round((heightPx - drawHeight) / 2d + request.OffsetY, MidpointRounding.AwayFromZero);
        canvas.Composite(source, offsetX, offsetY, CompositeOperator.Over);
        canvas.Quality = request.OutputFormat == PhotoOutputFormat.Jpeg ? request.JpegQuality : 100;

        using var output = new MemoryStream();
        canvas.Write(output, request.OutputFormat.ToMagickFormat());

        return new PhotoProcessingResult(output.ToArray(), widthPx, heightPx, request.OutputFormat);
    }

    private static void ApplySmartCutout(MagickImage image)
    {
        image.Alpha(AlphaOption.Set);

        var background = EstimateBackgroundColor(image, out var spread);
        image.ColorFuzz = new Percentage(Math.Clamp(2d + spread / 24d, 2d, 6d));
        var target = background.ToMagickColor();

        foreach (var (x, y) in GetSmartCutoutSeeds(image, background, spread))
        {
            image.FloodFill(MagickColors.Transparent, x, y, target);
        }
    }

    private static RgbColor EstimateBackgroundColor(MagickImage image, out double spread)
    {
        using var pixels = image.GetPixels();
        var width = (int)image.Width;
        var height = (int)image.Height;
        var samples = new List<RgbColor>(64);
        var visited = new HashSet<(int X, int Y)>();

        foreach (var (x, y) in GetBorderSeeds(width, height))
        {
            if (visited.Add((x, y)))
            {
                samples.Add(ReadPixel(pixels, x, y));
            }
        }

        if (samples.Count == 0)
        {
            spread = 0;
            return RgbColor.White;
        }

        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        foreach (var sample in samples)
        {
            totalR += sample.R;
            totalG += sample.G;
            totalB += sample.B;
        }

        var count = samples.Count;
        var averageR = totalR / count;
        var averageG = totalG / count;
        var averageB = totalB / count;

        var maxDeviation = 0d;
        foreach (var sample in samples)
        {
            maxDeviation = Math.Max(maxDeviation, Math.Abs(sample.R - averageR));
            maxDeviation = Math.Max(maxDeviation, Math.Abs(sample.G - averageG));
            maxDeviation = Math.Max(maxDeviation, Math.Abs(sample.B - averageB));
        }

        spread = maxDeviation;
        return new RgbColor(
            (byte)Math.Clamp(Math.Round(averageR), 0, 255),
            (byte)Math.Clamp(Math.Round(averageG), 0, 255),
            (byte)Math.Clamp(Math.Round(averageB), 0, 255));
    }

    private static IEnumerable<(int X, int Y)> GetSmartCutoutSeeds(MagickImage image, RgbColor background, double spread)
    {
        var width = (int)image.Width;
        var height = (int)image.Height;
        var threshold = Math.Clamp(10d + spread / 12d, 10d, 24d);

        var candidates = new[]
        {
            (0, 0),
            (Math.Max(0, width / 2), 0),
            (Math.Max(0, width - 1), 0),
            (0, Math.Max(0, height / 2)),
            (Math.Max(0, width - 1), Math.Max(0, height / 2)),
            (0, Math.Max(0, height - 1)),
            (Math.Max(0, width / 2), Math.Max(0, height - 1)),
            (Math.Max(0, width - 1), Math.Max(0, height - 1))
        };

        using var pixels = image.GetPixels();
        foreach (var candidate in candidates.Distinct())
        {
            var sample = ReadPixel(pixels, candidate.Item1, candidate.Item2);
            if (MaxChannelDistance(sample, background) <= threshold)
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<(int X, int Y)> GetBorderSeeds(int width, int height)
    {
        var stepX = Math.Max(1, width / 12);
        var stepY = Math.Max(1, height / 12);

        for (var x = 0; x < width; x += stepX)
        {
            yield return (x, 0);
            yield return (x, Math.Max(0, height - 1));
        }

        for (var y = 0; y < height; y += stepY)
        {
            yield return (0, y);
            yield return (Math.Max(0, width - 1), y);
        }
    }

    private static RgbColor ReadPixel(IPixelCollection<ushort> pixels, int x, int y)
    {
        var pixel = pixels.GetPixel(x, y);
        return new RgbColor(
            ToByte(pixel.GetChannel(0)),
            ToByte(pixel.GetChannel(1)),
            ToByte(pixel.GetChannel(2)));
    }

    private static int MaxChannelDistance(RgbColor left, RgbColor right) =>
        Math.Max(
            Math.Abs(left.R - right.R),
            Math.Max(Math.Abs(left.G - right.G), Math.Abs(left.B - right.B)));

    private static byte ToByte(ushort value) => (byte)Math.Clamp((int)Math.Round(value / 257d), 0, 255);
}

using System.Collections.Generic;
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
        source.Resize(new MagickGeometry((uint)widthPx, (uint)heightPx) { FillArea = true });
        source.Crop((uint)widthPx, (uint)heightPx, Gravity.Center);
        source.ResetPage();

        cancellationToken.ThrowIfCancellationRequested();

        using var canvas = new MagickImage(request.BackgroundColor.ToMagickColor(), (uint)widthPx, (uint)heightPx);
        canvas.Density = new Density(request.TargetSize.Dpi, request.TargetSize.Dpi);
        canvas.Composite(source, Gravity.Center, CompositeOperator.Over);
        canvas.Quality = request.OutputFormat == PhotoOutputFormat.Jpeg ? request.JpegQuality : 100;

        using var output = new MemoryStream();
        canvas.Write(output, request.OutputFormat.ToMagickFormat());

        return new PhotoProcessingResult(output.ToArray(), widthPx, heightPx, request.OutputFormat);
    }

    private static void ApplySmartCutout(MagickImage image)
    {
        image.Alpha(AlphaOption.Set);

        var background = EstimateBackgroundColor(image, out var spread);
        image.ColorFuzz = new Percentage(Math.Clamp(6d + spread / 8d, 6d, 18d));
        var target = background.ToMagickColor();

        foreach (var (x, y) in GetBorderSeeds((int)image.Width, (int)image.Height))
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

    private static byte ToByte(ushort value) => (byte)Math.Clamp((int)Math.Round(value / 257d), 0, 255);
}

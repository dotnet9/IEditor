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
}

namespace IEditor.Core.Models;

public sealed record PhotoProcessingRequest(
    string SourcePath,
    PhotoSizeSpec TargetSize,
    RgbColor BackgroundColor,
    PhotoOutputFormat OutputFormat = PhotoOutputFormat.Png,
    uint JpegQuality = 92);

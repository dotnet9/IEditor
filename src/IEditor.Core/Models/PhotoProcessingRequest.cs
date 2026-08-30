namespace IEditor.Core.Models;

public sealed record PhotoProcessingRequest(
    string SourcePath,
    PhotoSizeSpec TargetSize,
    RgbColor BackgroundColor,
    PhotoOutputFormat OutputFormat = PhotoOutputFormat.Png,
    uint JpegQuality = 92,
    int RotationDegrees = 0,
    bool FlipHorizontal = false,
    bool EnableSmartCutout = false,
    double ZoomFactor = 1d,
    double OffsetX = 0d,
    double OffsetY = 0d);

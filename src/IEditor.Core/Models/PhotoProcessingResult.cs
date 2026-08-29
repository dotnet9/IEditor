namespace IEditor.Core.Models;

public sealed record PhotoProcessingResult(
    byte[] Data,
    int WidthPx,
    int HeightPx,
    PhotoOutputFormat OutputFormat);

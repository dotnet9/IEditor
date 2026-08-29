namespace IEditor.Core.Models;

public readonly record struct PhotoSizeSpec(double WidthMm, double HeightMm, int Dpi)
{
    public int WidthPx => MmToPixels(WidthMm, Dpi);

    public int HeightPx => MmToPixels(HeightMm, Dpi);

    public double AspectRatio => WidthMm / HeightMm;

    public static int MmToPixels(double millimeters, int dpi)
    {
        if (millimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(millimeters));
        }

        if (dpi <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dpi));
        }

        return Math.Max(1, (int)Math.Round(millimeters / 25.4 * dpi, MidpointRounding.AwayFromZero));
    }
}

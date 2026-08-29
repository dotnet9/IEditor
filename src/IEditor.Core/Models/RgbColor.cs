using ImageMagick;

namespace IEditor.Core.Models;

public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static RgbColor White => new(255, 255, 255);

    public static RgbColor Blue => new(22, 119, 255);

    public MagickColor ToMagickColor() => MagickColor.FromRgb(R, G, B);

    public string ToHexString() => $"#{R:X2}{G:X2}{B:X2}";

    public override string ToString() => ToHexString();
}

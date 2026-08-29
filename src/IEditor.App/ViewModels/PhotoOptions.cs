using IEditor.Core.Models;

namespace IEditor.App.ViewModels;

public sealed record PhotoSizeOption(string DisplayName, string FileToken, double WidthMm, double HeightMm, bool IsCustom)
{
    public override string ToString() => DisplayName;
}

public sealed record BackgroundOption(string DisplayName, string FileToken, RgbColor Color, bool IsCustom)
{
    public override string ToString() => DisplayName;
}

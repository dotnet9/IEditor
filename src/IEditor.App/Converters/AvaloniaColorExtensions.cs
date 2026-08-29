using Avalonia.Media;
using IEditor.Core.Models;

namespace IEditor.App.Converters;

public static class AvaloniaColorExtensions
{
    public static RgbColor ToRgbColor(this Color color) => new(color.R, color.G, color.B);

    public static Color ToAvaloniaColor(this RgbColor color) => Color.FromRgb(color.R, color.G, color.B);
}

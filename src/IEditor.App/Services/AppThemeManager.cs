using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using IEditor.App.Converters;
using IEditor.App.Models;
using IEditor.Core.Models;

namespace IEditor.App.Services;

public sealed class AppThemeManager
{
    public void Apply(AppPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var application = Application.Current ?? throw new InvalidOperationException("Avalonia application has not been initialized.");
        application.RequestedThemeVariant = preferences.ThemeMode switch
        {
            AppThemeMode.Light => ThemeVariant.Light,
            AppThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        var dark = preferences.ThemeMode == AppThemeMode.Dark;
        var accent = preferences.AccentColor.ToAvaloniaColor();
        var accentShade = Blend(accent, Color.Parse("#00B8D9"), 0.35);
        var accentSoft = Color.FromArgb(dark ? (byte)48 : (byte)26, accent.R, accent.G, accent.B);
        var accentTint = Color.FromArgb(dark ? (byte)80 : (byte)52, accent.R, accent.G, accent.B);

        SetBrush(application, "IEditorWindowBackgroundBrush", dark ? Color.Parse("#0D1321") : Color.Parse("#F4F7FC"));
        SetBrush(application, "IEditorPageBackgroundBrush", dark ? Color.Parse("#0D1321") : Color.Parse("#F4F7FC"));
        SetBrush(application, "IEditorSurfaceBrush", dark ? Color.Parse("#151B2B") : Colors.White);
        SetBrush(application, "IEditorSurface2Brush", dark ? Color.Parse("#10182A") : Color.Parse("#F7F9FD"));
        SetBrush(application, "IEditorSegmentBrush", dark ? Color.Parse("#1A2336") : Color.Parse("#EDF1F8"));
        SetBrush(application, "IEditorSidebarBrush", dark ? Color.Parse("#131B2C") : Colors.White);
        SetBrush(application, "IEditorSidebarHoverBrush", dark ? Color.Parse("#1A2438") : Color.Parse("#F1F5FC"));
        SetBrush(application, "IEditorSidebarActiveBrush", dark ? Color.Parse("#172A49") : Color.Parse("#EAF1FF"));
        SetBrush(application, "IEditorBorderBrush", dark ? Color.Parse("#27324A") : Color.Parse("#E6EBF4"));
        SetBrush(application, "IEditorBorderStrongBrush", dark ? Color.Parse("#34405B") : Color.Parse("#D8E0EE"));
        SetBrush(application, "IEditorTextBrush", dark ? Color.Parse("#E8EEF9") : Color.Parse("#0F172A"));
        SetBrush(application, "IEditorTextSecondaryBrush", dark ? Color.Parse("#9AA7BD") : Color.Parse("#5B6B84"));
        SetBrush(application, "IEditorTextTertiaryBrush", dark ? Color.Parse("#66738B") : Color.Parse("#93A1B8"));
        SetBrush(application, "IEditorAccentBrush", accent);
        SetBrush(application, "IEditorAccentSoftBrush", accentSoft);
        SetBrush(application, "IEditorAccentTintBrush", accentTint);
        SetBrush(application, "IEditorSuccessBrush", Color.Parse("#12B76A"));
        SetBrush(application, "IEditorWarningBrush", Color.Parse("#F59E0B"));
        SetBrush(application, "IEditorDangerBrush", Color.Parse("#EF4444"));
        SetBrush(application, "IEditorCyanBrush", Color.Parse("#35E0FF"));
        SetBrush(application, "IEditorCloseHoverBrush", Color.Parse("#E81123"));
        SetBrush(application, "IEditorTitlebarHoverBrush", dark ? Color.Parse("#1E2A42") : Color.Parse("#E8EEF8"));
        SetBrush(application, "IEditorDimChipBrush", Color.Parse("#B80A1628"));
        SetBrush(application, "IEditorDimChipTextBrush", Color.Parse("#DFF4FF"));
        SetBrush(application, "IEditorSwatchBorderBrush", dark ? Color.Parse("#3A4864") : Color.Parse("#1F0F172A"));
        SetBrush(application, "IEditorSuccessSoftBrush", Color.Parse("#1A12B76A"));
        SetBrush(application, "IEditorSuccessTintBrush", Color.Parse("#4712B76A"));
        SetBrush(application, "IEditorWarningSoftBrush", Color.Parse("#1FF59E0B"));
        SetBrush(application, "IEditorWarningTintBrush", Color.Parse("#52F59E0B"));

        application.Resources["IEditorAccentGradientBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(accent, 0),
                new GradientStop(accentShade, 1)
            ]
        };

        application.Resources["IEditorNavActiveBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(26, accent.R, accent.G, accent.B), 0),
                new GradientStop(Color.FromArgb(20, 0, 184, 217), 1)
            ]
        };

        application.Resources["IEditorSpecActiveBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(18, accent.R, accent.G, accent.B), 0),
                new GradientStop(Color.FromArgb(15, 0, 184, 217), 1)
            ]
        };

        application.Resources["IEditorTitlebarBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(dark ? Color.Parse("#16203A") : Color.Parse("#FCFDFF"), 0),
                new GradientStop(dark ? Color.Parse("#121A2C") : Color.Parse("#F5F8FD"), 1)
            ]
        };
    }

    private static void SetBrush(Application application, string key, Color color) =>
        application.Resources[key] = new SolidColorBrush(color);

    private static Color Blend(Color left, Color right, double factor)
    {
        factor = Math.Clamp(factor, 0d, 1d);
        var inverse = 1 - factor;
        return Color.FromArgb(
            255,
            (byte)Math.Round(left.R * inverse + right.R * factor),
            (byte)Math.Round(left.G * inverse + right.G * factor),
            (byte)Math.Round(left.B * inverse + right.B * factor));
    }
}

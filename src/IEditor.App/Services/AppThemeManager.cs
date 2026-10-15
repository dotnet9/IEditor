using Avalonia;
using CodeWF.Avalonia.Controls.Themes;
using IEditor.App.Models;
namespace IEditor.App.Services;

/// <summary>主题模式与配色统一交给控件库，支持系统主题实时变化。</summary>
public sealed class AppThemeManager
{
    private StudioThemeController? _controller;
    public void Apply(AppPreferences preferences)
    {
        _controller ??= new StudioThemeController(Application.Current!);
        _controller.Mode = (StudioThemeMode)preferences.ThemeMode;
        _controller.Palette = preferences.Palette;
    }
}

using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using IEditor.App.Converters;
using IEditor.App.Models;
using IEditor.Core.Models;

namespace IEditor.App.ViewModels;

public partial class AccentColorOptionViewModel : ObservableObject
{
    public AccentColorOptionViewModel(string title, RgbColor color)
    {
        Title = title;
        Color = color;
    }

    public string Title { get; }

    public RgbColor Color { get; }

    public Color AvaloniaColor => Color.ToAvaloniaColor();

    [ObservableProperty]
    private bool isSelected;
}

public partial class LanguageOptionViewModel : ObservableObject
{
    public LanguageOptionViewModel(string title, string cultureName)
    {
        Title = title;
        CultureName = cultureName;
    }

    public string Title { get; }

    public string CultureName { get; }

    [ObservableProperty]
    private bool isSelected;
}

public partial class ThemeOptionViewModel : ObservableObject
{
    public ThemeOptionViewModel(string title, AppThemeMode value)
    {
        Title = title;
        Value = value;
    }

    public string Title { get; }

    public AppThemeMode Value { get; }

    [ObservableProperty]
    private bool isSelected;
}

public partial class FormatOptionViewModel : ObservableObject
{
    public FormatOptionViewModel(string title, PhotoOutputFormat value)
    {
        Title = title;
        Value = value;
    }

    public string Title { get; }

    public PhotoOutputFormat Value { get; }

    [ObservableProperty]
    private bool isSelected;
}

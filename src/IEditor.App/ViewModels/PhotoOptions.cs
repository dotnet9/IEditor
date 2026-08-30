using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using IEditor.App.Converters;
using IEditor.Core.Models;

namespace IEditor.App.ViewModels;

public partial class PhotoSizeOptionViewModel : ObservableObject
{
    public PhotoSizeOptionViewModel(string title, string token, double widthMm, double heightMm, bool isCustom)
    {
        Title = title;
        Token = token;
        WidthMm = widthMm;
        HeightMm = heightMm;
        IsCustom = isCustom;
    }

    public string Title { get; }

    public string Token { get; }

    public double WidthMm { get; }

    public double HeightMm { get; }

    public bool IsCustom { get; }

    public string SubLabel => IsCustom ? "自定" : $"{WidthMm:0}×{HeightMm:0}";

    [ObservableProperty]
    private bool isSelected;
}

public partial class BackgroundOptionViewModel : ObservableObject
{
    public BackgroundOptionViewModel(string title, string token, RgbColor color, bool isCustom)
    {
        Title = title;
        Token = token;
        _color = color;
        IsCustom = isCustom;
    }

    public string Title { get; }

    public string Token { get; }

    private RgbColor _color;

    public RgbColor Color
    {
        get => _color;
        private set
        {
            if (SetProperty(ref _color, value))
            {
                OnPropertyChanged(nameof(AvaloniaColor));
                OnPropertyChanged(nameof(AvaloniaBrush));
            }
        }
    }

    public Color AvaloniaColor => Color.ToAvaloniaColor();

    public IBrush AvaloniaBrush => new SolidColorBrush(AvaloniaColor);

    public bool IsCustom { get; }

    [ObservableProperty]
    private bool isSelected;

    public void SetColor(RgbColor color) => Color = color;
}

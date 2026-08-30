using CommunityToolkit.Mvvm.ComponentModel;

namespace IEditor.App.ViewModels;

public partial class BatchSizeOptionViewModel : ObservableObject
{
    public BatchSizeOptionViewModel(string title, string token, double? widthMm, double? heightMm, bool isFollowOriginal)
    {
        Title = title;
        Token = token;
        WidthMm = widthMm;
        HeightMm = heightMm;
        IsFollowOriginal = isFollowOriginal;
    }

    public string Title { get; }

    public string Token { get; }

    public double? WidthMm { get; }

    public double? HeightMm { get; }

    public bool IsFollowOriginal { get; }

    public string SubLabel => IsFollowOriginal ? "保持比例" : $"{WidthMm:0}×{HeightMm:0}";

    [ObservableProperty]
    private bool isSelected;
}

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

    [ObservableProperty]
    private bool isSelected;
}

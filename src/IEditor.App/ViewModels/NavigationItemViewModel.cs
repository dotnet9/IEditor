using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;

namespace IEditor.App.ViewModels;

public sealed partial class NavigationItemViewModel : LocalizedViewModel
{
    private readonly string _titleKey;
    private readonly string? _subtitleKey;
    private readonly string? _badgeKey;

    private string _title = string.Empty;
    private string _subtitle = string.Empty;
    private string _badge = string.Empty;

    public NavigationItemViewModel(
        string titleKey,
        Geometry icon,
        WorkspacePageViewModel? page,
        bool isEnabled = true,
        string? subtitleKey = null,
        string? badgeKey = null)
    {
        _titleKey = titleKey;
        _subtitleKey = subtitleKey;
        _badgeKey = badgeKey;
        Icon = icon;
        Page = page;
        IsEnabled = isEnabled;
    }

    public Geometry Icon { get; }

    public WorkspacePageViewModel? Page { get; }

    public bool IsEnabled { get; }

    [ObservableProperty]
    private bool isSelected;

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        private set => SetProperty(ref _subtitle, value);
    }

    public string Badge
    {
        get => _badge;
        private set => SetProperty(ref _badge, value);
    }

    protected override void RefreshLocalizedText()
    {
        Title = L(_titleKey);
        Subtitle = _subtitleKey is null ? string.Empty : L(_subtitleKey);
        Badge = _badgeKey is null ? string.Empty : L(_badgeKey);
    }
}

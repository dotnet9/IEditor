using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Icons;
using IEditor.App.Services;
using IEditor.Core.Services;

namespace IEditor.App.ViewModels;

public partial class MainViewModel : LocalizedViewModel, IDisposable
{
    private readonly PhotoStudioViewModel _photoStudioPage;
    private readonly BatchProcessingViewModel _batchProcessingPage;
    private readonly SettingsViewModel _settingsPage;
    private NavigationItemViewModel? _selectedNavigationItem;
    private WorkspacePageViewModel? _selectedPage;
    private string _windowTitle = string.Empty;
    private string _windowSubtitle = string.Empty;

    public MainViewModel(IPhotoStudioService photoStudioService, AppPreferencesService preferencesService)
    {
        _photoStudioPage = new PhotoStudioViewModel(photoStudioService, preferencesService);
        _batchProcessingPage = new BatchProcessingViewModel(photoStudioService, preferencesService);
        _settingsPage = new SettingsViewModel(preferencesService);

        NavigationItems =
        [
            new NavigationItemViewModel(
                Localization.Shell.Navigation.PhotoStudio,
                AppIcons.PhotoStudio,
                _photoStudioPage,
                true,
                subtitleKey: Localization.Shell.Navigation.Workspace),
            new NavigationItemViewModel(
                Localization.Shell.Navigation.BatchProcessing,
                AppIcons.BatchProcessing,
                _batchProcessingPage,
                true,
                badgeKey: Localization.Common.States.Planned),
            new NavigationItemViewModel(
                Localization.Shell.Navigation.SmartCutout,
                AppIcons.SmartCutout,
                null,
                false,
                badgeKey: Localization.Common.States.ComingSoon),
            new NavigationItemViewModel(
                Localization.Shell.Navigation.Toolbox,
                AppIcons.Toolbox,
                null,
                false,
                badgeKey: Localization.Common.States.ComingSoon),
            new NavigationItemViewModel(
                Localization.Shell.Navigation.Settings,
                AppIcons.Settings,
                _settingsPage,
                true)
        ];

        SelectedNavigationItem = NavigationItems[0];
        InitializeLocalizedText();
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public NavigationItemViewModel? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (value is { IsEnabled: false })
            {
                return;
            }

            if (SetProperty(ref _selectedNavigationItem, value))
            {
                NavigateToSelectedItem(value);
            }
        }
    }

    public WorkspacePageViewModel? SelectedPage
    {
        get => _selectedPage;
        private set
        {
            if (SetProperty(ref _selectedPage, value))
            {
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(WindowSubtitle));
                OnPropertyChanged(nameof(CurrentPageTitle));
                OnPropertyChanged(nameof(CurrentPageSubtitle));
            }
        }
    }

    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetProperty(ref _windowTitle, value);
    }

    public string WindowSubtitle
    {
        get => _windowSubtitle;
        private set => SetProperty(ref _windowSubtitle, value);
    }

    public string CurrentPageTitle => SelectedPage?.Title ?? string.Empty;

    public string CurrentPageSubtitle => SelectedPage?.Subtitle ?? string.Empty;

    public string CurrentPageBreadcrumb => SelectedPage?.Breadcrumb ?? string.Empty;

    public string CurrentPageStatus => SelectedPage?.StatusText ?? string.Empty;

    protected override void RefreshLocalizedText()
    {
        WindowTitle = L(Localization.Shell.Window.Title);
        UpdateWindowSubtitle();

        foreach (var item in NavigationItems)
        {
            item.Refresh();
        }
    }

    protected override void OnDisposed()
    {
        foreach (var item in NavigationItems)
        {
            item.Dispose();
        }

        _photoStudioPage.Dispose();
        _batchProcessingPage.Dispose();
        _settingsPage.Dispose();
        base.OnDisposed();
    }

    private void NavigateToSelectedItem(NavigationItemViewModel? item)
    {
        foreach (var navigationItem in NavigationItems)
        {
            navigationItem.IsSelected = navigationItem == item;
        }

        SelectedPage = item?.Page;
        UpdateWindowSubtitle();
    }

    private void UpdateWindowSubtitle()
    {
        WindowSubtitle = SelectedPage?.Subtitle
            ?? L(Localization.Shell.Window.WorkbenchSubtitle);
    }
}

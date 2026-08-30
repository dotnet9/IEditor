using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Icons;
using IEditor.App.Models;
using IEditor.App.Services;
using IEditor.App.Views;
using IEditor.Core.Services;

namespace IEditor.App.ViewModels;

public partial class MainViewModel : LocalizedViewModel, IDisposable
{
    private readonly PhotoStudioViewModel _photoStudioPage;
    private readonly BatchProcessingViewModel _batchProcessingPage;
    private readonly SettingsViewModel _settingsPage;
    private readonly AppPreferencesService _preferencesService;
    private NavigationItemViewModel? _selectedNavigationItem;
    private WorkspacePageViewModel? _selectedPage;
    private string _windowTitle = string.Empty;
    private string _windowSubtitle = string.Empty;

    public MainViewModel(IPhotoStudioService photoStudioService, AppPreferencesService preferencesService)
    {
        _photoStudioPage = new PhotoStudioViewModel(photoStudioService, preferencesService);
        _batchProcessingPage = new BatchProcessingViewModel(photoStudioService, preferencesService);
        _settingsPage = new SettingsViewModel(preferencesService);
        _preferencesService = preferencesService;

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

    public string BrandSubtitle { get; private set; } = string.Empty;

    public string WorkspaceLabel { get; private set; } = string.Empty;

    public string ModuleTitleText { get; private set; } = string.Empty;

    // 供菜单栏绑定：页面 VM 与活动状态
    public PhotoStudioViewModel PhotoPage => _photoStudioPage;

    public BatchProcessingViewModel BatchPage => _batchProcessingPage;

    public SettingsViewModel SettingsPage => _settingsPage;

    public bool IsPhotoPageActive => ReferenceEquals(SelectedPage, _photoStudioPage);

    public bool IsBatchPageActive => ReferenceEquals(SelectedPage, _batchProcessingPage);

    public bool IsSettingsPageActive => ReferenceEquals(SelectedPage, _settingsPage);

    public bool IsLightThemeChecked => _preferencesService.Preferences.ThemeMode == AppThemeMode.Light;

    public bool IsSystemThemeChecked => _preferencesService.Preferences.ThemeMode == AppThemeMode.System;

    protected override void RefreshLocalizedText()
    {
        WindowTitle = L(Localization.Shell.Window.Title);
        BrandSubtitle = L(Localization.Shell.Window.WorkbenchSubtitle);
        WorkspaceLabel = L(Localization.Shell.Navigation.Workspace);
        UpdateWindowSubtitle();
        UpdateModuleTitle();

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
        UpdateModuleTitle();
        UpdateActivePageState();
    }

    private void UpdateActivePageState()
    {
        OnPropertyChanged(nameof(IsPhotoPageActive));
        OnPropertyChanged(nameof(IsBatchPageActive));
        OnPropertyChanged(nameof(IsSettingsPageActive));
        OnPropertyChanged(nameof(IsLightThemeChecked));
        OnPropertyChanged(nameof(IsSystemThemeChecked));
        PhotoShowExportDialogCommand.NotifyCanExecuteChanged();
        OpenImageCommand.NotifyCanExecuteChanged();
        AddImagesCommand.NotifyCanExecuteChanged();
        PhotoRotateLeftCommand.NotifyCanExecuteChanged();
        PhotoRotateRightCommand.NotifyCanExecuteChanged();
        PhotoFlipCommand.NotifyCanExecuteChanged();
        PhotoResetCommand.NotifyCanExecuteChanged();
        PhotoToggleGuidesCommand.NotifyCanExecuteChanged();
        PhotoToggleCompareCommand.NotifyCanExecuteChanged();
        PhotoZoomCommand.NotifyCanExecuteChanged();
        BatchExportAllCommand.NotifyCanExecuteChanged();
        BatchExportZipCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private Task OpenImage() => Views.MainWindow.Instance?.OpenImagePickerAsync() ?? Task.CompletedTask;

    [RelayCommand(CanExecute = nameof(IsBatchPageActive))]
    private Task AddImages() => Views.MainWindow.Instance?.AddImagesPickerAsync() ?? Task.CompletedTask;

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoShowExportDialog() => _photoStudioPage.ShowExportDialogCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoRotateLeft() => _photoStudioPage.RotateLeftCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoRotateRight() => _photoStudioPage.RotateRightCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoFlip() => _photoStudioPage.FlipImageCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoReset() => _photoStudioPage.ResetCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoToggleGuides() => _photoStudioPage.ToggleGuidesCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoToggleCompare() => _photoStudioPage.ComparePreview = !_photoStudioPage.ComparePreview;

    [RelayCommand(CanExecute = nameof(IsPhotoPageActive))]
    private void PhotoZoom(string direction) => _photoStudioPage.SetZoomCommand.Execute(direction);

    [RelayCommand(CanExecute = nameof(IsBatchPageActive))]
    private void BatchExportAll() => _batchProcessingPage.ExportAllCommand.Execute(null);

    [RelayCommand(CanExecute = nameof(IsBatchPageActive))]
    private void BatchExportZip() => _batchProcessingPage.ExportZipCommand.Execute(null);

    [RelayCommand]
    private void NavigatePhotoStudio() => SelectedNavigationItem = NavigationItems[0];

    [RelayCommand]
    private void NavigateBatchProcessing() => SelectedNavigationItem = NavigationItems[1];

    [RelayCommand]
    private void NavigateSettings() => SelectedNavigationItem = NavigationItems[4];

    [RelayCommand]
    private void SmartCutout()
    {
        SelectedNavigationItem = NavigationItems[0];
        _photoStudioPage.IsSmartCutoutEnabled = true;
    }

    [RelayCommand]
    private void CheckUpdate()
    {
        SelectedNavigationItem = NavigationItems[4];
        _settingsPage.CheckUpdateCommand.Execute(null);
    }

    [RelayCommand]
    private void SetThemeLight()
    {
        _preferencesService.Preferences.ThemeMode = AppThemeMode.Light;
        _preferencesService.ApplyAndSave();
        UpdateActivePageState();
    }

    [RelayCommand]
    private void SetThemeSystem()
    {
        _preferencesService.Preferences.ThemeMode = AppThemeMode.System;
        _preferencesService.ApplyAndSave();
        UpdateActivePageState();
    }

    private void UpdateModuleTitle()
    {
        ModuleTitleText = SelectedPage is null
            ? BrandSubtitle
            : $"— {SelectedPage.Title}";
        OnPropertyChanged(nameof(ModuleTitleText));
        OnPropertyChanged(nameof(BrandSubtitle));
    }

    private void UpdateWindowSubtitle()
    {
        WindowSubtitle = SelectedPage?.Subtitle
            ?? L(Localization.Shell.Window.WorkbenchSubtitle);
    }
}

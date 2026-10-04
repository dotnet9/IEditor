using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Models;
using IEditor.App.Services;
using IEditor.Core.Models;
using Lang.Avalonia;

namespace IEditor.App.ViewModels;

public partial class SettingsViewModel : WorkspacePageViewModel
{
    private readonly AppPreferencesService _preferencesService;
    private readonly UpdateChecker _updateChecker = new("dotnet9", "IEditor");
    private ThemeOptionViewModel? selectedThemeOption;
    private AccentColorOptionViewModel? selectedAccentColorOption;
    private LanguageOptionViewModel? selectedLanguageOption;
    private FormatOptionViewModel? selectedFormatOption;
    private int defaultDpi;
    private PhotoOutputFormat defaultFormat;
    private uint defaultQuality;
    private string defaultSaveLocation = string.Empty;
    private string filenameTemplate = string.Empty;
    private bool showGuides;
    private bool embedSrgb;
    private bool autoCheckUpdate;
    private string currentVersionText = string.Empty;
    private string processingEngineText = string.Empty;
    private string openSourceRepositoryText = string.Empty;
    private string appearanceSectionTitle = string.Empty;
    private string imageProcessingSectionTitle = string.Empty;
    private string exportingSectionTitle = string.Empty;
    private string updateAboutSectionTitle = string.Empty;
    private string themeLabelText = string.Empty;
    private string themeDescriptionText = string.Empty;
    private string accentColorLabelText = string.Empty;
    private string accentColorDescriptionText = string.Empty;
    private string languageLabelText = string.Empty;
    private string languageDescriptionText = string.Empty;
    private string defaultDpiLabelText = string.Empty;
    private string defaultDpiDescriptionText = string.Empty;
    private string showGuidesLabelText = string.Empty;
    private string showGuidesDescriptionText = string.Empty;
    private string embedSrgbLabelText = string.Empty;
    private string embedSrgbDescriptionText = string.Empty;
    private string processingEngineLabelText = string.Empty;
    private string processingEngineDescriptionText = string.Empty;
    private string defaultSaveLocationLabelText = string.Empty;
    private string defaultSaveLocationDescriptionText = string.Empty;
    private string defaultFormatLabelText = string.Empty;
    private string defaultFormatDescriptionText = string.Empty;
    private string defaultQualityLabelText = string.Empty;
    private string defaultQualityDescriptionText = string.Empty;
    private string filenameTemplateLabelText = string.Empty;
    private string filenameTemplateDescriptionText = string.Empty;
    private string autoCheckUpdateLabelText = string.Empty;
    private string autoCheckUpdateDescriptionText = string.Empty;
    private string currentVersionLabelText = string.Empty;
    private string currentVersionDescriptionText = string.Empty;
    private string openSourceRepositoryLabelText = string.Empty;
    private string openSourceRepositoryDescriptionText = string.Empty;
    private string browseText = string.Empty;
    private string checkUpdateText = string.Empty;
    private string openRepositoryButtonText = string.Empty;
    private string versionBadgeText = "v" + (Assembly.GetEntryAssembly()?.GetName().Version is { } assemblyVersion
        ? new Version(assemblyVersion.Major, assemblyVersion.Minor, Math.Max(assemblyVersion.Build, 0)).ToString(3)
        : "0.3.0");
    private string processingEngineBadgeText = string.Empty;

    public SettingsViewModel(AppPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;

        var preferences = _preferencesService.Preferences;
        defaultDpi = preferences.DefaultDpi;
        defaultFormat = preferences.DefaultFormat;
        defaultQuality = preferences.DefaultQuality;
        defaultSaveLocation = preferences.DefaultSaveLocation;
        filenameTemplate = preferences.FilenameTemplate;
        showGuides = preferences.ShowGuides;
        embedSrgb = preferences.EmbedSrgb;
        autoCheckUpdate = preferences.AutoCheckUpdate;

        InitializeLocalizedText();
    }

    public ObservableCollection<ThemeOptionViewModel> ThemeOptions { get; private set; } = [];

    public ObservableCollection<AccentColorOptionViewModel> AccentColorOptions { get; private set; } = [];

    public ObservableCollection<LanguageOptionViewModel> LanguageOptions { get; private set; } = [];

    public ObservableCollection<FormatOptionViewModel> FormatOptions { get; private set; } = [];

    public ThemeOptionViewModel? SelectedThemeOption
    {
        get => selectedThemeOption;
        set
        {
            SelectThemeOption(value, persist: true);
        }
    }

    public AccentColorOptionViewModel? SelectedAccentColorOption
    {
        get => selectedAccentColorOption;
        set
        {
            SelectAccentColorOption(value, persist: true);
        }
    }

    public LanguageOptionViewModel? SelectedLanguageOption
    {
        get => selectedLanguageOption;
        set
        {
            SelectLanguageOption(value, persist: true);
        }
    }

    public FormatOptionViewModel? SelectedFormatOption
    {
        get => selectedFormatOption;
        set
        {
            SelectFormatOption(value, persist: true);
        }
    }

    public int DefaultDpi
    {
        get => defaultDpi;
        set
        {
            if (SetProperty(ref defaultDpi, value))
            {
                _preferencesService.Preferences.DefaultDpi = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public PhotoOutputFormat DefaultFormat
    {
        get => defaultFormat;
        set
        {
            if (SetProperty(ref defaultFormat, value))
            {
                _preferencesService.Preferences.DefaultFormat = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public uint DefaultQuality
    {
        get => defaultQuality;
        set
        {
            if (SetProperty(ref defaultQuality, value))
            {
                _preferencesService.Preferences.DefaultQuality = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public string DefaultSaveLocation
    {
        get => defaultSaveLocation;
        set
        {
            if (SetProperty(ref defaultSaveLocation, value))
            {
                _preferencesService.Preferences.DefaultSaveLocation = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public string FilenameTemplate
    {
        get => filenameTemplate;
        set
        {
            if (SetProperty(ref filenameTemplate, value))
            {
                _preferencesService.Preferences.FilenameTemplate = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public bool ShowGuides
    {
        get => showGuides;
        set
        {
            if (SetProperty(ref showGuides, value))
            {
                _preferencesService.Preferences.ShowGuides = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public bool EmbedSrgb
    {
        get => embedSrgb;
        set
        {
            if (SetProperty(ref embedSrgb, value))
            {
                _preferencesService.Preferences.EmbedSrgb = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public bool AutoCheckUpdate
    {
        get => autoCheckUpdate;
        set
        {
            if (SetProperty(ref autoCheckUpdate, value))
            {
                _preferencesService.Preferences.AutoCheckUpdate = value;
                _preferencesService.Save();
                StatusText = L(Localization.Settings.Page.AutoSaved);
            }
        }
    }

    public string CurrentVersionText
    {
        get => currentVersionText;
        private set => SetProperty(ref currentVersionText, value);
    }

    public string ProcessingEngineText
    {
        get => processingEngineText;
        private set => SetProperty(ref processingEngineText, value);
    }

    public string OpenSourceRepositoryText
    {
        get => openSourceRepositoryText;
        private set => SetProperty(ref openSourceRepositoryText, value);
    }

    public string AppearanceSectionTitle
    {
        get => appearanceSectionTitle;
        private set => SetProperty(ref appearanceSectionTitle, value);
    }

    public string ImageProcessingSectionTitle
    {
        get => imageProcessingSectionTitle;
        private set => SetProperty(ref imageProcessingSectionTitle, value);
    }

    public string ExportingSectionTitle
    {
        get => exportingSectionTitle;
        private set => SetProperty(ref exportingSectionTitle, value);
    }

    public string UpdateAboutSectionTitle
    {
        get => updateAboutSectionTitle;
        private set => SetProperty(ref updateAboutSectionTitle, value);
    }

    public string ThemeLabelText
    {
        get => themeLabelText;
        private set => SetProperty(ref themeLabelText, value);
    }

    public string ThemeDescriptionText
    {
        get => themeDescriptionText;
        private set => SetProperty(ref themeDescriptionText, value);
    }

    public string AccentColorLabelText
    {
        get => accentColorLabelText;
        private set => SetProperty(ref accentColorLabelText, value);
    }

    public string AccentColorDescriptionText
    {
        get => accentColorDescriptionText;
        private set => SetProperty(ref accentColorDescriptionText, value);
    }

    public string LanguageLabelText
    {
        get => languageLabelText;
        private set => SetProperty(ref languageLabelText, value);
    }

    public string LanguageDescriptionText
    {
        get => languageDescriptionText;
        private set => SetProperty(ref languageDescriptionText, value);
    }

    public string DefaultDpiLabelText
    {
        get => defaultDpiLabelText;
        private set => SetProperty(ref defaultDpiLabelText, value);
    }

    public string DefaultDpiDescriptionText
    {
        get => defaultDpiDescriptionText;
        private set => SetProperty(ref defaultDpiDescriptionText, value);
    }

    public string ShowGuidesLabelText
    {
        get => showGuidesLabelText;
        private set => SetProperty(ref showGuidesLabelText, value);
    }

    public string ShowGuidesDescriptionText
    {
        get => showGuidesDescriptionText;
        private set => SetProperty(ref showGuidesDescriptionText, value);
    }

    public string EmbedSrgbLabelText
    {
        get => embedSrgbLabelText;
        private set => SetProperty(ref embedSrgbLabelText, value);
    }

    public string EmbedSrgbDescriptionText
    {
        get => embedSrgbDescriptionText;
        private set => SetProperty(ref embedSrgbDescriptionText, value);
    }

    public string ProcessingEngineLabelText
    {
        get => processingEngineLabelText;
        private set => SetProperty(ref processingEngineLabelText, value);
    }

    public string ProcessingEngineDescriptionText
    {
        get => processingEngineDescriptionText;
        private set => SetProperty(ref processingEngineDescriptionText, value);
    }

    public string DefaultSaveLocationLabelText
    {
        get => defaultSaveLocationLabelText;
        private set => SetProperty(ref defaultSaveLocationLabelText, value);
    }

    public string DefaultSaveLocationDescriptionText
    {
        get => defaultSaveLocationDescriptionText;
        private set => SetProperty(ref defaultSaveLocationDescriptionText, value);
    }

    public string DefaultFormatLabelText
    {
        get => defaultFormatLabelText;
        private set => SetProperty(ref defaultFormatLabelText, value);
    }

    public string DefaultFormatDescriptionText
    {
        get => defaultFormatDescriptionText;
        private set => SetProperty(ref defaultFormatDescriptionText, value);
    }

    public string DefaultQualityLabelText
    {
        get => defaultQualityLabelText;
        private set => SetProperty(ref defaultQualityLabelText, value);
    }

    public string DefaultQualityDescriptionText
    {
        get => defaultQualityDescriptionText;
        private set => SetProperty(ref defaultQualityDescriptionText, value);
    }

    public string FilenameTemplateLabelText
    {
        get => filenameTemplateLabelText;
        private set => SetProperty(ref filenameTemplateLabelText, value);
    }

    public string FilenameTemplateDescriptionText
    {
        get => filenameTemplateDescriptionText;
        private set => SetProperty(ref filenameTemplateDescriptionText, value);
    }

    public string AutoCheckUpdateLabelText
    {
        get => autoCheckUpdateLabelText;
        private set => SetProperty(ref autoCheckUpdateLabelText, value);
    }

    public string AutoCheckUpdateDescriptionText
    {
        get => autoCheckUpdateDescriptionText;
        private set => SetProperty(ref autoCheckUpdateDescriptionText, value);
    }

    public string CurrentVersionLabelText
    {
        get => currentVersionLabelText;
        private set => SetProperty(ref currentVersionLabelText, value);
    }

    public string CurrentVersionDescriptionText
    {
        get => currentVersionDescriptionText;
        private set => SetProperty(ref currentVersionDescriptionText, value);
    }

    public string OpenSourceRepositoryLabelText
    {
        get => openSourceRepositoryLabelText;
        private set => SetProperty(ref openSourceRepositoryLabelText, value);
    }

    public string OpenSourceRepositoryDescriptionText
    {
        get => openSourceRepositoryDescriptionText;
        private set => SetProperty(ref openSourceRepositoryDescriptionText, value);
    }

    public string BrowseText
    {
        get => browseText;
        private set => SetProperty(ref browseText, value);
    }

    public string CheckUpdateText
    {
        get => checkUpdateText;
        private set => SetProperty(ref checkUpdateText, value);
    }

    public string OpenRepositoryButtonText
    {
        get => openRepositoryButtonText;
        private set => SetProperty(ref openRepositoryButtonText, value);
    }

    public string VersionBadgeText
    {
        get => versionBadgeText;
        private set => SetProperty(ref versionBadgeText, value);
    }

    public string ProcessingEngineBadgeText
    {
        get => processingEngineBadgeText;
        private set => SetProperty(ref processingEngineBadgeText, value);
    }

    [RelayCommand]
    private void OpenSaveLocation(IReadOnlyList<string>? paths)
    {
        var path = paths?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        DefaultSaveLocation = path;
    }

    [RelayCommand]
    private void SelectTheme(ThemeOptionViewModel? option)
    {
        SelectedThemeOption = option;
    }

    [RelayCommand]
    private void SelectAccentColor(AccentColorOptionViewModel? option)
    {
        SelectedAccentColorOption = option;
    }

    [RelayCommand]
    private void SelectLanguage(LanguageOptionViewModel? option)
    {
        SelectedLanguageOption = option;
    }

    [RelayCommand]
    private void SelectFormat(FormatOptionViewModel? option)
    {
        SelectedFormatOption = option;
    }

    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        Version current = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 3, 0);
        StatusText = L(Localization.Common.States.CheckingUpdate);
        UpdateCheckResult result = await _updateChecker.CheckAsync(current);
        if (!result.Succeeded)
        {
            StatusText = string.Format(
                CultureInfo.CurrentCulture,
                L(Localization.Common.States.UpdateCheckFailed),
                result.Error);
            return;
        }

        if (result.Update is { } update)
        {
            // 仅提醒不自动下载：提示并打开发布页
            StatusText = string.Format(
                CultureInfo.CurrentCulture,
                L(Localization.Common.States.UpdateAvailable),
                update.Tag);
            OpenUrl(update.PageUrl);
            return;
        }

        StatusText = L(Localization.Common.States.UpToDate);
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            StatusText = L(Localization.Common.States.ComingSoon);
        }
    }

    [RelayCommand]
    private void AdjustDefaultDpi(string direction)
    {
        DefaultDpi = Math.Clamp(DefaultDpi + (direction == "+" ? 30 : -30), 72, 1200);
    }

    [RelayCommand]
    private void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/dotnet9/IEditor",
                UseShellExecute = true
            });
            StatusText = L(Localization.Common.States.Ready);
        }
        catch
        {
            StatusText = L(Localization.Common.States.ComingSoon);
        }
    }

    protected override void RefreshLocalizedText()
    {
        Title = L(Localization.Shell.Navigation.Settings);
        Subtitle = L(Localization.Settings.Page.Subtitle);
        Breadcrumb = L(Localization.Settings.Page.Breadcrumb);
        StatusText = L(Localization.Settings.Page.AutoSaved);
        AppearanceSectionTitle = L(Localization.Settings.Page.Appearance);
        ImageProcessingSectionTitle = L(Localization.Settings.Page.ImageProcessing);
        ExportingSectionTitle = L(Localization.Settings.Page.Exporting);
        UpdateAboutSectionTitle = L(Localization.Settings.Page.UpdateAbout);
        ThemeLabelText = L(Localization.Settings.Labels.Theme);
        ThemeDescriptionText = L(Localization.Settings.Descriptions.Theme);
        AccentColorLabelText = L(Localization.Settings.Labels.AccentColor);
        AccentColorDescriptionText = L(Localization.Settings.Descriptions.AccentColor);
        LanguageLabelText = L(Localization.Settings.Labels.Language);
        LanguageDescriptionText = L(Localization.Settings.Descriptions.Language);
        DefaultDpiLabelText = L(Localization.Settings.Labels.DefaultDpi);
        DefaultDpiDescriptionText = L(Localization.Settings.Descriptions.DefaultDpi);
        ShowGuidesLabelText = L(Localization.Settings.Labels.ShowGuides);
        ShowGuidesDescriptionText = L(Localization.Settings.Descriptions.ShowGuides);
        EmbedSrgbLabelText = L(Localization.Settings.Labels.EmbedSrgb);
        EmbedSrgbDescriptionText = L(Localization.Settings.Descriptions.EmbedSrgb);
        ProcessingEngineLabelText = L(Localization.Settings.Labels.ProcessingEngine);
        ProcessingEngineDescriptionText = L(Localization.Settings.Descriptions.ProcessingEngine);
        DefaultSaveLocationLabelText = L(Localization.Settings.Labels.DefaultSaveLocation);
        DefaultSaveLocationDescriptionText = L(Localization.Settings.Descriptions.DefaultSaveLocation);
        DefaultFormatLabelText = L(Localization.Settings.Labels.DefaultFormat);
        DefaultFormatDescriptionText = L(Localization.Settings.Descriptions.DefaultFormat);
        DefaultQualityLabelText = L(Localization.Settings.Labels.DefaultQuality);
        DefaultQualityDescriptionText = L(Localization.Settings.Descriptions.DefaultQuality);
        FilenameTemplateLabelText = L(Localization.Settings.Labels.FilenameTemplate);
        FilenameTemplateDescriptionText = L(Localization.Settings.Descriptions.FilenameTemplate);
        AutoCheckUpdateLabelText = L(Localization.Settings.Labels.AutoCheckUpdate);
        AutoCheckUpdateDescriptionText = L(Localization.Settings.Descriptions.AutoCheckUpdate);
        CurrentVersionLabelText = L(Localization.Settings.Labels.CurrentVersion);
        CurrentVersionDescriptionText = L(Localization.Settings.Descriptions.CurrentVersion);
        OpenSourceRepositoryLabelText = L(Localization.Settings.Labels.OpenSourceRepository);
        OpenSourceRepositoryDescriptionText = L(Localization.Settings.Descriptions.OpenSourceRepository);
        BrowseText = L(Localization.Common.Actions.Browse);
        CheckUpdateText = L(Localization.Common.Actions.CheckUpdate);
        OpenRepositoryButtonText = L(Localization.Common.Actions.OpenRepository);
        VersionBadgeText = "v0.3.0";
        ProcessingEngineBadgeText = "Magick.NET · 内置";
        CurrentVersionText = L(Localization.Settings.Descriptions.CurrentVersion);
        ProcessingEngineText = L(Localization.Settings.Descriptions.ProcessingEngine);
        OpenSourceRepositoryText = L(Localization.Settings.Descriptions.OpenSourceRepository);
        BuildOptions();
    }

    private void BuildOptions()
    {
        var preferences = _preferencesService.Preferences;

        ThemeOptions =
        [
            new ThemeOptionViewModel(L(Localization.Common.Theme.Light), AppThemeMode.Light),
            new ThemeOptionViewModel(L(Localization.Common.Theme.Dark), AppThemeMode.Dark, false),
            new ThemeOptionViewModel(L(Localization.Common.Theme.System), AppThemeMode.System)
        ];

        AccentColorOptions =
        [
            new AccentColorOptionViewModel(L(Localization.Settings.Options.TechBlue), new RgbColor(46, 107, 255)),
            new AccentColorOptionViewModel(L(Localization.Settings.Options.AuroraCyan), new RgbColor(0, 184, 217)),
            new AccentColorOptionViewModel(L(Localization.Settings.Options.NebulaPurple), new RgbColor(124, 92, 255)),
            new AccentColorOptionViewModel(L(Localization.Settings.Options.Green), new RgbColor(18, 183, 106)),
            new AccentColorOptionViewModel(L(Localization.Settings.Options.Amber), new RgbColor(245, 158, 11))
        ];

        LanguageOptions =
        [
            new LanguageOptionViewModel(L(Localization.Common.Language.SimplifiedChinese), "zh-CN")
        ];

        FormatOptions =
        [
            new FormatOptionViewModel(L(Localization.Common.Formats.Jpg), PhotoOutputFormat.Jpeg),
            new FormatOptionViewModel(L(Localization.Common.Formats.Png), PhotoOutputFormat.Png)
        ];

        SelectThemeOption(ThemeOptions.FirstOrDefault(item => item.Value == preferences.ThemeMode) ?? ThemeOptions[0], persist: false);
        SelectAccentColorOption(AccentColorOptions.FirstOrDefault(item => item.Color == preferences.AccentColor) ?? AccentColorOptions[0], persist: false);
        SelectLanguageOption(LanguageOptions.FirstOrDefault(item => item.CultureName == preferences.Language) ?? LanguageOptions[0], persist: false);
        SelectFormatOption(FormatOptions.FirstOrDefault(item => item.Value == preferences.DefaultFormat) ?? FormatOptions[0], persist: false);

        OnPropertyChanged(nameof(ThemeOptions));
        OnPropertyChanged(nameof(AccentColorOptions));
        OnPropertyChanged(nameof(LanguageOptions));
        OnPropertyChanged(nameof(FormatOptions));
    }

    private void SelectThemeOption(ThemeOptionViewModel? value, bool persist)
    {
        if (SetProperty(ref selectedThemeOption, value))
        {
            foreach (var option in ThemeOptions)
            {
                option.IsSelected = option == value;
            }
        }

        if (persist && value is not null && value.IsEnabled)
        {
            _preferencesService.Preferences.ThemeMode = value.Value;
            _preferencesService.ApplyAndSave();
            StatusText = L(Localization.Settings.Page.AutoSaved);
        }
    }

    private void SelectAccentColorOption(AccentColorOptionViewModel? value, bool persist)
    {
        if (SetProperty(ref selectedAccentColorOption, value))
        {
            foreach (var option in AccentColorOptions)
            {
                option.IsSelected = option == value;
            }
        }

        if (persist && value is not null)
        {
            _preferencesService.Preferences.AccentColor = value.Color;
            _preferencesService.ApplyAndSave();
            StatusText = L(Localization.Settings.Page.AutoSaved);
        }
    }

    private void SelectLanguageOption(LanguageOptionViewModel? value, bool persist)
    {
        if (SetProperty(ref selectedLanguageOption, value))
        {
            foreach (var option in LanguageOptions)
            {
                option.IsSelected = option == value;
            }
        }

        if (persist && value is not null)
        {
            _preferencesService.Preferences.Language = value.CultureName;
            I18nManager.Instance.Culture = new CultureInfo(value.CultureName);
            _preferencesService.Save();
            StatusText = L(Localization.Settings.Page.AutoSaved);
        }
    }

    private void SelectFormatOption(FormatOptionViewModel? value, bool persist)
    {
        if (SetProperty(ref selectedFormatOption, value))
        {
            foreach (var option in FormatOptions)
            {
                option.IsSelected = option == value;
            }
        }

        if (persist && value is not null)
        {
            DefaultFormat = value.Value;
        }
    }
}

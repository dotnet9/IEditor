using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IEditor.App.Services;
using IEditor.App.ViewModels;
using IEditor.App.Views;
using IEditor.Core.Services;
using Lang.Avalonia;
using Lang.Avalonia.Json;
using System.Globalization;
using System.IO;

namespace IEditor.App;

public partial class App : Application
{
    public static AppPreferencesService PreferencesService { get; private set; } = null!;

    public static IPhotoStudioService PhotoStudioService { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterLocalization();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var preferencesStore = new AppPreferencesStore();
            var themeManager = new AppThemeManager();
            PreferencesService = new AppPreferencesService(preferencesStore, themeManager);
            PreferencesService.Apply();

            I18nManager.Instance.Culture = new CultureInfo(PreferencesService.Preferences.Language);
            PhotoStudioService = new MagickPhotoStudioService();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(PhotoStudioService, PreferencesService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterLocalization()
    {
        var langPlugin = new JsonLangPlugin
        {
            ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n")
        };

        I18nManager.Instance.Register(langPlugin, new CultureInfo("zh-CN"), out _);
    }
}

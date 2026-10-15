using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IEditor.App.Services;
using IEditor.App.ViewModels;
using IEditor.App.Views;
using IEditor.Core.Services;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
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
        // macOS 标准应用包把宿主资源放在 Contents/Resources，其他部署使用相邻目录。
        var bundledResources = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources", "I18n"));
        var langPlugin = new JsonLangPlugin
        {
            ResourceFolder = OperatingSystem.IsMacOS() && Directory.Exists(bundledResources)
                ? bundledResources : Path.Combine(AppContext.BaseDirectory, "I18n")
        };

        I18nManager.Instance.Register(langPlugin, new CultureInfo("zh-CN"), out _);
    }
}

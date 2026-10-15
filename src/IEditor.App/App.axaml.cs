using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        ApplyControlCulture();
    }

    private static void OnCultureChanged(object? sender, EventArgs args)
    {
        if (Dispatcher.UIThread.CheckAccess()) ApplyControlCulture();
        else Dispatcher.UIThread.Post(ApplyControlCulture);
    }

    /// <summary>更新已加载主题的内建文案，保留应用资源和局部资源的同键覆盖。</summary>
    private static void ApplyControlCulture()
    {
        if (Application.Current is not { } application || I18nManager.Instance.Culture is not { } culture) return;
        foreach (var theme in application.Styles.OfType<Semi.Avalonia.SemiTheme>()) theme.Locale = culture;
        foreach (var theme in application.Styles.OfType<Ursa.Themes.Semi.UrsaSemiTheme>()) theme.Locale = culture;
    }
}

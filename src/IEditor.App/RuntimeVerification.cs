using System.Globalization;
using System.Runtime.CompilerServices;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using IEditor.App.Models;
using IEditor.App.Services;

namespace IEditor.App;

/// <summary>显式验证正式原生产物的翻译和隔离偏好，不创建窗口或访问用户图片。</summary>
internal static class RuntimeVerification
{
    public static void Run(string directory)
    {
        if (RuntimeFeature.IsDynamicCodeSupported)
            throw new InvalidOperationException("Runtime verification requires a NativeAOT executable.");
        var root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        var resources = Path.Combine(AppContext.BaseDirectory, "I18n");
        if (OperatingSystem.IsMacOS() && !Directory.Exists(resources))
            resources = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources", "I18n"));
        var manager = I18nManager.Instance;
        manager.Register(new JsonLangPlugin { ResourceFolder = resources }, CultureInfo.GetCultureInfo("en-US"));
        var labels = new HashSet<string>();
        foreach (var culture in new[] { "en-US", "zh-CN" })
        {
            if (!File.Exists(Path.Combine(resources, "IEditor.App", culture + ".json")))
                throw new FileNotFoundException("Published language file is missing: " + culture);
            manager.Culture = CultureInfo.GetCultureInfo(culture);
            var text = manager.GetResource(Localization.Settings.Labels.Language);
            if (string.IsNullOrWhiteSpace(text) || text == Localization.Settings.Labels.Language)
                throw new InvalidOperationException("Published language resources are missing: " + culture);
            labels.Add(text);
        }
        if (labels.Count != 2) throw new InvalidOperationException("Published languages do not switch.");
        var preferencesPath = Path.Combine(root, "runtime-" + Guid.NewGuid().ToString("N") + ".json");
        new AppPreferencesStore(preferencesPath).Save(new AppPreferences
        {
            ThemeMode = AppThemeMode.System,
            Palette = StudioPalette.Aurora,
            Language = "en-US",
            DefaultSaveLocation = Path.Combine(root, "exports"),
            AutoCheckUpdate = false
        });
        var saved = new AppPreferencesStore(preferencesPath).Load();
        if (saved.ThemeMode != AppThemeMode.System || saved.Palette != StudioPalette.Aurora || saved.Language != "en-US")
            throw new InvalidOperationException("Native appearance preferences did not round-trip.");
        var result = $"NativeAOT runtime verification passed; version={typeof(Program).Assembly.GetName().Version}; languages=2; preferences=isolated";
        File.WriteAllText(Path.Combine(root, "runtime-verification.txt"), result);
        Console.WriteLine(result);
    }
}

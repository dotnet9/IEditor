using System;
using System.IO;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using IEditor.Core.Models;

namespace IEditor.App.Models;

public partial class AppPreferences : ObservableObject
{
    private static string GetDefaultSaveLocation()
    {
        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        return string.IsNullOrWhiteSpace(pictures)
            ? Path.Combine(Environment.CurrentDirectory, "IEditor")
            : Path.Combine(pictures, "IEditor");
    }

    [ObservableProperty]
    private AppThemeMode themeMode = AppThemeMode.Light;

    [ObservableProperty]
    private RgbColor accentColor = new(46, 107, 255);

    [ObservableProperty]
    private string language = "zh-CN";

    [ObservableProperty]
    private int defaultDpi = 300;

    [ObservableProperty]
    private PhotoOutputFormat defaultFormat = PhotoOutputFormat.Jpeg;

    [ObservableProperty]
    private uint defaultQuality = 92;

    [ObservableProperty]
    private string defaultSaveLocation = GetDefaultSaveLocation();

    [ObservableProperty]
    private string filenameTemplate = "证件照_{规格}_{底色}";

    [ObservableProperty]
    private bool showGuides = true;

    [ObservableProperty]
    private bool embedSrgb = true;

    [ObservableProperty]
    private bool autoCheckUpdate = true;

    /// <summary>上次自动检查更新的时间（UTC）；「每周检查一次」的节流依据。</summary>
    [ObservableProperty]
    private DateTime? lastAutoCheckUpdateAt;

    [ObservableProperty]
    private int batchConcurrency = 4;

    [ObservableProperty]
    private bool batchSkipProcessed = true;

    [ObservableProperty]
    private bool batchOpenFolderWhenDone = true;
}

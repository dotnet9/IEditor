using System;
using System.IO;
using CodeWF.Avalonia.Controls.Themes;
using IEditor.Core.Models;

namespace IEditor.App.Models;

public sealed class AppPreferences
{
    private static string GetDefaultSaveLocation()
    {
        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        return string.IsNullOrWhiteSpace(pictures)
            ? Path.Combine(Environment.CurrentDirectory, "IEditor")
            : Path.Combine(pictures, "IEditor");
    }

    public AppThemeMode ThemeMode { get; set; } = AppThemeMode.Light;

    public StudioPalette Palette { get; set; } = StudioPalette.DotNet;

    public RgbColor AccentColor { get; set; } = new(46, 107, 255);

    public string Language { get; set; } = "zh-CN";

    public int DefaultDpi { get; set; } = 300;

    public PhotoOutputFormat DefaultFormat { get; set; } = PhotoOutputFormat.Jpeg;

    public uint DefaultQuality { get; set; } = 92;

    public string DefaultSaveLocation { get; set; } = GetDefaultSaveLocation();

    public string FilenameTemplate { get; set; } = "photo_{size}_{color}";

    public bool ShowGuides { get; set; } = true;

    public bool EmbedSrgb { get; set; } = true;

    public bool AutoCheckUpdate { get; set; } = true;

    /// <summary>上次自动检查更新的时间（UTC）；「每周检查一次」的节流依据。</summary>
    public DateTime? LastAutoCheckUpdateAt { get; set; }

    public int BatchConcurrency { get; set; } = 4;

    public bool BatchSkipProcessed { get; set; } = true;

    public bool BatchOpenFolderWhenDone { get; set; } = true;
}

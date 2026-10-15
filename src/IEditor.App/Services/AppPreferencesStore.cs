using System;
using System.IO;
using System.Text.Json;
using IEditor.App.Models;

namespace IEditor.App.Services;

public sealed class AppPreferencesStore
{
    private readonly string _filePath;

    public AppPreferencesStore(string? filePath = null)
    {
        if (filePath is not null)
        {
            _filePath = Path.GetFullPath(filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            return;
        }

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IEditor");

        // 旧版偏好存在 Roaming（%APPDATA% 下），首次升级一次性迁到 Local（应用数据标准位置），旧目录保留作备份
        var legacyFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IEditor");
        if (Directory.Exists(legacyFolder) && !Directory.Exists(folder))
        {
            try
            {
                CopyDirectory(legacyFolder, folder);
            }
            catch
            {
                // 迁移失败不阻塞启动，旧目录原样保留
            }
        }

        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "preferences.json");
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: false);
        }
    }

    public AppPreferences Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new AppPreferences();
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize(json, PreferencesJsonContext.Default.AppPreferences) ?? new AppPreferences();
        }
        catch
        {
            return new AppPreferences();
        }
    }

    public void Save(AppPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var json = JsonSerializer.Serialize(preferences, PreferencesJsonContext.Default.AppPreferences);
        File.WriteAllText(_filePath, json);
    }
}

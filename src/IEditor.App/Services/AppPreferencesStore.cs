using System;
using System.IO;
using System.Text.Json;
using IEditor.App.Models;

namespace IEditor.App.Services;

public sealed class AppPreferencesStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public AppPreferencesStore()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IEditor");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "preferences.json");
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
            return JsonSerializer.Deserialize<AppPreferences>(json, SerializerOptions) ?? new AppPreferences();
        }
        catch
        {
            return new AppPreferences();
        }
    }

    public void Save(AppPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var json = JsonSerializer.Serialize(preferences, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }
}

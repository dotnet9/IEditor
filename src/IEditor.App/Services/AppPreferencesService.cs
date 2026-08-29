using IEditor.App.Models;

namespace IEditor.App.Services;

public sealed class AppPreferencesService
{
    private readonly AppPreferencesStore _store;
    private readonly AppThemeManager _themeManager;

    public AppPreferencesService(AppPreferencesStore store, AppThemeManager themeManager)
    {
        _store = store;
        _themeManager = themeManager;
        Preferences = _store.Load();
    }

    public AppPreferences Preferences { get; }

    public void Apply() => _themeManager.Apply(Preferences);

    public void Save() => _store.Save(Preferences);

    public void ApplyAndSave()
    {
        Apply();
        Save();
    }
}

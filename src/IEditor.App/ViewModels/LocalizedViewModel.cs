using CommunityToolkit.Mvvm.ComponentModel;
using CodeWF.Avalonia.Lang;

namespace IEditor.App.ViewModels;

public abstract partial class LocalizedViewModel : ObservableObject, IDisposable
{
    private bool _disposed;

    protected LocalizedViewModel()
    {
        I18nManager.Instance.CultureChanged += OnCultureChanged;
    }

    protected abstract void RefreshLocalizedText();

    protected string L(string key) => I18nManager.Instance.GetResource(key);

    public void Refresh() => RefreshLocalizedText();

    protected void InitializeLocalizedText() => RefreshLocalizedText();

    protected virtual void OnDisposed()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        OnDisposed();
    }

    private void OnCultureChanged(object? sender, EventArgs e) => RefreshLocalizedText();
}

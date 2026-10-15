using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CodeWF.Toolkit.Lang;

namespace IEditor.App.ViewModels;

public abstract partial class LocalizedViewModel : ObservableObject, IDisposable
{
    private bool _disposed;

    protected LocalizedViewModel()
    {
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        I18nManager.Instance.ResourcesChanged += OnCultureChanged;
    }

    protected abstract void RefreshLocalizedText();

    protected string L(string key) => I18nManager.Instance.GetResource(key);

    public void Refresh()
    {
        RefreshLocalizedText();
        OnPropertyChanged(string.Empty);
    }

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
        I18nManager.Instance.ResourcesChanged -= OnCultureChanged;
        OnDisposed();
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess()) Refresh();
        else Dispatcher.UIThread.Post(Refresh);
    }
}

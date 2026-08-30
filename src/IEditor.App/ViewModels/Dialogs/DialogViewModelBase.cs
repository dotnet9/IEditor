using System;
using Irihi.Avalonia.Shared.Contracts;

namespace IEditor.App.ViewModels.Dialogs;

public abstract partial class DialogViewModelBase : LocalizedViewModel, IDialogContext
{
    private event EventHandler<object?>? _requestClose;

    event EventHandler<object?>? IDialogContext.RequestClose
    {
        add => _requestClose += value;
        remove => _requestClose -= value;
    }

    void IDialogContext.Close()
    {
        RequestClose();
    }

    protected void RequestClose(object? result = null)
    {
        _requestClose?.Invoke(this, result);
    }

    protected override void OnDisposed()
    {
        _requestClose = null;
        base.OnDisposed();
    }
}

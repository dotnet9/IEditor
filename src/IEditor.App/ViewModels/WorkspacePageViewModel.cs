namespace IEditor.App.ViewModels;

public abstract partial class WorkspacePageViewModel : LocalizedViewModel
{
    private string _title = string.Empty;
    private string _subtitle = string.Empty;
    private string _breadcrumb = string.Empty;
    private string _statusText = string.Empty;

    public string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        protected set => SetProperty(ref _subtitle, value);
    }

    public string Breadcrumb
    {
        get => _breadcrumb;
        protected set => SetProperty(ref _breadcrumb, value);
    }

    public string StatusText
    {
        get => _statusText;
        protected set => SetProperty(ref _statusText, value);
    }
}

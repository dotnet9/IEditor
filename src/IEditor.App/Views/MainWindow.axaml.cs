using Ursa.Controls;

namespace IEditor.App.Views;

public partial class MainWindow : UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnClosed(e);
    }
}

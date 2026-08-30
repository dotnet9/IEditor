using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IEditor.App.ViewModels.Dialogs;

namespace IEditor.App.Views.Dialogs;

public partial class ExportDialogView : UserControl
{
    public ExportDialogView()
    {
        InitializeComponent();
    }

    private ExportDialogViewModel? ViewModel => DataContext as ExportDialogViewModel;

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = ViewModel;
        var topLevel = TopLevel.GetTopLevel(this);
        if (viewModel is null || topLevel is null)
        {
            return;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            viewModel.OutputFolder = folders[0].Path.LocalPath;
        }
    }
}

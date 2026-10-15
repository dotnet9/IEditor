using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CodeWF.Avalonia.Controls.Controls;
using IEditor.App.ViewModels;

namespace IEditor.App.Views;

public partial class MainWindow : CodeWFWindow
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }

    private AboutWindow? _aboutWindow;

    public void ShowAbout()
    {
        if (_aboutWindow is not null) { _aboutWindow.Activate(); return; }
        _aboutWindow = new AboutWindow { DataContext = DataContext };
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _ = _aboutWindow.ShowDialog(this);
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnExitClick(object? sender, RoutedEventArgs e) => Close();

    private async void OnOpenImageClick(object? sender, RoutedEventArgs e) => await OpenImagePickerAsync();

    private async void OnAddImagesClick(object? sender, RoutedEventArgs e) => await AddImagesPickerAsync();

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        var photoPage = ViewModel?.PhotoPage;
        if (photoPage is null)
        {
            return;
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            photoPage.OutputFolder = folders[0].Path.LocalPath;
        }
    }

    public async Task OpenImagePickerAsync()
    {
        var photoPage = ViewModel?.PhotoPage;
        if (photoPage is null)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = photoPage.LoadImageText,
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count > 0)
        {
            photoPage.SetSourcePath(files[0].Path.LocalPath);
        }
    }

    public async Task AddImagesPickerAsync()
    {
        var batchPage = ViewModel?.BatchPage;
        if (batchPage is null)
        {
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = batchPage.AddImageText,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count > 0)
        {
            batchPage.AddImagePaths(files.Select(file => file.Path.LocalPath));
        }
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

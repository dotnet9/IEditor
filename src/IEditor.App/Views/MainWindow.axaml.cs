using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IEditor.App.ViewModels;

namespace IEditor.App.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeClick(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

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

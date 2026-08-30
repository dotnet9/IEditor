using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using IEditor.App.ViewModels;

namespace IEditor.App.Views;

public partial class BatchProcessingView : UserControl
{
    public BatchProcessingView()
    {
        InitializeComponent();
    }

    private BatchProcessingViewModel? ViewModel => DataContext as BatchProcessingViewModel;

    private async void OnAddImagesClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = ViewModel;
        var topLevel = TopLevel.GetTopLevel(this);
        if (viewModel is null || topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = viewModel.AddImageText,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count > 0)
        {
            viewModel.AddImagePaths(files.Select(file => file.Path.LocalPath));
        }
    }
}

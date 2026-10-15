using Avalonia.Input;
using System.IO;
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
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, (_, args) => args.DragEffects = args.DataTransfer.TryGetFiles() is null ? DragDropEffects.None : DragDropEffects.Copy);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs args)
    {
        var paths = args.DataTransfer.TryGetFiles()?.Select(item => item.TryGetLocalPath()).OfType<string>().ToArray() ?? [];
        ViewModel?.AddImagePaths(paths);
        args.Handled = true;
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

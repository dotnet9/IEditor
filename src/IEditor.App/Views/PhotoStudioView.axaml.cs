using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using IEditor.App.ViewModels;

namespace IEditor.App.Views;

public partial class PhotoStudioView : UserControl
{
    public PhotoStudioView()
    {
        InitializeComponent();
        Loaded += OnViewLoaded;
    }

    private PhotoStudioViewModel? ViewModel => DataContext as PhotoStudioViewModel;

    private void OnViewLoaded(object? sender, RoutedEventArgs e) => StartScanline();

    private void StartScanline()
    {
        // TransformAnimator 要求目标是 Visual：在矩形上动画其 TranslateTransform.Y
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3.4),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter { Property = TranslateTransform.YProperty, Value = -70d } }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.55),
                    Setters = { new Setter { Property = TranslateTransform.YProperty, Value = 470d } }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter { Property = TranslateTransform.YProperty, Value = 470d } }
                }
            }
        };
        _ = animation.RunAsync(ScanlineRect);
    }

    private async void OnLoadImageClick(object? sender, RoutedEventArgs e) => await PickSourceImageAsync();

    private async Task PickSourceImageAsync()
    {
        var viewModel = ViewModel;
        var topLevel = TopLevel.GetTopLevel(this);
        if (viewModel is null || topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = viewModel.LoadImageText,
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count > 0)
        {
            viewModel.SetSourcePath(files[0].Path.LocalPath);
        }
    }

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

    private void OnOpenColorPickerClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        if (control.GetValue(FlyoutBase.AttachedFlyoutProperty) is PopupFlyoutBase flyout)
        {
            flyout.ShowAt(control);
        }
    }

    private void OnComparePressed(object? sender, PointerPressedEventArgs e) => ViewModel?.SetCompare(true);

    private void OnCompareReleased(object? sender, PointerReleasedEventArgs e) => ViewModel?.SetCompare(false);

    private void OnCompareLeft(object? sender, PointerEventArgs e) => ViewModel?.SetCompare(false);
}

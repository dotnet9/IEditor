using Avalonia;
using System;
using System.ComponentModel;
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
    private readonly ScaleTransform previewImageScaleTransform = new(1d, 1d);
    private readonly TranslateTransform previewImageTranslateTransform = new();
    private bool isDraggingPreview;
    private Point previewDragLastPoint;
    private PhotoStudioViewModel? subscribedViewModel;

    public PhotoStudioView()
    {
        InitializeComponent();
        PreviewImage.RenderTransform = new TransformGroup
        {
            Children =
            {
                previewImageScaleTransform,
                previewImageTranslateTransform
            }
        };
        Loaded += OnViewLoaded;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        DataContextChanged += OnDataContextChanged;
    }

    private PhotoStudioViewModel? ViewModel => DataContext as PhotoStudioViewModel;

    private void OnViewLoaded(object? sender, RoutedEventArgs e) => StartScanline();

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        subscribedViewModel?.CancelPreviewInteraction();
        if (subscribedViewModel is not null)
        {
            subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            subscribedViewModel = null;
        }

        isDraggingPreview = false;
        ResetPreviewImageTransform();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        subscribedViewModel?.CancelPreviewInteraction();
        if (subscribedViewModel is not null)
        {
            subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        subscribedViewModel = ViewModel;
        if (subscribedViewModel is not null)
        {
            subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        ResetPreviewImageTransform();
    }

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

    private void OnPhotoStagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control || ViewModel is null || string.IsNullOrWhiteSpace(ViewModel.SourcePath))
        {
            return;
        }

        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        isDraggingPreview = true;
        ViewModel.BeginPreviewInteraction();
        previewDragLastPoint = e.GetPosition(control);
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void OnPhotoStagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isDraggingPreview || sender is not Control control || ViewModel is null)
        {
            return;
        }

        var point = e.GetPosition(control);
        var delta = point - previewDragLastPoint;
        var width = Math.Max(control.Bounds.Width, 1);
        var height = Math.Max(control.Bounds.Height, 1);
        var scaleX = ViewModel.CurrentSize.WidthPx / width;
        var scaleY = ViewModel.CurrentSize.HeightPx / height;

        previewImageTranslateTransform.X += delta.X;
        previewImageTranslateTransform.Y += delta.Y;
        ViewModel.MovePreviewOffset(delta.X * scaleX, delta.Y * scaleY);
        previewDragLastPoint = point;
        e.Handled = true;
    }

    private void OnPhotoStagePointerReleased(object? sender, PointerReleasedEventArgs e) => EndPreviewDrag(sender, e.Pointer, true);

    private void OnPhotoStagePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => EndPreviewDrag(sender, e.Pointer, false);

    private void EndPreviewDrag(object? sender, IPointer pointer, bool releaseCapture)
    {
        if (!isDraggingPreview)
        {
            return;
        }

        isDraggingPreview = false;
        ViewModel?.EndPreviewInteraction();

        if (releaseCapture)
        {
            pointer.Capture(null);
        }
    }

    private void OnPhotoStagePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ViewModel is null || string.IsNullOrWhiteSpace(ViewModel.SourcePath))
        {
            return;
        }

        ViewModel.AdjustPreviewZoom(e.Delta.Y > 0 ? 10 : -10);
        e.Handled = true;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PhotoStudioViewModel.PreviewBitmap) ||
            e.PropertyName == nameof(PhotoStudioViewModel.SourcePath))
        {
            if (!isDraggingPreview)
            {
                ResetPreviewImageTransform();
            }

            return;
        }

        if (e.PropertyName == nameof(PhotoStudioViewModel.PreviewOffsetX) ||
            e.PropertyName == nameof(PhotoStudioViewModel.PreviewOffsetY))
        {
            if (!isDraggingPreview)
            {
                previewImageTranslateTransform.X = 0d;
                previewImageTranslateTransform.Y = 0d;
            }

            return;
        }

        if (e.PropertyName == nameof(PhotoStudioViewModel.PreviewScale))
        {
            var scale = subscribedViewModel?.PreviewScale ?? 1d;
            previewImageScaleTransform.ScaleX = scale;
            previewImageScaleTransform.ScaleY = scale;
        }
    }

    private void ResetPreviewImageTransform()
    {
        previewImageScaleTransform.ScaleX = 1d;
        previewImageScaleTransform.ScaleY = 1d;
        previewImageTranslateTransform.X = 0d;
        previewImageTranslateTransform.Y = 0d;
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

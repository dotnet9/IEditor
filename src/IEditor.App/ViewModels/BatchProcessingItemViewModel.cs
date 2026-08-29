using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IEditor.App.ViewModels;

public partial class BatchProcessingItemViewModel : ObservableObject, IDisposable
{
    private readonly Bitmap? _thumbnail;

    public BatchProcessingItemViewModel(
        string fileName,
        string fileInfo,
        string sizeText,
        string backgroundText,
        Color backgroundColor,
        string statusText,
        double progress,
        int pixelWidth,
        int pixelHeight,
        string? sourcePath = null,
        Bitmap? thumbnail = null)
    {
        FileName = fileName;
        FileInfo = fileInfo;
        SizeText = sizeText;
        BackgroundText = backgroundText;
        BackgroundColor = backgroundColor;
        StatusText = statusText;
        Progress = progress;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        SourcePath = sourcePath;
        _thumbnail = thumbnail;
    }

    public string FileName { get; }

    public string FileInfo { get; }

    public string SizeText { get; }

    public string BackgroundText { get; }

    public Color BackgroundColor { get; }

    public IBrush ThumbnailBrush => new SolidColorBrush(BackgroundColor);

    public Bitmap? Thumbnail => _thumbnail;

    public string? SourcePath { get; }

    public int PixelWidth { get; }

    public int PixelHeight { get; }

    [ObservableProperty]
    private string statusText = string.Empty;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private bool isSelected;

    public static BatchProcessingItemViewModel CreatePlaceholder(
        string fileName,
        string fileInfo,
        string sizeText,
        string backgroundText,
        Color backgroundColor,
        string statusText,
        double progress) =>
        new(fileName, fileInfo, sizeText, backgroundText, backgroundColor, statusText, progress, 0, 0);

    public static BatchProcessingItemViewModel CreateFromPath(
        string sourcePath,
        string sizeText,
        string backgroundText,
        Color backgroundColor,
        string statusText,
        double progress = 0)
    {
        using var stream = File.OpenRead(sourcePath);
        var bitmap = new Bitmap(stream);
        var fileInfo = $"{bitmap.PixelSize.Width}×{bitmap.PixelSize.Height} · {new FileInfo(sourcePath).Length / 1024d / 1024d:0.#} MB";
        return new BatchProcessingItemViewModel(
            Path.GetFileName(sourcePath),
            fileInfo,
            sizeText,
            backgroundText,
            backgroundColor,
            statusText,
            progress,
            bitmap.PixelSize.Width,
            bitmap.PixelSize.Height,
            sourcePath,
            bitmap);
    }

    public void Dispose()
    {
        _thumbnail?.Dispose();
    }
}

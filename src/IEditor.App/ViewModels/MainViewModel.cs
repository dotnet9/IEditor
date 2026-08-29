using System.IO;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Converters;
using IEditor.Core.Models;
using IEditor.Core.Services;
using Avalonia.Media.Imaging;

namespace IEditor.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IPhotoStudioService _photoStudioService;
    private Bitmap? _previewBitmap;

    public MainViewModel(IPhotoStudioService photoStudioService)
    {
        _photoStudioService = photoStudioService;

        SizeOptions = new[]
        {
            new PhotoSizeOption("1寸", "1寸", 25, 35, false),
            new PhotoSizeOption("2寸", "2寸", 35, 49, false),
            new PhotoSizeOption("自定义", "自定义", 25, 35, true)
        };

        BackgroundOptions = new[]
        {
            new BackgroundOption("白色", "白色", RgbColor.White, false),
            new BackgroundOption("蓝色", "蓝色", RgbColor.Blue, false),
            new BackgroundOption("自定义", "自定义", RgbColor.White, true)
        };

        selectedSizeOption = SizeOptions[0];
        selectedBackgroundOption = BackgroundOptions[0];
        widthMm = selectedSizeOption.WidthMm;
        heightMm = selectedSizeOption.HeightMm;
        dpi = 300;
        customBackgroundColor = Colors.White;
        inputSuggestedStartPath = Environment.CurrentDirectory;
        outputSuggestedStartPath = Environment.CurrentDirectory;
        suggestedOutputFileName = BuildSuggestedOutputFileName();
        sourceSummary = "未选择图片";
        processingSummary = string.Empty;
        backgroundSummary = string.Empty;
        statusMessage = "就绪";
        UpdateDerivedText();
    }

    public IReadOnlyList<PhotoSizeOption> SizeOptions { get; }

    public IReadOnlyList<BackgroundOption> BackgroundOptions { get; }

    [ObservableProperty]
    private PhotoSizeOption selectedSizeOption;

    [ObservableProperty]
    private BackgroundOption selectedBackgroundOption;

    [ObservableProperty]
    private double widthMm;

    [ObservableProperty]
    private double heightMm;

    [ObservableProperty]
    private int dpi;

    [ObservableProperty]
    private Color customBackgroundColor;

    [ObservableProperty]
    private string? sourcePath;

    [ObservableProperty]
    private string? outputPath;

    [ObservableProperty]
    private string inputSuggestedStartPath;

    [ObservableProperty]
    private string outputSuggestedStartPath;

    [ObservableProperty]
    private string suggestedOutputFileName;

    [ObservableProperty]
    private string sourceSummary;

    [ObservableProperty]
    private string processingSummary;

    [ObservableProperty]
    private string backgroundSummary;

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private bool isBusy;

    public Bitmap? PreviewBitmap
    {
        get => _previewBitmap;
        private set
        {
            if (ReferenceEquals(_previewBitmap, value))
            {
                return;
            }

            var old = _previewBitmap;
            _previewBitmap = value;
            OnPropertyChanged();
            old?.Dispose();
        }
    }

    public bool IsCustomSize => SelectedSizeOption.IsCustom;

    public bool IsCustomBackground => SelectedBackgroundOption.IsCustom;

    public PhotoSizeSpec CurrentSize => new(WidthMm, HeightMm, Dpi);

    public RgbColor ActiveBackgroundColor =>
        SelectedBackgroundOption.IsCustom ? CustomBackgroundColor.ToRgbColor() : SelectedBackgroundOption.Color;

    [RelayCommand]
    private void OpenSource(IReadOnlyList<string>? paths)
    {
        var path = paths?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        SourcePath = path;
        InputSuggestedStartPath = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
        OutputSuggestedStartPath = InputSuggestedStartPath;
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
        SourceSummary = Path.GetFileName(path);
        StatusMessage = "源图片已选中";
        _ = GeneratePreviewAsync();
    }

    [RelayCommand]
    private async Task GeneratePreviewAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(SourcePath))
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _photoStudioService.ProcessAsync(BuildRequest(PhotoOutputFormat.Png));
            PreviewBitmap = CreateBitmap(result.Data);
            UpdateDerivedText();
            StatusMessage = $"已生成预览：{result.WidthPx} × {result.HeightPx} 像素";
        }
        catch (Exception ex)
        {
            StatusMessage = $"预览失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var targetPath = ResolveOutputPath();
        if (targetPath is null)
        {
            StatusMessage = "未选择输出路径";
            return;
        }

        IsBusy = true;
        try
        {
            var request = BuildRequest(DetectOutputFormat(targetPath));
            var result = await _photoStudioService.ProcessAsync(request);
            await File.WriteAllBytesAsync(targetPath, result.Data);
            PreviewBitmap = CreateBitmap(result.Data);
            OutputPath = targetPath;
            OutputSuggestedStartPath = Path.GetDirectoryName(targetPath) ?? OutputSuggestedStartPath;
            StatusMessage = $"已保存：{targetPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        SourcePath = null;
        OutputPath = null;
        InputSuggestedStartPath = Environment.CurrentDirectory;
        OutputSuggestedStartPath = Environment.CurrentDirectory;
        SelectedSizeOption = SizeOptions[0];
        SelectedBackgroundOption = BackgroundOptions[0];
        WidthMm = SelectedSizeOption.WidthMm;
        HeightMm = SelectedSizeOption.HeightMm;
        Dpi = 300;
        CustomBackgroundColor = Colors.White;
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
        SourceSummary = "未选择图片";
        UpdateDerivedText();
        PreviewBitmap = null;
        StatusMessage = "已重置";
    }

    partial void OnSelectedSizeOptionChanged(PhotoSizeOption value)
    {
        if (!value.IsCustom)
        {
            WidthMm = value.WidthMm;
            HeightMm = value.HeightMm;
        }

        UpdateDerivedText();
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
        OnPropertyChanged(nameof(IsCustomSize));
    }

    partial void OnSelectedBackgroundOptionChanged(BackgroundOption value)
    {
        UpdateDerivedText();
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
        OnPropertyChanged(nameof(IsCustomBackground));
    }

    partial void OnWidthMmChanged(double value)
    {
        UpdateDerivedText();
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
    }

    partial void OnHeightMmChanged(double value)
    {
        UpdateDerivedText();
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
    }

    partial void OnDpiChanged(int value)
    {
        UpdateDerivedText();
    }

    partial void OnCustomBackgroundColorChanged(Color value)
    {
        UpdateDerivedText();
    }

    partial void OnSourcePathChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SourceSummary = "未选择图片";
            return;
        }

        SourceSummary = Path.GetFileName(value);
        InputSuggestedStartPath = Path.GetDirectoryName(value) ?? Environment.CurrentDirectory;
        OutputSuggestedStartPath = InputSuggestedStartPath;
        SuggestedOutputFileName = BuildSuggestedOutputFileName();
    }

    private void UpdateDerivedText()
    {
        var size = CurrentSize;
        ProcessingSummary = $"{SelectedSizeOption.DisplayName} · {size.WidthMm:0.#} × {size.HeightMm:0.#} 毫米 · {size.WidthPx} × {size.HeightPx} 像素";
        BackgroundSummary = $"{SelectedBackgroundOption.DisplayName}背景 · {ActiveBackgroundColor.ToHexString()}";
        OnPropertyChanged(nameof(IsCustomSize));
        OnPropertyChanged(nameof(IsCustomBackground));
    }

    private PhotoProcessingRequest BuildRequest(PhotoOutputFormat outputFormat) =>
        new(SourcePath ?? string.Empty, CurrentSize, ActiveBackgroundColor, outputFormat);

    private static Bitmap CreateBitmap(byte[] data)
    {
        using var stream = new MemoryStream(data);
        return new Bitmap(stream);
    }

    private string BuildSuggestedOutputFileName()
    {
        var baseName = string.IsNullOrWhiteSpace(SourcePath)
            ? "照片"
            : Path.GetFileNameWithoutExtension(SourcePath);

        return $"{baseName}-{SelectedSizeOption.FileToken}-{SelectedBackgroundOption.FileToken}.jpg";
    }

    private string? ResolveOutputPath()
    {
        if (!string.IsNullOrWhiteSpace(OutputPath))
        {
            return OutputPath;
        }

        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(SourcePath);
        return string.IsNullOrWhiteSpace(directory)
            ? SuggestedOutputFileName
            : Path.Combine(directory, SuggestedOutputFileName);
    }

    private static PhotoOutputFormat DetectOutputFormat(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => PhotoOutputFormat.Png,
            ".jpg" => PhotoOutputFormat.Jpeg,
            ".jpeg" => PhotoOutputFormat.Jpeg,
            _ => PhotoOutputFormat.Jpeg
        };
    }

    public void Dispose()
    {
        PreviewBitmap?.Dispose();
    }
}

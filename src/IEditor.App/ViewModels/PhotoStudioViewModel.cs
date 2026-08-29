using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Converters;
using IEditor.App.Models;
using IEditor.App.Services;
using IEditor.Core.Models;
using IEditor.Core.Services;

namespace IEditor.App.ViewModels;

public enum PhotoStudioTool
{
    LoadImage,
    Crop,
    RotateLeft,
    RotateRight,
    FlipHorizontal,
    Reset
}

public partial class PhotoStudioViewModel : WorkspacePageViewModel
{
    private readonly IPhotoStudioService _photoStudioService;
    private readonly AppPreferencesService _preferencesService;
    private Bitmap? _previewBitmap;
    private Bitmap? _originalBitmap;
    private string? _selectedSizeToken;
    private string? _selectedBackgroundToken;
    private PhotoSizeOptionViewModel? selectedSizeOption;
    private BackgroundOptionViewModel? selectedBackgroundOption;
    private string? sourcePath;
    private string? outputFolder;
    private string? outputPath;
    private string inputSuggestedStartPath = string.Empty;
    private string outputSuggestedStartPath = string.Empty;
    private string suggestedOutputFileName = string.Empty;
    private double widthMm;
    private double heightMm;
    private int dpi;
    private Color customBackgroundColor;
    private PhotoOutputFormat outputFormat;
    private uint outputQuality;
    private string filenameTemplate = string.Empty;
    private bool showGuides;
    private bool comparePreview;
    private int previewZoomPercent;
    private PhotoStudioTool selectedTool;
    private string sourceSummary = string.Empty;
    private string processingSummary = string.Empty;
    private string backgroundSummary = string.Empty;
    private bool isBusy;

    public PhotoStudioViewModel(IPhotoStudioService photoStudioService, AppPreferencesService preferencesService)
    {
        _photoStudioService = photoStudioService;
        _preferencesService = preferencesService;

        var preferences = _preferencesService.Preferences;
        dpi = preferences.DefaultDpi;
        outputFormat = preferences.DefaultFormat;
        outputQuality = preferences.DefaultQuality;
        outputFolder = preferences.DefaultSaveLocation;
        filenameTemplate = preferences.FilenameTemplate;
        showGuides = preferences.ShowGuides;
        inputSuggestedStartPath = preferences.DefaultSaveLocation;
        outputSuggestedStartPath = preferences.DefaultSaveLocation;
        selectedTool = PhotoStudioTool.Crop;
        previewZoomPercent = 100;
        comparePreview = false;
        StatusText = L(Localization.PhotoStudio.Status.Ready);
        customBackgroundColor = Colors.White;
        widthMm = 25;
        heightMm = 35;
        InitializeLocalizedText();
    }

    public ObservableCollection<PhotoSizeOptionViewModel> SizeOptions { get; private set; } = [];

    public ObservableCollection<BackgroundOptionViewModel> BackgroundOptions { get; private set; } = [];

    public ObservableCollection<FormatOptionViewModel> FormatOptions { get; private set; } = [];

    public PhotoSizeOptionViewModel? SelectedSizeOption
    {
        get => selectedSizeOption;
        set
        {
            if (SetProperty(ref selectedSizeOption, value))
            {
                _selectedSizeToken = value?.Token;
                UpdateSizeFields(value);
                UpdateSuggestedFileName();
                UpdateDerivedText();
            }
        }
    }

    public BackgroundOptionViewModel? SelectedBackgroundOption
    {
        get => selectedBackgroundOption;
        set
        {
            if (SetProperty(ref selectedBackgroundOption, value))
            {
                _selectedBackgroundToken = value?.Token;
                foreach (var option in BackgroundOptions)
                {
                    option.IsSelected = option == value;
                }

                if (value is not null && !value.IsCustom)
                {
                    CustomBackgroundColor = value.AvaloniaColor;
                }

                UpdateSuggestedFileName();
                UpdateDerivedText();
            }
        }
    }

    public FormatOptionViewModel? SelectedFormatOption
    {
        get => FormatOptions.FirstOrDefault(item => item.Value == OutputFormat);
        set
        {
            if (value is not null)
            {
                OutputFormat = value.Value;
                foreach (var option in FormatOptions)
                {
                    option.IsSelected = option == value;
                }
            }
        }
    }

    public double WidthMm
    {
        get => widthMm;
        set
        {
            if (SetProperty(ref widthMm, value))
            {
                UpdateSuggestedFileName();
                UpdateDerivedText();
            }
        }
    }

    public double HeightMm
    {
        get => heightMm;
        set
        {
            if (SetProperty(ref heightMm, value))
            {
                UpdateSuggestedFileName();
                UpdateDerivedText();
            }
        }
    }

    public int Dpi
    {
        get => dpi;
        set
        {
            if (SetProperty(ref dpi, value))
            {
                UpdateDerivedText();
            }
        }
    }

    public Color CustomBackgroundColor
    {
        get => customBackgroundColor;
        set
        {
            if (SetProperty(ref customBackgroundColor, value))
            {
                if (SelectedBackgroundOption is { IsCustom: true })
                {
                    SelectedBackgroundOption.SetColor(value.ToRgbColor());
                }

                UpdateDerivedText();
            }
        }
    }

    public string? SourcePath
    {
        get => sourcePath;
        set
        {
            if (SetProperty(ref sourcePath, value))
            {
                OnSourcePathUpdated();
            }
        }
    }

    public string? OutputFolder
    {
        get => outputFolder;
        set
        {
            if (SetProperty(ref outputFolder, value))
            {
                UpdateSuggestedFileName();
            }
        }
    }

    public string? OutputPath
    {
        get => outputPath;
        set
        {
            if (SetProperty(ref outputPath, value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var directory = Path.GetDirectoryName(value);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        OutputFolder = directory;
                        OutputSuggestedStartPath = directory;
                    }

                    var fileName = Path.GetFileName(value);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        SuggestedOutputFileName = fileName;
                    }
                }
            }
        }
    }

    public string InputSuggestedStartPath
    {
        get => inputSuggestedStartPath;
        set => SetProperty(ref inputSuggestedStartPath, value);
    }

    public string OutputSuggestedStartPath
    {
        get => outputSuggestedStartPath;
        set => SetProperty(ref outputSuggestedStartPath, value);
    }

    public string SuggestedOutputFileName
    {
        get => suggestedOutputFileName;
        private set => SetProperty(ref suggestedOutputFileName, value);
    }

    public PhotoOutputFormat OutputFormat
    {
        get => outputFormat;
        set
        {
            if (SetProperty(ref outputFormat, value))
            {
                OnPropertyChanged(nameof(IsJpegFormat));
                OnPropertyChanged(nameof(IsPngFormat));
                OnPropertyChanged(nameof(SelectedFormatOption));
                foreach (var option in FormatOptions)
                {
                    option.IsSelected = option.Value == value;
                }
                UpdateSuggestedFileName();
            }
        }
    }

    public uint OutputQuality
    {
        get => outputQuality;
        set => SetProperty(ref outputQuality, value);
    }

    public string FilenameTemplate
    {
        get => filenameTemplate;
        set
        {
            if (SetProperty(ref filenameTemplate, value))
            {
                UpdateSuggestedFileName();
            }
        }
    }

    public bool ShowGuides
    {
        get => showGuides;
        set
        {
            if (SetProperty(ref showGuides, value))
            {
                _preferencesService.Preferences.ShowGuides = value;
                _preferencesService.ApplyAndSave();
            }
        }
    }

    public bool ComparePreview
    {
        get => comparePreview;
        set
        {
            if (SetProperty(ref comparePreview, value))
            {
                OnPropertyChanged(nameof(DisplayedPreviewBitmap));
            }
        }
    }

    public double PreviewScale => PreviewZoomPercent / 100d;

    public int PreviewZoomPercent
    {
        get => previewZoomPercent;
        set
        {
            if (SetProperty(ref previewZoomPercent, Math.Clamp(value, 50, 200)))
            {
                OnPropertyChanged(nameof(PreviewScale));
                OnPropertyChanged(nameof(PreviewZoomText));
            }
        }
    }

    public string PreviewZoomText => $"{PreviewZoomPercent}%";

    public bool IsJpegFormat => OutputFormat == PhotoOutputFormat.Jpeg;

    public bool IsPngFormat => OutputFormat == PhotoOutputFormat.Png;

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public PhotoStudioTool SelectedTool
    {
        get => selectedTool;
        set => SetProperty(ref selectedTool, value);
    }

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
            OnPropertyChanged(nameof(DisplayedPreviewBitmap));
            old?.Dispose();
        }
    }

    public Bitmap? OriginalBitmap
    {
        get => _originalBitmap;
        private set
        {
            if (ReferenceEquals(_originalBitmap, value))
            {
                return;
            }

            var old = _originalBitmap;
            _originalBitmap = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedPreviewBitmap));
            old?.Dispose();
        }
    }

    public Bitmap? DisplayedPreviewBitmap => ComparePreview ? OriginalBitmap : PreviewBitmap;

    public bool IsCustomSize => SelectedSizeOption?.IsCustom == true;

    public bool IsCustomBackground => SelectedBackgroundOption?.IsCustom == true;

    public PhotoSizeSpec CurrentSize => new(WidthMm, HeightMm, Dpi);

    public RgbColor ActiveBackgroundColor =>
        SelectedBackgroundOption is { IsCustom: true }
            ? CustomBackgroundColor.ToRgbColor()
            : SelectedBackgroundOption?.Color ?? RgbColor.White;

    public string WindowTitle => Title;

    public string WindowSubtitle => Subtitle;

    public string PageStatusBadge => StatusText;

    public string SourceSummary
    {
        get => sourceSummary;
        private set => SetProperty(ref sourceSummary, value);
    }

    public string ProcessingSummary
    {
        get => processingSummary;
        private set => SetProperty(ref processingSummary, value);
    }

    public string BackgroundSummary
    {
        get => backgroundSummary;
        private set => SetProperty(ref backgroundSummary, value);
    }

    public string SourceSectionTitle { get; private set; } = string.Empty;
    public string SourceSectionHint { get; private set; } = string.Empty;
    public string SizeSectionTitle { get; private set; } = string.Empty;
    public string SizeSectionHint { get; private set; } = string.Empty;
    public string BackgroundSectionTitle { get; private set; } = string.Empty;
    public string BackgroundSectionHint { get; private set; } = string.Empty;
    public string OutputSectionTitle { get; private set; } = string.Empty;
    public string OutputSectionHint { get; private set; } = string.Empty;
    public string EngineReady { get; private set; } = string.Empty;
    public string LoadImageText { get; private set; } = string.Empty;
    public string CompareOriginalText { get; private set; } = string.Empty;
    public string GuidesText { get; private set; } = string.Empty;
    public string FitToWindowText { get; private set; } = string.Empty;
    public string RotateLeftText { get; private set; } = string.Empty;
    public string RotateRightText { get; private set; } = string.Empty;
    public string FlipHorizontalText { get; private set; } = string.Empty;
    public string ResetViewText { get; private set; } = string.Empty;
    public string HelpText { get; private set; } = string.Empty;
    public string ThemeSwitchText { get; private set; } = string.Empty;
    public string ExportImageText { get; private set; } = string.Empty;
    public string FileFormatText { get; private set; } = string.Empty;
    public string OutputQualityText { get; private set; } = string.Empty;
    public string SaveLocationText { get; private set; } = string.Empty;
    public string FilenameTemplateText { get; private set; } = string.Empty;
    public string CropText { get; private set; } = string.Empty;
    public string ProcessingEngineText { get; private set; } = string.Empty;
    public string WidthMmText { get; private set; } = string.Empty;
    public string HeightMmText { get; private set; } = string.Empty;
    public string DpiText { get; private set; } = string.Empty;
    public string FormatJpgText { get; private set; } = string.Empty;
    public string FormatPngText { get; private set; } = string.Empty;
    public string SaveExportText { get; private set; } = string.Empty;

    protected override void OnDisposed()
    {
        PreviewBitmap?.Dispose();
        OriginalBitmap?.Dispose();
        base.OnDisposed();
    }

    protected override void RefreshLocalizedText()
    {
        Title = L(Localization.Shell.Navigation.PhotoStudio);
        Subtitle = L(Localization.Shell.Window.Subtitle);
        Breadcrumb = L(Localization.PhotoStudio.Page.Breadcrumb);
        StatusText = string.IsNullOrWhiteSpace(StatusText) ? L(Localization.PhotoStudio.Status.Ready) : StatusText;
        SourceSummary = string.IsNullOrWhiteSpace(SourceSummary)
            ? L(Localization.PhotoStudio.Summary.NoImageSelected)
            : SourceSummary;

        SourceSectionTitle = L(Localization.PhotoStudio.Page.SourceSection);
        SourceSectionHint = L(Localization.PhotoStudio.Labels.SourceHint);
        SizeSectionTitle = L(Localization.PhotoStudio.Page.SizeSection);
        SizeSectionHint = L(Localization.PhotoStudio.Labels.SizeHint);
        BackgroundSectionTitle = L(Localization.PhotoStudio.Page.BackgroundSection);
        BackgroundSectionHint = L(Localization.PhotoStudio.Page.BackgroundSmartDescription);
        OutputSectionTitle = L(Localization.PhotoStudio.Page.OutputSection);
        OutputSectionHint = L(Localization.PhotoStudio.Page.OutputHint);
        EngineReady = L(Localization.PhotoStudio.Page.EngineReady);
        LoadImageText = L(Localization.PhotoStudio.Page.LoadImage);
        CompareOriginalText = L(Localization.PhotoStudio.Page.CompareOriginal);
        GuidesText = L(Localization.PhotoStudio.Page.Guides);
        FitToWindowText = L(Localization.PhotoStudio.Page.FitToWindow);
        CropText = L(Localization.Common.Actions.Crop);
        RotateLeftText = L(Localization.PhotoStudio.Page.RotateLeft);
        RotateRightText = L(Localization.PhotoStudio.Page.RotateRight);
        FlipHorizontalText = L(Localization.PhotoStudio.Page.FlipHorizontal);
        ResetViewText = L(Localization.PhotoStudio.Page.ResetView);
        HelpText = L(Localization.Common.Actions.Help);
        ThemeSwitchText = L(Localization.PhotoStudio.Page.ThemeSwitch);
        ExportImageText = L(Localization.PhotoStudio.Page.ExportImage);
        FileFormatText = L(Localization.PhotoStudio.Labels.FileFormat);
        OutputQualityText = L(Localization.PhotoStudio.Labels.OutputQuality);
        SaveLocationText = L(Localization.PhotoStudio.Labels.SaveLocation);
        FilenameTemplateText = L(Localization.PhotoStudio.Labels.FilenameTemplate);
        WidthMmText = L(Localization.PhotoStudio.Labels.WidthMm);
        HeightMmText = L(Localization.PhotoStudio.Labels.HeightMm);
        ProcessingEngineText = L(Localization.Settings.Labels.ProcessingEngine);
        DpiText = L(Localization.PhotoStudio.Labels.Dpi);

        FormatJpgText = L(Localization.Common.Formats.Jpg);
        FormatPngText = L(Localization.Common.Formats.Png);
        SaveExportText = L(Localization.Common.Actions.SaveExport);

        RefreshOptions();
        UpdateDerivedText();
    }

    [RelayCommand]
    private void OpenSource(IReadOnlyList<string>? paths)
    {
        var path = paths?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        SourcePath = path;
        var directory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
        InputSuggestedStartPath = directory;
        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            OutputFolder = directory;
            OutputSuggestedStartPath = directory;
        }

        StatusText = L(Localization.PhotoStudio.Status.SourceSelected);
        OutputPath ??= Path.Combine(OutputFolder ?? directory, SuggestedOutputFileName);
        _ = GeneratePreviewAsync();
    }

    [RelayCommand]
    private async Task GeneratePreviewAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(SourcePath))
        {
            StatusText = L(Localization.PhotoStudio.Status.SourcePathRequired);
            return;
        }

        IsBusy = true;
        try
        {
            OriginalBitmap ??= LoadBitmap(SourcePath!);
            var result = await _photoStudioService.ProcessAsync(BuildRequest(PhotoOutputFormat.Png));
            PreviewBitmap = CreateBitmap(result.Data);
            UpdateDerivedText();
            StatusText = string.Format(L(Localization.PhotoStudio.Status.PreviewGenerated), result.WidthPx, result.HeightPx);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(L(Localization.PhotoStudio.Status.PreviewFailed), ex.Message);
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
            StatusText = L(Localization.PhotoStudio.Status.OutputPathMissing);
            return;
        }

        IsBusy = true;
        try
        {
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var request = BuildRequest(OutputFormat);
            var result = await _photoStudioService.ProcessAsync(request);
            await File.WriteAllBytesAsync(targetPath, result.Data);
            PreviewBitmap = CreateBitmap(result.Data);
            OutputPath = targetPath;
            OutputSuggestedStartPath = Path.GetDirectoryName(targetPath) ?? OutputSuggestedStartPath;
            StatusText = string.Format(L(Localization.PhotoStudio.Status.SavedFormat), targetPath);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(L(Localization.PhotoStudio.Status.SaveFailed), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        var preferences = _preferencesService.Preferences;
        SourcePath = null;
        OutputFolder = preferences.DefaultSaveLocation;
        InputSuggestedStartPath = preferences.DefaultSaveLocation;
        OutputSuggestedStartPath = preferences.DefaultSaveLocation;
        OutputFormat = preferences.DefaultFormat;
        OutputQuality = preferences.DefaultQuality;
        FilenameTemplate = preferences.FilenameTemplate;
        Dpi = preferences.DefaultDpi;
        ShowGuides = preferences.ShowGuides;
        ComparePreview = false;
        PreviewZoomPercent = 100;
        SelectedTool = PhotoStudioTool.Crop;
        CustomBackgroundColor = Colors.White;
        OutputPath = null;
        RefreshOptions();
        StatusText = L(Localization.PhotoStudio.Status.Reset);
        UpdateDerivedText();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var preferences = _preferencesService.Preferences;
        preferences.ThemeMode = preferences.ThemeMode switch
        {
            AppThemeMode.Light => AppThemeMode.Dark,
            AppThemeMode.Dark => AppThemeMode.Light,
            _ => AppThemeMode.Light
        };
        _preferencesService.ApplyAndSave();
        StatusText = L(Localization.Common.States.AutoSaved);
    }

    [RelayCommand]
    private void Help()
    {
        StatusText = L(Localization.Common.States.ComingSoon);
    }

    [RelayCommand]
    private void ToggleGuides()
    {
        ShowGuides = !ShowGuides;
    }

    [RelayCommand]
    private void ToggleCompare()
    {
        ComparePreview = !ComparePreview;
    }

    [RelayCommand]
    private void SetZoom(string direction)
    {
        PreviewZoomPercent = direction switch
        {
            "+" => Math.Min(200, PreviewZoomPercent + 25),
            "-" => Math.Max(50, PreviewZoomPercent - 25),
            "fit" => 100,
            _ => PreviewZoomPercent
        };
    }

    [RelayCommand]
    private void ActivateTool(PhotoStudioTool tool)
    {
        SelectedTool = tool;
        if (tool == PhotoStudioTool.Reset)
        {
            Reset();
        }
    }

    private void OnSourcePathUpdated()
    {
        PreviewBitmap = null;
        OriginalBitmap = null;

        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            SourceSummary = L(Localization.PhotoStudio.Summary.NoImageSelected);
            UpdateSuggestedFileName();
            return;
        }

        var fileName = Path.GetFileName(SourcePath);
        SourceSummary = fileName;
        InputSuggestedStartPath = Path.GetDirectoryName(SourcePath) ?? Environment.CurrentDirectory;
        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            OutputFolder = InputSuggestedStartPath;
        }

        OutputSuggestedStartPath = OutputFolder ?? InputSuggestedStartPath;
        UpdateSuggestedFileName();
    }

    private void RefreshOptions()
    {
        _selectedSizeToken ??= SelectedSizeOption?.Token ?? "1寸";
        _selectedBackgroundToken ??= SelectedBackgroundOption?.Token ?? "白色";

        SizeOptions =
        [
            new PhotoSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.OneInch), "1寸", 25, 35, false),
            new PhotoSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.TwoInch), "2寸", 35, 49, false),
            new PhotoSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.Custom), "自定义", WidthMm, HeightMm, true)
        ];

        BackgroundOptions =
        [
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.WhiteName), "白色", RgbColor.White, false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.BlueName), "蓝色", RgbColor.Blue, false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.Custom), "自定义", CustomBackgroundColor.ToRgbColor(), true)
        ];

        FormatOptions =
        [
            new FormatOptionViewModel(FormatJpgText, PhotoOutputFormat.Jpeg),
            new FormatOptionViewModel(FormatPngText, PhotoOutputFormat.Png)
        ];

        SelectedSizeOption = SizeOptions.FirstOrDefault(item => item.Token == _selectedSizeToken) ?? SizeOptions[0];
        SelectedBackgroundOption = BackgroundOptions.FirstOrDefault(item => item.Token == _selectedBackgroundToken) ?? BackgroundOptions[0];
        SelectedFormatOption = FormatOptions.FirstOrDefault(item => item.Value == OutputFormat) ?? FormatOptions[0];

        foreach (var option in SizeOptions)
        {
            option.IsSelected = option == SelectedSizeOption;
        }

        foreach (var option in BackgroundOptions)
        {
            option.IsSelected = option == SelectedBackgroundOption;
        }

        foreach (var option in FormatOptions)
        {
            option.IsSelected = option == SelectedFormatOption;
        }
    }

    private void UpdateSizeFields(PhotoSizeOptionViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        foreach (var option in SizeOptions)
        {
            option.IsSelected = option == value;
        }

        if (!value.IsCustom)
        {
            WidthMm = value.WidthMm;
            HeightMm = value.HeightMm;
        }
    }

    private Dictionary<string, string> BuildFilenameTokens()
    {
        var size = SelectedSizeOption?.Title ?? string.Empty;
        var background = SelectedBackgroundOption?.Title ?? string.Empty;
        var sourceName = string.IsNullOrWhiteSpace(SourcePath)
            ? "照片"
            : Path.GetFileNameWithoutExtension(SourcePath);

        return new Dictionary<string, string>
        {
            ["原名"] = sourceName,
            ["规格"] = size,
            ["底色"] = background,
            ["日期"] = DateTime.Now.ToString("yyyyMMdd"),
            ["序号"] = "001"
        };
    }

    private void UpdateDerivedText()
    {
        var size = CurrentSize;
        ProcessingSummary = string.Format(
            L(Localization.PhotoStudio.Summary.ProcessingFormat),
            SelectedSizeOption?.Title ?? string.Empty,
            size.WidthMm,
            size.HeightMm,
            size.WidthPx,
            size.HeightPx);

        BackgroundSummary = string.Format(
            L(Localization.PhotoStudio.Summary.BackgroundFormat),
            SelectedBackgroundOption?.Title ?? string.Empty,
            ActiveBackgroundColor.ToHexString());

        OnPropertyChanged(nameof(IsCustomSize));
        OnPropertyChanged(nameof(IsCustomBackground));
        OnPropertyChanged(nameof(IsJpegFormat));
        OnPropertyChanged(nameof(IsPngFormat));
        OnPropertyChanged(nameof(PreviewScale));
        OnPropertyChanged(nameof(PreviewZoomText));
    }

    private PhotoProcessingRequest BuildRequest(PhotoOutputFormat outputFormat) =>
        new(
            SourcePath ?? string.Empty,
            CurrentSize,
            ActiveBackgroundColor,
            outputFormat,
            OutputQuality);

    private static Bitmap CreateBitmap(byte[] data)
    {
        using var stream = new MemoryStream(data);
        return new Bitmap(stream);
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        return new Bitmap(stream);
    }

    private string? ResolveOutputPath()
    {
        if (!string.IsNullOrWhiteSpace(OutputPath))
        {
            return OutputPath;
        }

        var folder = string.IsNullOrWhiteSpace(OutputFolder)
            ? InputSuggestedStartPath
            : OutputFolder;

        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        return Path.Combine(folder, SuggestedOutputFileName);
    }

    private void UpdateSuggestedFileName()
    {
        var baseName = FilenameTemplateFormatter.Format(FilenameTemplate, BuildFilenameTokens());
        var extension = OutputFormat.GetExtension();
        SuggestedOutputFileName = Path.ChangeExtension(baseName, extension);
    }
}

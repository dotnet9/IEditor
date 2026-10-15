using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using IEditor.App.Converters;
using IEditor.App.Services;
using IEditor.Core.Models;
using IEditor.Core.Services;

namespace IEditor.App.ViewModels;

public partial class BatchProcessingViewModel : WorkspacePageViewModel
{
    private readonly Dictionary<string, string> _processed = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _lifetime = new();
    private readonly IPhotoStudioService _photoStudioService;
    private readonly AppPreferencesService _preferencesService;
    private BatchSizeOptionViewModel? selectedSizeOption;
    private BackgroundOptionViewModel? selectedBackgroundOption;
    private string filenameTemplate = string.Empty;
    private int concurrency;
    private bool skipProcessed;
    private bool openFolderWhenDone;
    private bool isBusy;
    private string queueSummaryText = string.Empty;
    private string footerStatusText = string.Empty;
    private string importPathsText = string.Empty;
    private string dropTitleText = string.Empty;
    private string dropDescriptionText = string.Empty;
    private string queueTitleText = string.Empty;
    private string queueHintText = string.Empty;
    private string addImageText = string.Empty;
    private string exportAllText = string.Empty;
    private string exportZipText = string.Empty;
    private string uniformSizeText = string.Empty;
    private string uniformBackgroundText = string.Empty;
    private string removeText = string.Empty;
    private string advancedOptionsTitleText = string.Empty;
    private string filenameTemplateLabelText = string.Empty;
    private string filenameTemplateDescriptionText = string.Empty;
    private string concurrencyLabelText = string.Empty;
    private string concurrencyDescriptionText = string.Empty;
    private string skipProcessedLabelText = string.Empty;
    private string skipProcessedDescriptionText = string.Empty;
    private string openFolderWhenDoneLabelText = string.Empty;
    private string openFolderWhenDoneDescriptionText = string.Empty;
    private string sizeSectionTitleText = string.Empty;
    private string sizeSectionHintText = string.Empty;
    private string backgroundSectionTitleText = string.Empty;
    private string backgroundSectionHintText = string.Empty;
    private string sizeSummaryText = string.Empty;
    private string selectedCountText = string.Empty;
    private string fileColumnText = string.Empty;
    private string sizeColumnText = string.Empty;
    private string backgroundColumnText = string.Empty;
    private string statusColumnText = string.Empty;
    private int completedCount;
    private int processingCount;
    private int waitingCount;
    private int failedCount;

    public BatchProcessingViewModel(IPhotoStudioService photoStudioService, AppPreferencesService preferencesService)
    {
        _photoStudioService = photoStudioService;
        _preferencesService = preferencesService;

        var preferences = _preferencesService.Preferences;
        filenameTemplate = "photo_{name}_{size}";
        concurrency = preferences.BatchConcurrency;
        skipProcessed = preferences.BatchSkipProcessed;
        openFolderWhenDone = preferences.BatchOpenFolderWhenDone;
        DefaultDpi = preferences.DefaultDpi;

        InitializeLocalizedText();
        BuildOptions();
        RebuildSummary();
    }

    public ObservableCollection<BatchProcessingItemViewModel> QueueItems { get; private set; } = [];

    public ObservableCollection<BatchSizeOptionViewModel> SizeOptions { get; private set; } = [];

    public ObservableCollection<BackgroundOptionViewModel> BackgroundOptions { get; private set; } = [];

    public BatchSizeOptionViewModel? SelectedSizeOption
    {
        get => selectedSizeOption;
        set
        {
            if (SetProperty(ref selectedSizeOption, value))
            {
                foreach (var option in SizeOptions)
                {
                    option.IsSelected = option == value;
                }

                UpdateSizeSummaryText();
                RefreshQueueLabels();
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
                foreach (var option in BackgroundOptions)
                {
                    option.IsSelected = option == value;
                }
                RefreshQueueLabels();
            }
        }
    }

    public string FilenameTemplate
    {
        get => filenameTemplate;
        set => SetProperty(ref filenameTemplate, value);
    }

    public int Concurrency
    {
        get => concurrency;
        set
        {
            if (SetProperty(ref concurrency, Math.Clamp(value, 1, 16)))
            {
                _preferencesService.Preferences.BatchConcurrency = concurrency;
                _preferencesService.Save();
                OnPropertyChanged(nameof(ConcurrencyText));
                OnPropertyChanged(nameof(IsConcurrency2));
                OnPropertyChanged(nameof(IsConcurrency4));
                OnPropertyChanged(nameof(IsConcurrency8));
            }
        }
    }

    public bool IsConcurrency2 => Concurrency == 2;

    public bool IsConcurrency4 => Concurrency == 4;

    public bool IsConcurrency8 => Concurrency == 8;

    public bool SkipProcessed
    {
        get => skipProcessed;
        set
        {
            if (SetProperty(ref skipProcessed, value))
            {
                _preferencesService.Preferences.BatchSkipProcessed = value;
                _preferencesService.Save();
            }
        }
    }

    public bool OpenFolderWhenDone
    {
        get => openFolderWhenDone;
        set
        {
            if (SetProperty(ref openFolderWhenDone, value))
            {
                _preferencesService.Preferences.BatchOpenFolderWhenDone = value;
                _preferencesService.Save();
            }
        }
    }

    public int DefaultDpi { get; private set; }

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    public string QueueSummaryText
    {
        get => queueSummaryText;
        private set => SetProperty(ref queueSummaryText, value);
    }

    public string FooterStatusText
    {
        get => footerStatusText;
        private set => SetProperty(ref footerStatusText, value);
    }

    public string ImportPathsText
    {
        get => importPathsText;
        set => SetProperty(ref importPathsText, value);
    }

    public string DropTitleText
    {
        get => dropTitleText;
        private set => SetProperty(ref dropTitleText, value);
    }

    public string DropDescriptionText
    {
        get => dropDescriptionText;
        private set => SetProperty(ref dropDescriptionText, value);
    }

    public string QueueTitleText
    {
        get => queueTitleText;
        private set => SetProperty(ref queueTitleText, value);
    }

    public string QueueHintText
    {
        get => queueHintText;
        private set => SetProperty(ref queueHintText, value);
    }

    public string AddImageText
    {
        get => addImageText;
        private set => SetProperty(ref addImageText, value);
    }

    public string SelectFileText
    {
        get => selectFileText;
        private set => SetProperty(ref selectFileText, value);
    }

    private string selectFileText = string.Empty;

    public string ConcurrencyText => string.Format(L(Localization.Batch.Labels.ConcurrencyFormat), Concurrency);

    public string TokenNameText { get; private set; } = "{name}";

    public string TokenSizeText { get; private set; } = "{size}";

    public string TokenColorText { get; private set; } = "{color}";

    public string TokenDateText { get; private set; } = "{date}";

    public string TokenIndexText { get; private set; } = "{index}";

    public string ExportAllText
    {
        get => exportAllText;
        private set => SetProperty(ref exportAllText, value);
    }

    public string ExportZipText
    {
        get => exportZipText;
        private set => SetProperty(ref exportZipText, value);
    }

    public string UniformSizeText
    {
        get => uniformSizeText;
        private set => SetProperty(ref uniformSizeText, value);
    }

    public string UniformBackgroundText
    {
        get => uniformBackgroundText;
        private set => SetProperty(ref uniformBackgroundText, value);
    }

    public string RemoveText
    {
        get => removeText;
        private set => SetProperty(ref removeText, value);
    }

    public string AdvancedOptionsTitleText
    {
        get => advancedOptionsTitleText;
        private set => SetProperty(ref advancedOptionsTitleText, value);
    }

    public string FilenameTemplateLabelText
    {
        get => filenameTemplateLabelText;
        private set => SetProperty(ref filenameTemplateLabelText, value);
    }

    public string FilenameTemplateDescriptionText
    {
        get => filenameTemplateDescriptionText;
        private set => SetProperty(ref filenameTemplateDescriptionText, value);
    }

    public string ConcurrencyLabelText
    {
        get => concurrencyLabelText;
        private set => SetProperty(ref concurrencyLabelText, value);
    }

    public string ConcurrencyDescriptionText
    {
        get => concurrencyDescriptionText;
        private set => SetProperty(ref concurrencyDescriptionText, value);
    }

    public string SkipProcessedLabelText
    {
        get => skipProcessedLabelText;
        private set => SetProperty(ref skipProcessedLabelText, value);
    }

    public string SkipProcessedDescriptionText
    {
        get => skipProcessedDescriptionText;
        private set => SetProperty(ref skipProcessedDescriptionText, value);
    }

    public string OpenFolderWhenDoneLabelText
    {
        get => openFolderWhenDoneLabelText;
        private set => SetProperty(ref openFolderWhenDoneLabelText, value);
    }

    public string OpenFolderWhenDoneDescriptionText
    {
        get => openFolderWhenDoneDescriptionText;
        private set => SetProperty(ref openFolderWhenDoneDescriptionText, value);
    }

    public string SizeSectionTitleText
    {
        get => sizeSectionTitleText;
        private set => SetProperty(ref sizeSectionTitleText, value);
    }

    public string SizeSectionHintText
    {
        get => sizeSectionHintText;
        private set => SetProperty(ref sizeSectionHintText, value);
    }

    public string BackgroundSectionTitleText
    {
        get => backgroundSectionTitleText;
        private set => SetProperty(ref backgroundSectionTitleText, value);
    }

    public string BackgroundSectionHintText
    {
        get => backgroundSectionHintText;
        private set => SetProperty(ref backgroundSectionHintText, value);
    }

    public string SizeSummaryText
    {
        get => sizeSummaryText;
        private set => SetProperty(ref sizeSummaryText, value);
    }

    public string FileColumnText
    {
        get => fileColumnText;
        private set => SetProperty(ref fileColumnText, value);
    }

    public string SizeColumnText
    {
        get => sizeColumnText;
        private set => SetProperty(ref sizeColumnText, value);
    }

    public string BackgroundColumnText
    {
        get => backgroundColumnText;
        private set => SetProperty(ref backgroundColumnText, value);
    }

    public string StatusColumnText
    {
        get => statusColumnText;
        private set => SetProperty(ref statusColumnText, value);
    }

    public string SelectedCountText
    {
        get => selectedCountText;
        private set => SetProperty(ref selectedCountText, value);
    }

    public int CompletedCount
    {
        get => completedCount;
        private set => SetProperty(ref completedCount, value);
    }

    public int ProcessingCount
    {
        get => processingCount;
        private set => SetProperty(ref processingCount, value);
    }

    public int WaitingCount
    {
        get => waitingCount;
        private set => SetProperty(ref waitingCount, value);
    }

    public int FailedCount
    {
        get => failedCount;
        private set => SetProperty(ref failedCount, value);
    }

    public int SelectedCount => QueueItems.Count(item => item.IsSelected);

    public bool AreAllItemsSelected
    {
        get => QueueItems.Count > 0 && QueueItems.All(item => item.IsSelected);
        set => SetAllSelection(value);
    }

    public string WindowTitle => Title;

    public string WindowSubtitle => Subtitle;

    public string PageStatusBadge => StatusText;

    protected override void RefreshLocalizedText()
    {
        Title = L(Localization.Shell.Navigation.BatchProcessing);
        Subtitle = L(Localization.Batch.Page.Subtitle);
        Breadcrumb = L(Localization.Batch.Page.Breadcrumb);
        StatusText = L(Localization.Common.States.Ready);
        OnPropertyChanged(nameof(PageStatusBadge));
        OnPropertyChanged(nameof(ConcurrencyText));
        DropTitleText = L(Localization.Batch.Labels.DropTitle);
        DropDescriptionText = L(Localization.Batch.Labels.DropDescription);
        QueueTitleText = L(Localization.Batch.Labels.QueueTitle);
        QueueHintText = QueueSummaryText;
        AddImageText = L(Localization.Batch.Page.AddImage);
        SelectFileText = L(Localization.Common.Actions.SelectFile);
        ExportAllText = L(Localization.Batch.Labels.ExportAll);
        ExportZipText = L(Localization.Batch.Labels.ExportZip);
        UniformSizeText = L(Localization.Batch.Labels.UniformSize);
        UniformBackgroundText = L(Localization.Batch.Labels.UniformBackground);
        RemoveText = L(Localization.Batch.Labels.Remove);
        AdvancedOptionsTitleText = L(Localization.Batch.Page.AdvancedOptions);
        FilenameTemplateLabelText = L(Localization.Batch.Labels.FilenameTemplate);
        FilenameTemplateDescriptionText = L(Localization.Batch.Descriptions.FilenameTemplate);
        ConcurrencyLabelText = L(Localization.Batch.Labels.Concurrency);
        ConcurrencyDescriptionText = L(Localization.Batch.Descriptions.Concurrency);
        SkipProcessedLabelText = L(Localization.Batch.Labels.SkipProcessed);
        SkipProcessedDescriptionText = L(Localization.Batch.Descriptions.SkipProcessed);
        OpenFolderWhenDoneLabelText = L(Localization.Batch.Labels.OpenFolderWhenDone);
        OpenFolderWhenDoneDescriptionText = L(Localization.Batch.Descriptions.OpenFolderWhenDone);
        SizeSectionTitleText = L(Localization.Batch.Labels.UniformSizeTitle);
        SizeSectionHintText = L(Localization.Batch.Labels.KeepRatio);
        BackgroundSectionTitleText = L(Localization.Batch.Labels.UniformBackground);
        BackgroundSectionHintText = L(Localization.Batch.Labels.Background);
        FileColumnText = L(Localization.Batch.Labels.File);
        SizeColumnText = L(Localization.Batch.Labels.Size);
        BackgroundColumnText = L(Localization.Batch.Labels.Background);
        StatusColumnText = L(Localization.Batch.Labels.Status);
        BuildOptions();
        UpdateSizeSummaryText();
        RebuildSummary();
    }

    [RelayCommand]
    private void AddImages(IReadOnlyList<string>? paths)
    {
        var inputPaths = paths?.Where(File.Exists).Where(path => !QueueItems.Any(item => string.Equals(item.SourcePath, path, StringComparison.OrdinalIgnoreCase))).Distinct(StringComparer.OrdinalIgnoreCase).Take(Math.Max(0, 200 - QueueItems.Count)).ToArray() ?? [];
        if (inputPaths.Length == 0)
        {
            return;
        }

        if (QueueItems.Count > 0 && QueueItems.All(item => item.SourcePath is null))
        {
            UnhookQueueItems();
            foreach (var item in QueueItems)
            {
                item.Dispose();
            }

            QueueItems.Clear();
        }

        foreach (var path in inputPaths)
        {
            if (Path.GetExtension(path).ToLowerInvariant() is not (".jpg" or ".jpeg" or ".png" or ".webp")) continue;
            try
            {
                var queueItem = CreateQueueItem(path);
                HookQueueItem(queueItem);
                QueueItems.Add(queueItem);
            }
            catch (Exception error) when (error is IOException or ArgumentException or InvalidOperationException)
            {
                StatusText = string.Format(L(Localization.PhotoStudio.Status.PreviewFailed), error.Message);
                var failed = new BatchProcessingItemViewModel(Path.GetFileName(path), error.Message,
                    SelectedSizeOption?.Title ?? string.Empty, SelectedBackgroundOption?.Title ?? string.Empty,
                    ActiveBackgroundColor.ToAvaloniaColor(), L(Localization.Batch.Status.Failed), 0, 0, 0,
                    sourcePath: path, statusKind: "failed");
                HookQueueItem(failed);
                QueueItems.Add(failed);
            }
        }

        ImportPathsText = string.Join(Environment.NewLine, inputPaths);
        OnPropertyChanged(nameof(QueueItems));
        RebuildSummary();
    }

    public void AddImagePaths(IEnumerable<string> paths)
    {
        try
        {
            AddImages(paths.SelectMany(path => Directory.Exists(path)
                ? Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly) : [path]).ToArray());
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            StatusText = string.Format(L(Localization.PhotoStudio.Status.PreviewFailed), error.Message);
        }
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        if (IsBusy) return;
        var removing = QueueItems.Where(item => item.IsSelected).ToArray();
        foreach (var item in removing)
        {
            UnhookQueueItem(item);
            item.Dispose();
            QueueItems.Remove(item);
        }

        RebuildSummary();
    }

    [RelayCommand]
    private void RemoveItem(BatchProcessingItemViewModel? item)
    {
        if (IsBusy || item is null || !QueueItems.Contains(item)) return;
        UnhookQueueItem(item);
        QueueItems.Remove(item);
        item.Dispose();
        RebuildSummary();
    }

    [RelayCommand]
    private async Task ExportAllAsync()
    {
        if (IsBusy) return;
        var exportItems = QueueItems.Where(item => item.IsSelected && item.SourcePath is not null).ToArray();
        if (exportItems.Length == 0) return;
        IsBusy = true;
        try
        {
            var outputFolder = EnsureExportFolder();
            using var workers = new SemaphoreSlim(Concurrency);
            var tasks = exportItems.Select(async (item, index) =>
            {
                var request = BuildBatchRequest(item);
                var target = Path.Combine(outputFolder, BuildOutputFileName(item, request.OutputFormat, index + 1));
                var fingerprint = $"{request}|{new FileInfo(item.SourcePath!).LastWriteTimeUtc.Ticks}|{target}";
                if (SkipProcessed && File.Exists(target) && _processed.TryGetValue(item.SourcePath!, out var previous) && previous == fingerprint) return;
                await workers.WaitAsync(_lifetime.Token);
                try
                {
                    item.SetStatus(L(Localization.Batch.Status.Processing), "processing");
                    item.Progress = 0.2;
                    var result = await _photoStudioService.ProcessAsync(request, _lifetime.Token);
                    await File.WriteAllBytesAsync(target, result.Data, _lifetime.Token);
                    _processed[item.SourcePath!] = fingerprint;
                    item.SetStatus(L(Localization.Batch.Status.Completed), "completed");
                    item.Progress = 1;
                }
                catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
                catch (Exception error)
                {
                    item.SetStatus(L(Localization.Batch.Status.Failed), "failed");
                    StatusText = string.Format(L(Localization.PhotoStudio.Status.SaveFailed), error.Message);
                }
                finally { workers.Release(); }
            });
            await Task.WhenAll(tasks);
            if (exportItems.All(item => item.IsCompleted))
            {
                StatusText = string.Format(L(Localization.PhotoStudio.Status.SavedFormat), outputFolder);
                if (OpenFolderWhenDone) OpenFolder(outputFolder);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) { StatusText = string.Format(L(Localization.PhotoStudio.Status.SaveFailed), error.Message); }
        finally { IsBusy = false; RebuildSummary(); }
    }

    private PhotoProcessingRequest BuildBatchRequest(BatchProcessingItemViewModel item) => new(
        item.SourcePath!, BuildTargetSize(item), ActiveBackgroundColor,
        _preferencesService.Preferences.DefaultFormat, _preferencesService.Preferences.DefaultQuality,
        EmbedSrgb: _preferencesService.Preferences.EmbedSrgb);

    [RelayCommand]
    private async Task ExportZipAsync()
    {
        if (IsBusy) return;
        var exportItems = QueueItems.Where(item => item.IsSelected && item.SourcePath is not null).ToArray();
        if (exportItems.Length == 0)
        {
            StatusText = L(Localization.Common.States.Ready);
            return;
        }

        IsBusy = true;
        try
        {
            var outputFolder = EnsureExportFolder();
            var targetPath = Path.Combine(outputFolder, $"BatchExport_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

            using var stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

            var total = exportItems.Length;
            for (var index = 0; index < exportItems.Length; index++)
            {
                var item = exportItems[index];
                item.SetStatus(L(Localization.Batch.Status.Processing), "processing");
                item.Progress = 0.5;

                var request = BuildBatchRequest(item);

                var result = await _photoStudioService.ProcessAsync(request, _lifetime.Token);
                var entry = archive.CreateEntry(BuildOutputFileName(item, request.OutputFormat, index + 1), CompressionLevel.Optimal);
                await using (var entryStream = entry.Open())
                {
                    await entryStream.WriteAsync(result.Data, 0, result.Data.Length);
                }

                item.SetStatus(L(Localization.Batch.Status.Completed), "completed");
                item.Progress = 1;
                FooterStatusText = $"{L(Localization.Batch.Status.Processing)} · {index + 1} / {total}";
                RebuildSummary();
            }

            StatusText = string.Format(L(Localization.PhotoStudio.Status.SavedFormat), targetPath);
            if (OpenFolderWhenDone)
            {
                OpenFolder(outputFolder);
            }
        }
        catch (Exception ex)
        {
            StatusText = string.Format(L(Localization.PhotoStudio.Status.SaveFailed), ex.Message);
            foreach (var item in exportItems.Where(item => item.Progress < 1))
            {
                item.SetStatus(L(Localization.Batch.Status.Failed), "failed");
            }

            RebuildSummary();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetAllSelection(true);
    }

    private void SetAllSelection(bool isSelected)
    {
        foreach (var item in QueueItems)
        {
            item.IsSelected = isSelected;
        }

        RebuildSummary();
    }

    [RelayCommand]
    private void SelectSize(BatchSizeOptionViewModel? option)
    {
        SelectedSizeOption = option;
    }

    [RelayCommand]
    private void SelectBackground(BackgroundOptionViewModel? option)
    {
        SelectedBackgroundOption = option;
    }

    [RelayCommand]
    private void SetConcurrency(string value)
    {
        if (int.TryParse(value, out var parsed))
        {
            Concurrency = parsed;
        }
    }

    protected override void OnDisposed()
    {
        _lifetime.Cancel();
        UnhookQueueItems();
        foreach (var item in QueueItems)
        {
            item.Dispose();
        }

        base.OnDisposed();
    }

    private void BuildOptions()
    {
        var preferences = _preferencesService.Preferences;
        var sizeToken = SelectedSizeOption?.Token;
        var backgroundToken = SelectedBackgroundOption?.Token;

        SizeOptions =
        [
            new BatchSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.OneInch), "one-inch", 25, 35, false),
            new BatchSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.TwoInch), "two-inch", 35, 49, false),
            new BatchSizeOptionViewModel(L(Localization.PhotoStudio.Sizes.SmallTwoInch), "small-two-inch", 33, 48, false),
            new BatchSizeOptionViewModel(L(Localization.Batch.Labels.FollowOriginal), "original", null, null, true)
        ];

        BackgroundOptions =
        [
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.WhiteName), "white", RgbColor.White, false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.BlueName), "blue", RgbColor.Blue, false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.RedName), "red", new RgbColor(190, 11, 36), false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.GrayLightName), "light-gray", new RgbColor(237, 241, 246), false),
            new BackgroundOptionViewModel(L(Localization.PhotoStudio.Backgrounds.Custom), "custom", preferences.AccentColor, true)
        ];

        SelectedSizeOption = SizeOptions.FirstOrDefault(item => item.Token == sizeToken) ?? SizeOptions[0];
        SelectedBackgroundOption = BackgroundOptions.FirstOrDefault(item => item.Token == backgroundToken) ?? BackgroundOptions[1];
        RefreshQueueLabels();

        OnPropertyChanged(nameof(SizeOptions));
        OnPropertyChanged(nameof(BackgroundOptions));
        UpdateSizeSummaryText();
    }

    private void RefreshQueueLabels()
    {
        if (SelectedSizeOption is null || SelectedBackgroundOption is null) return;
        foreach (var item in QueueItems)
            item.RefreshLabels(SelectedSizeOption.Title, SelectedBackgroundOption.Title, L(item.StatusKind switch
            {
                "completed" => Localization.Batch.Status.Completed,
                "processing" => Localization.Batch.Status.Processing,
                "failed" => Localization.Batch.Status.Failed,
                _ => Localization.Batch.Status.Waiting
            }), ActiveBackgroundColor.ToAvaloniaColor());
    }

    private BatchProcessingItemViewModel CreateQueueItem(string path)
    {
        var sizeText = SelectedSizeOption?.Title ?? string.Empty;
        var backgroundText = SelectedBackgroundOption?.Title ?? string.Empty;
        return BatchProcessingItemViewModel.CreateFromPath(
            path,
            sizeText,
            backgroundText,
            ActiveBackgroundColor.ToAvaloniaColor(),
            L(Localization.Batch.Status.Waiting));
    }

    private PhotoSizeSpec BuildTargetSize(BatchProcessingItemViewModel item)
    {
        var option = SelectedSizeOption ?? SizeOptions[0];
        if (option.IsFollowOriginal)
        {
            var dpi = DefaultDpi;
            var widthMm = item.PixelWidth <= 0
                ? 25
                : item.PixelWidth / (double)dpi * 25.4;
            var heightMm = item.PixelHeight <= 0
                ? 35
                : item.PixelHeight / (double)dpi * 25.4;
            return new PhotoSizeSpec(widthMm, heightMm, dpi);
        }

        return new PhotoSizeSpec(option.WidthMm ?? 25, option.HeightMm ?? 35, DefaultDpi);
    }

    private RgbColor ActiveBackgroundColor =>
        SelectedBackgroundOption?.Color ?? RgbColor.White;

    private string BuildOutputFileName(BatchProcessingItemViewModel item, PhotoOutputFormat outputFormat, int index)
    {
        var tokens = new Dictionary<string, string>
        {
            ["name"] = Path.GetFileNameWithoutExtension(item.FileName),
            ["size"] = SelectedSizeOption?.Title ?? string.Empty,
            ["color"] = SelectedBackgroundOption?.Title ?? string.Empty,
            ["date"] = DateTime.Now.ToString("yyyyMMdd"),
            ["index"] = index.ToString("000")
        };

        var baseName = FilenameTemplateFormatter.Format(FilenameTemplate, tokens);
        return Path.ChangeExtension(baseName, outputFormat.GetExtension());
    }

    private void HookQueueItems()
    {
        foreach (var item in QueueItems)
        {
            HookQueueItem(item);
        }
    }

    private void HookQueueItem(BatchProcessingItemViewModel item)
    {
        item.PropertyChanged += QueueItemOnPropertyChanged;
    }

    private void UnhookQueueItem(BatchProcessingItemViewModel item)
    {
        item.PropertyChanged -= QueueItemOnPropertyChanged;
    }

    private void UnhookQueueItems()
    {
        foreach (var item in QueueItems)
        {
            UnhookQueueItem(item);
        }
    }

    private void QueueItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BatchProcessingItemViewModel.IsSelected) or nameof(BatchProcessingItemViewModel.StatusText) or nameof(BatchProcessingItemViewModel.Progress))
        {
            RebuildSummary();
        }
    }

    private void RebuildSummary()
    {
        CompletedCount = QueueItems.Count(item => item.IsCompleted);
        ProcessingCount = QueueItems.Count(item => item.IsProcessing);
        WaitingCount = QueueItems.Count(item => item.IsWaiting);
        FailedCount = QueueItems.Count(item => item.IsFailed);

        QueueSummaryText = string.Format(
            "{0} {1} · {2} {3} · {4} {5} · {6} {7}",
            L(Localization.Batch.Status.Completed), CompletedCount,
            L(Localization.Batch.Status.Processing), ProcessingCount,
            L(Localization.Batch.Status.Waiting), WaitingCount,
            L(Localization.Batch.Status.Failed), FailedCount);
        QueueHintText = QueueSummaryText;

        FooterStatusText = ProcessingCount > 0
            ? $"{L(Localization.Batch.Status.Processing)} · {ProcessingCount} / {QueueItems.Count}"
            : string.Format(L(Localization.Batch.Labels.ReadyFiles), QueueItems.Count);

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(AreAllItemsSelected));
        SelectedCountText = string.Format(L(Localization.Batch.Labels.SelectedFormat), SelectedCount);
    }

    private string EnsureExportFolder()
    {
        var outputFolder = Path.Combine(_preferencesService.Preferences.DefaultSaveLocation, "BatchExport");
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
    }

    private static void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private void UpdateSizeSummaryText()
    {
        if (SelectedSizeOption is null)
        {
            SizeSummaryText = string.Empty;
            return;
        }

        if (SelectedSizeOption.IsFollowOriginal)
        {
            SizeSummaryText = $"{SelectedSizeOption.Title} · {L(Localization.Batch.Labels.KeepRatio)}";
            return;
        }

        var widthMm = SelectedSizeOption.WidthMm ?? 0;
        var heightMm = SelectedSizeOption.HeightMm ?? 0;
        var widthPx = PhotoSizeSpec.MmToPixels(widthMm, DefaultDpi);
        var heightPx = PhotoSizeSpec.MmToPixels(heightMm, DefaultDpi);
        SizeSummaryText = string.Format(L(Localization.Batch.Labels.SizeSummary), SelectedSizeOption.Title, widthPx, heightPx, DefaultDpi);
    }
}

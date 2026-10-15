using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using Ursa.Controls;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using IEditor.App;
using IEditor.App.Models;
using IEditor.App.Services;
using IEditor.App.ViewModels;
using IEditor.App.Views;
using IEditor.Core.Models;
using IEditor.Core.Services;
using ImageMagick;

var output = Path.GetFullPath(args.FirstOrDefault() ?? "artifacts/ui-verification");
Directory.CreateDirectory(output);
AppBuilder.Configure<App>().UseSkia().WithInterFont()
    .With(new FontManagerOptions { FontFallbacks = [new FontFallback { FontFamily = new FontFamily("Microsoft YaHei UI") }, new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") }] })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
var store = new AppPreferencesStore(Path.Combine(output, "preferences.json"));
store.Save(new AppPreferences { AutoCheckUpdate = false, BatchOpenFolderWhenDone = false,
    DefaultSaveLocation = Path.Combine(output, "exports"), Language = "zh-CN" });
var preferences = new AppPreferencesService(store, new AppThemeManager()); preferences.Apply();
var service = new CountingPhotos(); var updates = new FakeUpdates(); var opened = new List<string>(); var exits = 0; var aboutCalls = 0;
using var model = new MainViewModel(service, preferences, updates, opened.Add, () => exits++, () => aboutCalls++);
var window = new MainWindow { DataContext = model }; window.Show(); Pump();
Check(model.IsPhotoPageActive, "Startup keeps the photo workspace active");
Check(model.BatchPage.QueueItems.Count == 0, "Batch queue starts empty without fabricated files");
Check(model.SettingsPage.ThemeOptions.Count == 3 && model.SettingsPage.ThemeOptions.All(item => item.IsEnabled), "Three usable theme modes");
Check(model.SettingsPage.AccentColorOptions.Count == 8, "Eight shared accent palettes");
var themeControl = window.GetVisualDescendants().OfType<ThemeToggleButton>().Single();
var themeButton = themeControl.GetVisualDescendants().OfType<Button>().Single(button => button.Name == ThemeToggleButton.PART_ThemeButton);
model.SettingsPage.ThemeMode = StudioThemeMode.System;
model.SettingsPage.SelectedAccentColorOption = model.SettingsPage.AccentColorOptions.Single(item => item.Palette == StudioPalette.Aurora);
foreach (var expectedMode in new[] { StudioThemeMode.Light, StudioThemeMode.Dark, StudioThemeMode.System })
{
    themeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
    Check(model.SettingsPage.ThemeMode == expectedMode && store.Load().Palette == StudioPalette.Aurora, "Title theme cycle preserves the active palette");
}
var languageButton = window.FindControl<Button>("TitleLanguageButton")!;
Check(languageButton.Flyout is MenuFlyout { Items.Count: 2 }, "Title language icon contains the actual choices");
foreach (var item in ((MenuFlyout)languageButton.Flyout!).Items.OfType<MenuItem>())
{
    ((MenuFlyout)languageButton.Flyout!).ShowAt(languageButton); Pump();
    var expectedCulture = ((LanguageOptionViewModel)item.CommandParameter!).CultureName;
    item.Command!.Execute(item.CommandParameter); Pump();
    ((MenuFlyout)languageButton.Flyout!).Hide();
    Check(I18nManager.Instance.Culture?.Name == expectedCulture, "Title language menu changes the shared culture");
}
Check(model.Version == UpdateVersion.Display(typeof(App).Assembly) && model.CompileTime != "—", "About uses the actual application metadata");
Check(model.SettingsPage.VersionBadgeText == model.Version, "Settings version matches About");
model.ShowAboutCommand.Execute(null); Check(aboutCalls == 1, "About entry reaches its window service");
foreach (var palette in Enum.GetValues<StudioPalette>())
{
    model.SettingsPage.SelectedAccentColorOption = model.SettingsPage.AccentColorOptions.Single(item => item.Palette == palette);
    foreach (var mode in new[] { AppThemeMode.Light, AppThemeMode.Dark })
    {
        model.SettingsPage.SelectedThemeOption = model.SettingsPage.ThemeOptions.Single(item => item.Value == mode); Pump();
        Check(Application.Current!.RequestedThemeVariant == StudioTheme.GetVariant(palette, mode == AppThemeMode.Dark), "Shared theme applied: " + palette + "/" + mode);
        var saved = store.Load(); Check(saved.Palette == palette && saved.ThemeMode == mode, "AOT-safe theme preferences round trip");
        Capture(window, "photo-" + palette + "-" + mode + ".png");
    }
}
model.SettingsPage.SelectedThemeOption = model.SettingsPage.ThemeOptions.Single(item => item.Value == AppThemeMode.System);
Check(store.Load().ThemeMode == AppThemeMode.System, "System theme persists independently");
model.SettingsPage.SelectedAccentColorOption = model.SettingsPage.AccentColorOptions.Single(item => item.Palette == StudioPalette.DotNet);
model.SetThemeLightCommand.Execute(null);
var about = new AboutWindow { DataContext = model }; about.Show();
foreach (var culture in new[] { "zh-CN", "en-US", "zh-CN", "en-US" })
{
    model.SettingsPage.SelectedLanguageOption = model.SettingsPage.LanguageOptions.Single(item => item.CultureName == culture); Pump();
    Check(ToolTip.GetTip(themeButton) is TextBlock tip && tip.Text == I18nManager.Instance.GetResource(Localization.Common.Theme.Light), "Existing Ursa tooltip follows the shared culture");
    Check(store.Load().Language == culture && I18nManager.Instance.Culture?.Name == culture, "Single global language persists: " + culture);
    Check(model.SettingsPage.SelectedThemeOption is not null && model.SettingsPage.SelectedAccentColorOption is not null && model.SettingsPage.SelectedLanguageOption is not null, "Appearance selections remain visible after translation refresh");
    Check(window.GetVisualDescendants().OfType<ListBox>().Single(list => list.Classes.Contains("titlePaletteSwatches")).SelectedItem is not null, "Title palette selection survives translation refresh");
    Check(about.Title == I18nManager.Instance.GetResource(Localization.About.Text.Title), "Existing About window switches language");
    Check(about.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == model.Version), "Actual version is rendered in About");
    Check(window.GetVisualDescendants().OfType<Menu>().Single().Items.OfType<MenuItem>().FirstOrDefault()?.Header?.ToString() == I18nManager.Instance.GetResource(Localization.Common.Menu.File), "Existing menu updates its language");
    Check(window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == model.PhotoPage.SourceSectionTitle), "Existing photo card updates its language");
    Capture(about, "about-" + culture + ".png");
    model.NavigateSettingsCommand.Execute(null); Pump();
    Check(window.GetVisualDescendants().OfType<ScrollViewer>().All(scroll => scroll.Offset.Y == 0), "Settings opens at the top after localization refresh");
    Capture(window, "settings-" + culture + ".png");
    model.NavigateBatchProcessingCommand.Execute(null); Pump(); Capture(window, "batch-empty-" + culture + ".png");
    model.NavigatePhotoStudioCommand.Execute(null);
}
about.Close();
Capture(window, "photo-empty-en-US.png");
// 所有图像与导出位于传入的隔离目录，不读取或修改用户图片。
var inputs = Path.Combine(output, "inputs"); Directory.CreateDirectory(inputs);
var sourcePaths = new string[3];
for (var index = 0; index < 3; index++)
{
    using var image = new MagickImage(MagickColors.White, 180, 240);
    using var subject = new MagickImage(new MagickColor("#324B71"), 80, 160);
    image.Composite(subject, Gravity.Center, CompositeOperator.Over);
    sourcePaths[index] = Path.Combine(inputs, "sample-" + index + ".png");
    image.Write(sourcePaths[index]);
}
var photo = model.PhotoPage; photo.SetSourcePath(Path.Combine(inputs, "sample-0.png")); WaitUntil(() => !photo.IsBusy && photo.PreviewBitmap is not null);
Check(photo.PreviewBitmap!.PixelSize.Width == 295 && photo.PreviewBitmap.PixelSize.Height == 413, "Real image preview matches one-inch 300 DPI output");
// 先暂停防抖预览，再变更一组参数，避免定时器与显式预览争用导出。
photo.BeginPreviewInteraction(); WaitUntil(() => !photo.IsBusy);
photo.SelectedSizeOption = photo.SizeOptions.Single(item => item.IsCustom); photo.WidthMm = 30; photo.HeightMm = 40;
photo.Dpi = 150; photo.OutputFormat = PhotoOutputFormat.Png; photo.ShowGuides = false;
Complete(photo.GeneratePreviewCommand.ExecuteAsync(null));
photo.OutputPath = Path.Combine(output, "exports", "single.png"); var save = photo.SaveExportAsync(); Complete(save); Check(save.Result, "Single export succeeds: " + photo.StatusText);
using (var exported = new MagickImage(photo.OutputPath))
{
    Check(exported.Width == 177 && exported.Height == 236, "Changing size and DPI changes the exported pixels");
    Check(System.Text.Encoding.Latin1.GetString(File.ReadAllBytes(photo.OutputPath!)).Contains("sRGB"), "sRGB output includes its PNG color-space chunk");
}
Capture(window, "photo-loaded-en-US.png");
photo.ResetCommand.Execute(null); service.ResetCounters(); model.NavigateBatchProcessingCommand.Execute(null); var batch = model.BatchPage;
batch.AddImagePaths(sourcePaths); batch.AddImagePaths(sourcePaths);
Check(batch.QueueItems.Count == 3, "Import deduplicates real image paths");
batch.SelectAllCommand.Execute(null); batch.Concurrency = 2; batch.OpenFolderWhenDone = false;
var sizeToken = batch.SizeOptions[1].Token; batch.SelectedSizeOption = batch.SizeOptions[1];
I18nManager.Instance.Culture = new CultureInfo("zh-CN"); Pump();
Check(batch.SelectedSizeOption!.Token == sizeToken, "Language changes preserve the selected batch size");
I18nManager.Instance.Culture = new CultureInfo("en-US"); Pump();
Complete(batch.ExportAllCommand.ExecuteAsync(null));
Check(batch.CompletedCount == 3 && service.MaximumConcurrent == 2, "Batch processing honors the chosen concurrency (completed=" + batch.CompletedCount + ", maximum=" + service.MaximumConcurrent + ")");
var count = service.Calls; Complete(batch.ExportAllCommand.ExecuteAsync(null));
Check(service.Calls == count, "Skip processed avoids unchanged work");
batch.SelectedSizeOption = batch.SizeOptions[0]; Complete(batch.ExportAllCommand.ExecuteAsync(null));
Check(service.Calls == count + 3, "Changed settings invalidate the processed cache");
Complete(batch.ExportZipCommand.ExecuteAsync(null));
using (var zip = ZipFile.OpenRead(Directory.GetFiles(Path.Combine(output, "exports", "BatchExport"), "*.zip").OrderByDescending(File.GetLastWriteTimeUtc).First()))
    Check(zip.Entries.Count == 3, "ZIP export contains the selected actual files");
Capture(window, "batch-loaded-en-US.png");
VerifyUpdates();
model.SetThemeDarkCommand.Execute(null); Pump(); Capture(window, "batch-loaded-dark.png");
batch.RemoveItemCommand.Execute(batch.QueueItems[0]); Check(batch.QueueItems.Count == 2, "Row removal releases only the requested image");
var corrupt = Path.Combine(inputs, "broken.png"); File.WriteAllText(corrupt, "invalid image");
batch.AddImagePaths([corrupt]); Check(batch.QueueItems.Last().IsFailed && batch.QueueItems.Count == 3, "Corrupt images stay visible as failed rows");
window.Close(); Console.WriteLine("IEditor UI verification passed: " + output);

void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); Console.WriteLine("PASS " + message); }
void Pump() { var timer = Stopwatch.StartNew(); do { Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Thread.Sleep(10); } while (timer.ElapsedMilliseconds < 200); }
void WaitUntil(Func<bool> condition) { var timer = Stopwatch.StartNew(); while (!condition() && timer.ElapsedMilliseconds < 15000) Pump(); Check(condition(), "Asynchronous image work completes"); Pump(); }
void Complete(Task task) { WaitUntil(() => task.IsCompleted); task.GetAwaiter().GetResult(); }
void Capture(Window target, string name)
{
    Pump();
    using var image = target.CaptureRenderedFrame() ?? throw new InvalidOperationException("Missing rendered frame");
    using (var stream = File.Create(Path.Combine(output, name))) image.Save(stream, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
    var lines = new List<string> { "region,x,y,width,height" };
    foreach (var key in new[] { "ie-topbar", "ie-statusbar", "ie-stage", "ie-card", "ie-card-header", "ie-row" })
    {
        var index = 0;
        foreach (var control in target.GetVisualDescendants().OfType<Control>().Where(control => control.IsEffectivelyVisible && control.Classes.Contains(key)))
        {
            if (control.TranslatePoint(default, target) is not { } point) continue;
            lines.Add(FormattableString.Invariant($"{key}-{index++},{point.X:0.###},{point.Y:0.###},{control.Bounds.Width:0.###},{control.Bounds.Height:0.###}"));
        }
    }
    File.WriteAllLines(Path.Combine(output, Path.GetFileNameWithoutExtension(name) + ".geometry.csv"), lines);
}
void VerifyUpdates()
{
    var vm = model.SettingsPage.Update;
    Complete(vm.CheckAsync()); Check(vm.State == UpdateState.Current, "Current release state");
    updates.Result = UpdateCheckResult.Failed("offline"); Complete(vm.CheckAsync()); Check(vm.State == UpdateState.Failed && vm.StatusText.Contains("offline"), "Visible network failure");
    updates.ThrowDownload = false;
    updates.Result = new(new UpdateInfo(new Version(9, 0), "v9.0", "IEditor", "Notes", "https://example.test/release", "https://example.test/setup.exe", "setup.exe", "https://example.test/setup.exe.sha256"), true, null);
    Complete(vm.CheckAsync()); Check(vm.State == UpdateState.Available, "New release enables download");
    Complete(vm.AdvanceAsync()); Check(vm.State == UpdateState.Downloaded && updates.Downloads == 1, "Verified download enables installation");
    Complete(vm.AdvanceAsync()); Check(vm.State == UpdateState.Installed && exits == 0, "Manual installation keeps the app running");
    updates.ExitAfterInstall = true; Complete(vm.CheckAsync()); Complete(vm.AdvanceAsync()); Complete(vm.AdvanceAsync()); Check(exits == 1, "Installer may request app exit");
    updates.ThrowDownload = true; Complete(vm.CheckAsync()); Complete(vm.AdvanceAsync()); Check(vm.State == UpdateState.Failed, "Checksum or transport failure blocks installation");
    updates.Delay = new TaskCompletionSource<UpdateCheckResult>(); var pending = vm.CheckAsync();
    Check(vm.IsBusy && !vm.ActionCommand.CanExecute(null), "Busy update action is disabled"); vm.CancelCommand.Execute(null); Complete(pending); Check(vm.State == UpdateState.Canceled, "Cancellation restores a usable state");
    using var closing = new UpdateViewModel(updates, opened.Add, () => exits++); var closingTask = closing.CheckAsync(); closing.Dispose(); Complete(closingTask); Check(!closing.ActionCommand.CanExecute(null), "Disposal cancels in-flight work");
    updates.Delay = null;
    updates.Result = new(new UpdateInfo(new Version(9, 0), "v9.0", "IEditor", "Notes", "https://example.test/release", null, null, null), true, null);
    Complete(vm.CheckAsync()); Complete(vm.AdvanceAsync()); Check(opened.Last() == "https://example.test/release", "Missing platform assets fall back to the release page");
    Check(updates.CurrentVersion == UpdateVersion.Current(typeof(App).Assembly), "Update checks use the application version, including under NativeAOT");
}
sealed class CountingPhotos : IPhotoStudioService
{
    private readonly MagickPhotoStudioService _service = new(); private int _active;
    public int Calls { get; private set; } public int MaximumConcurrent { get; private set; }
    public void ResetCounters() { Calls = 0; MaximumConcurrent = 0; }
    public async Task<PhotoProcessingResult> ProcessAsync(PhotoProcessingRequest request, CancellationToken cancellationToken = default)
    {
        Calls++; _active++; MaximumConcurrent = Math.Max(MaximumConcurrent, _active);
        try { await Task.Delay(80, cancellationToken); return await _service.ProcessAsync(request, cancellationToken); }
        finally { _active--; }
    }
}
sealed class FakeUpdates : IUpdateService
{
    public UpdateServiceOptions Options { get; } = new() { Repository = "IEditor", ProductName = "IEditor" };
    public Version CurrentVersion { get; private set; } = new(0, 0); public string CurrentVersionText => CurrentVersion.ToString();
    public UpdateCheckResult Result { get; set; } = UpdateCheckResult.Latest();
    public TaskCompletionSource<UpdateCheckResult>? Delay { get; set; }
    public bool ThrowDownload { get; set; } public bool ExitAfterInstall { get; set; } public int Downloads { get; private set; }
    public Task<UpdateCheckResult> CheckAsync(Version? current = null, CancellationToken cancellationToken = default)
    { CurrentVersion = current!; return Delay?.Task.WaitAsync(cancellationToken) ?? Task.FromResult(Result); }
    public Task<UpdateDownloadResult> DownloadAsync(UpdateInfo update, IProgress<UpdateDownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    { if (ThrowDownload) throw new InvalidDataException("checksum"); Downloads++; return Task.FromResult(new UpdateDownloadResult("setup.exe", "setup.exe", 100)); }
    public Task<UpdateInstallResult> InstallAsync(UpdateDownloadResult download, CancellationToken cancellationToken = default) => Task.FromResult(new UpdateInstallResult(UpdateInstallKind.Manual, "Manual installation") { ShouldExitApplication = ExitAfterInstall });
}

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IEditor.Core.Models;
using Ursa.Controls;

namespace IEditor.App.ViewModels.Dialogs;

public partial class ExportDialogViewModel : DialogViewModelBase
{
    private readonly PhotoStudioViewModel _owner;
    private ObservableCollection<FormatOptionViewModel> formatOptions = [];
    private PhotoOutputFormat outputFormat;
    private uint outputQuality;
    private string outputFolder = string.Empty;
    private string fileNameText = string.Empty;
    private string titleText = string.Empty;
    private string fileFormatText = string.Empty;
    private string outputQualityText = string.Empty;
    private string saveLocationText = string.Empty;
    private string filenameText = string.Empty;
    private string browseText = string.Empty;
    private string cancelText = string.Empty;
    private string saveExportText = string.Empty;

    public ExportDialogViewModel(PhotoStudioViewModel owner)
    {
        _owner = owner;
        outputFormat = owner.OutputFormat;
        outputQuality = owner.OutputQuality;
        outputFolder = string.IsNullOrWhiteSpace(owner.OutputFolder)
            ? owner.OutputSuggestedStartPath
            : owner.OutputFolder!;
        fileNameText = string.IsNullOrWhiteSpace(owner.SuggestedOutputFileName)
            ? string.Empty
            : owner.SuggestedOutputFileName;

        InitializeLocalizedText();
    }

    public ObservableCollection<FormatOptionViewModel> FormatOptions
    {
        get => formatOptions;
        private set => SetProperty(ref formatOptions, value);
    }

    public string TitleText
    {
        get => titleText;
        private set => SetProperty(ref titleText, value);
    }

    public string FileFormatText
    {
        get => fileFormatText;
        private set => SetProperty(ref fileFormatText, value);
    }

    public string OutputQualityText
    {
        get => outputQualityText;
        private set => SetProperty(ref outputQualityText, value);
    }

    public string SaveLocationText
    {
        get => saveLocationText;
        private set => SetProperty(ref saveLocationText, value);
    }

    public string FilenameText
    {
        get => filenameText;
        private set => SetProperty(ref filenameText, value);
    }

    public string BrowseText
    {
        get => browseText;
        private set => SetProperty(ref browseText, value);
    }

    public string CancelText
    {
        get => cancelText;
        private set => SetProperty(ref cancelText, value);
    }

    public string SaveExportText
    {
        get => saveExportText;
        private set => SetProperty(ref saveExportText, value);
    }

    public string OutputFolder
    {
        get => outputFolder;
        set => SetProperty(ref outputFolder, value);
    }

    public string FileNameText
    {
        get => fileNameText;
        set
        {
            if (SetProperty(ref fileNameText, value))
            {
                OnPropertyChanged(nameof(NormalizedFileName));
            }
        }
    }

    public uint OutputQuality
    {
        get => outputQuality;
        set
        {
            if (SetProperty(ref outputQuality, value))
            {
                OnPropertyChanged(nameof(QualityValueText));
                OnPropertyChanged(nameof(QualitySliderValue));
            }
        }
    }

    public double QualitySliderValue
    {
        get => OutputQuality;
        set => OutputQuality = (uint)Math.Clamp(value, 60, 100);
    }

    public string QualityValueText => OutputQuality.ToString();

    public FormatOptionViewModel? SelectedFormatOption
    {
        get => FormatOptions.FirstOrDefault(item => item.Value == outputFormat);
        set
        {
            if (value is null)
            {
                return;
            }

            if (SetFormat(value.Value))
            {
                UpdateFileNameExtension();
            }
        }
    }

    public string NormalizedFileName => NormalizeFileName(FileNameText, outputFormat.GetExtension());

    protected override void RefreshLocalizedText()
    {
        TitleText = L(Localization.PhotoStudio.Page.ExportImage);
        FileFormatText = L(Localization.PhotoStudio.Labels.FileFormat);
        OutputQualityText = L(Localization.PhotoStudio.Labels.OutputQuality);
        SaveLocationText = L(Localization.PhotoStudio.Labels.SaveLocation);
        FilenameText = L(Localization.PhotoStudio.Labels.Filename);
        BrowseText = L(Localization.Common.Actions.Browse);
        CancelText = L(Localization.Common.Actions.Cancel);
        SaveExportText = L(Localization.Common.Actions.SaveExport);
        RebuildFormatOptions();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ApplyToOwner();
        if (await _owner.SaveExportAsync())
        {
            RequestClose(DialogResult.OK);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose(DialogResult.Cancel);
    }

    private void ApplyToOwner()
    {
        _owner.OutputFormat = outputFormat;
        _owner.OutputQuality = OutputQuality;
        _owner.OutputPath = Path.Combine(OutputFolder, NormalizedFileName);
    }

    private bool SetFormat(PhotoOutputFormat format)
    {
        if (outputFormat == format)
        {
            return false;
        }

        outputFormat = format;

        foreach (var option in FormatOptions)
        {
            option.IsSelected = option.Value == format;
        }

        OnPropertyChanged(nameof(SelectedFormatOption));
        OnPropertyChanged(nameof(NormalizedFileName));
        return true;
    }

    private void RebuildFormatOptions()
    {
        var selected = outputFormat;
        FormatOptions =
        [
            new FormatOptionViewModel(L(Localization.Common.Formats.Jpg), PhotoOutputFormat.Jpeg),
            new FormatOptionViewModel(L(Localization.Common.Formats.Png), PhotoOutputFormat.Png)
        ];

        OnPropertyChanged(nameof(FormatOptions));
        SetFormat(selected);
    }

    private void UpdateFileNameExtension()
    {
        var normalized = NormalizeFileName(FileNameText, outputFormat.GetExtension());
        if (!string.Equals(FileNameText, normalized, StringComparison.Ordinal))
        {
            FileNameText = normalized;
        }

        OnPropertyChanged(nameof(NormalizedFileName));
    }

    private static string NormalizeFileName(string value, string extension)
    {
        var candidate = Path.GetFileName(value.Trim());
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "证件照";
        }

        return Path.ChangeExtension(candidate, extension);
    }
}

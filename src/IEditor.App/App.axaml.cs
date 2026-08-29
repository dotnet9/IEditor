using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IEditor.App.ViewModels;
using IEditor.App.Views;
using IEditor.Core.Services;

namespace IEditor.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var photoService = new MagickPhotoStudioService();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(photoService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

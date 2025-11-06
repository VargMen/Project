using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace VargPlot
{
    public partial class App : Application
    {
        public static IServiceProvider Services = null!;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Services = DI.Build();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                DataContext = Services.GetRequiredService<MainViewModel>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;


namespace VargPlot;

public static class DI
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        // Core singletons
        services.AddSingleton<AppState>();

        //// Services
        services.AddTransient<PlotViewModel>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        //services.AddTransient<ChartsViewModel>();

        return services.BuildServiceProvider();
    }
}

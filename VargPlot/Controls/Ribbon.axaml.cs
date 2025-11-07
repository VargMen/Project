using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace VargPlot;

public partial class Ribbon : UserControl
{
    private MainViewModel? VM => DataContext as MainViewModel;

    public Ribbon()
    {
        InitializeComponent();
    }

    private void OnStartSimulationClick(object? sender, RoutedEventArgs e)
    {
        VM.plotVM.RestartPlot(false);
        VM.viewAvaPlot.Setup();
    }

    private void OnLoadPolygramClick(object? sender, RoutedEventArgs e)
    {
        VM.plotVM.RestartPlot(true);
        VM.viewAvaPlot.Setup();
    }

    private void OnPauseClick(object? sender, RoutedEventArgs e)
    {
        //VM.plotVM.
    }

    // --- Сервіс ---
    private void OnNewClick(object? sender, RoutedEventArgs e)
    {

    }

    private void OnSettingsClick(object? sender, RoutedEventArgs e)
    {

    }

    // --- Шаблони ---
    private void OnCreateTemplateClick(object? sender, RoutedEventArgs e)
    {

    }

    // --- Мови ---
    private void OnLanguageSettingsClick(object? sender, RoutedEventArgs e)
    {

    }

    // --- Допомога ---
    private void OnHelpClick(object? sender, RoutedEventArgs e)
    {

    }
}
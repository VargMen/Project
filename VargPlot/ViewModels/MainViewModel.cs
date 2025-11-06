using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VargPlot;

public partial class MainViewModel : ObservableObject
{
    public PlotViewModel plotVM;

    [ObservableProperty]
    private int _currWaveformId = 0;

    [ObservableProperty]
    private float _currWaveformScale = 1.0f;

    [ObservableProperty]
    private float _currWaveformOffset = 0.0f;

    public MainViewModel()
    {
    }
    
    public void SetCanvasAndSpacing(Avalonia.Controls.Canvas sliderCanvas, int slidersCount, float sidePadding)
    {
        plotVM = new PlotViewModel(slidersCount);

        //float height = (float)sliderCanvas.Bounds.Height;
        float height = 600.0f; // Temporary fix for initialization issue
        float spacing = height / slidersCount;

        for (int i = 0; i < slidersCount; i++)
        {
            plotVM.SetWaveformOffset(i, i * spacing + 25);
            plotVM.SetWaveformScale(i, 1.0f);
        }
    }
}

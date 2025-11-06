using CommunityToolkit.Mvvm.ComponentModel;

namespace VargPlot;

public partial class AppState : ObservableObject
{
    [ObservableProperty] private float _xScale = 1.0f;
}


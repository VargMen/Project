using CommunityToolkit.Mvvm.ComponentModel;

namespace VargPlot;

public partial class PlotViewModel : ObservableObject
{
    public Plot _plot;
    public List<TimeRect> _timeRects;
    public TimeRectChangesHandler _timeRectChangesHandler;

    public PlotViewModel(int signalsAmount)
    {
        _plot = new Plot(signalsAmount);
        _timeRects = new List<TimeRect>();
        _timeRectChangesHandler = new TimeRectChangesHandler(_timeRects);
    }

    public void SetWaveformOffset(int waveformId, float offset)
    {
        if (waveformId < 0 || waveformId >= _plot.Waveforms.Count) return;
        _plot.Waveforms[waveformId].YOffset = offset;
        //OnPropertyChanged(nameof(_plot));
    }

    public void SetWaveformScale(int waveformId, float scale)
    {
        if (waveformId < 0 || waveformId >= _plot.Waveforms.Count) return;
        _plot.Waveforms[waveformId].Scale = scale;
        //OnPropertyChanged(nameof(_plot));
    }

    public float GetWaveformOffset(int waveformId)
    {
        if (waveformId < 0 || waveformId >= _plot.Waveforms.Count) return 0.0f;
        return _plot.Waveforms[waveformId].YOffset;
    }

    public float GetWaveformScale(int waveformId)
    {
        if (waveformId < 0 || waveformId >= _plot.Waveforms.Count) return 1.0f;
        return _plot.Waveforms[waveformId].Scale;
    }
}

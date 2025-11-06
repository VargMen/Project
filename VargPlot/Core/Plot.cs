using Avalonia;
using Avalonia.Media;

namespace VargPlot;

public class Plot
{
    public List<Waveform> Waveforms;
    public List<float> SampleTimes;
    public double TimeScale = 1.0;

    public Plot(List<Waveform> waveforms, List<float> sampleTimes)
    {
        Waveforms = waveforms;
        SampleTimes = sampleTimes;
    }

    public Plot(int waveformsAmount)
        : this(Waveform.CreateMultiple(waveformsAmount), new List<float>())
    {
    }

    public void AppendChunk(float sampleTime, List<float> samples)
    {
        SampleTimes.Add(sampleTime);
        for (int i = 0; i < Waveforms.Count; ++i)
        {
            Waveforms[i].Samples.Add(samples[i]);
        }
    }
}


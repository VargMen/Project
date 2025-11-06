using Avalonia.Media;

namespace VargPlot;

public class Waveform
{
    public const int SampleCapacity = 100_000;
    public List<float> Samples = new(SampleCapacity);

    public float Scale = 1.0f;
    public float YOffset = 0.0f;
    public Pen Pen = new Pen(Brushes.White, 1.0);

    public float GetTransformedSample(int index)
    {
        return Samples[index] * Scale + YOffset;
    }
    public static List<Waveform> CreateMultiple(int count)
    {
        var list = new List<Waveform>(count);
        for (int i = 0; i < count; ++i)
        {
            list.Add(new Waveform());
            list.Last().Samples.Capacity = SampleCapacity;
            list.Last().Pen = Services.PredefinedPens.All[i % Services.PredefinedPens.All.Count];
        }
        return list;
    }
}


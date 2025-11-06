using Avalonia;
using System.Collections.Generic;

namespace VargPlot;

public static class WaveformRenderPointsGenerator
{
    public static List<Point> GenerateRenderPoints(Waveform waveform, Plot plot, int startIndex, int endIndex, float xScale, float rightPx)
    {
        List<Point> points = new(endIndex - startIndex + 1);
        for (int j = startIndex; j < endIndex; j++)
        {
            points.Add(new Point(plot.SampleTimes[j] * xScale, waveform.GetTransformedSample(j)));
        }

        if (plot.SampleTimes[endIndex] * xScale < rightPx)
        {
            points.Add(new Point(plot.SampleTimes[endIndex] * xScale, waveform.YOffset));
            points.Add(new Point(rightPx, waveform.YOffset));
        }
        return points;
    }
}

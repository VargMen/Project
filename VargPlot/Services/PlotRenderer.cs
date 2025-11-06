using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public static class PlotRenderer
{
    public static void RenderPlot(DrawingContext context, Plot plot, int start, int end, float xScale, float rightPx)
    {
        DrawWaveforms(context, plot, start, end, xScale, rightPx);
    }

    private static void DrawWaveforms(DrawingContext context, Plot plot, int start, int end, float xScale, float rightPx)
    {
        for (int i = 0; i < plot.Waveforms.Count; ++i)
        {
            var points = WaveformRenderPointsGenerator.GenerateRenderPoints(plot.Waveforms[i], plot, start, end, xScale, rightPx);

            var geo = new StreamGeometry();
            using (var g = geo.Open())
            {
                g.BeginFigure(points[0], isFilled: false);
                foreach(var p in points)
                {
                    g.LineTo(p);
                }
                g.EndFigure(isClosed: false);
            }

            context.DrawGeometry(null, plot.Waveforms[i].Pen, geo);
        }
    }

    public static bool HasData(Plot plot)
    {
        return plot is not null
            && plot.SampleTimes.Count > 0;
    }
}


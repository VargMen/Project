using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public static class NumbersRenderer
{
    static readonly Dictionary<int, int> s_secondToValue = new();
    static readonly object s_cacheLock = new();

    public static void RenderNumbers(
    DrawingContext context,
    float minX, float maxX,
    int timeInterval,
    float xScale,
    float panY,
    float height)
    {
        if (xScale < 35)
            return;

        // Which second ticks are visible now
        List<int> timeLabelValues = MathUtils.FindDivisibleIntegers(minX, maxX, timeInterval);

        foreach (int sec in timeLabelValues)
        {
            int valToPrint;
            lock (s_cacheLock)
            {
                if (!s_secondToValue.TryGetValue(sec, out valToPrint))
                {
                    valToPrint = Random.Shared.Next(230, 250);
                    s_secondToValue[sec] = valToPrint;
                }
            }

            // color by cached value
            SolidColorBrush brush = (valToPrint < 247)
                ? new SolidColorBrush(Color.FromArgb(180, 0, 255, 0))
                : new SolidColorBrush(Color.FromArgb(180, 255, 0, 0));

            var pt = new Avalonia.Point(sec * xScale, height - 70 + panY);

            var ft = new FormattedText(
                valToPrint.ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                24,
                brush);

            context.DrawText(ft, pt);
        }
    }
}

using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class TimeLinesRenderer
{
    public static void RenderTimeLines(DrawingContext context, float minX, float maxX, int timeInterval, float xScale, float panY, float height)
    {
        List<int> timeLabelValues = MathUtils.FindDivisibleIntegers(minX, maxX, timeInterval);

        foreach (int val in timeLabelValues)
        {
            int valToPring = val;
            var pt = new Avalonia.Point(val * xScale, height - 50 + panY);
            Avalonia.Media.FormattedText timeTextBuffer = new FormattedText(
                            ToMinutesSeconds(valToPring),
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"),
                            24,
                            Brushes.White);

            context.DrawText(timeTextBuffer, pt);

            var dashedPen = new Pen(
                new SolidColorBrush(Colors.White, 0.5),
                2,
                new DashStyle(new double[] { 6, 4 }, 0)
            );

            RenderUtils.DrawVerticalLine(context, val * xScale + 29, dashedPen, panY, height);
        }
    }

    public static string ToMinutesSeconds(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0; // optional guard

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }
}

using Avalonia;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class TimeRectsRenderer
{
    public static void RenderTimeRects(DrawingContext context, List<TimeRect> timeRects, float minPlotX, float maxPlotX, float xScale, float panY, float height)
    {
        foreach (var timeRect in timeRects)
        {
            if (minPlotX < timeRect.endTime && maxPlotX > timeRect.startTime)
            {
                var rect = new Rect(
                    timeRect.startTime * xScale,
                    panY,
                    (timeRect.endTime - timeRect.startTime) * xScale,
                    height);

                context.DrawRectangle(timeRect.fillColor, timeRect.outlineColor, rect);

                foreach (var timeMarker in timeRect.markers)
                {
                    RenderUtils.DrawVerticalLine(context, timeMarker.time * xScale, timeMarker.pen, panY, height);

                    var flag = new Rect(
                    timeMarker.time * xScale,
                    panY + 40,
                    50,
                    35);

                    context.DrawRectangle(timeMarker.flagColor, timeMarker.flagOutlineColor, flag);
                }
            }
        }
    }
}

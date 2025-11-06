using Avalonia.Media;

namespace VargPlot;

public static class RenderUtils
{
    public static void DrawVerticalLine(DrawingContext ctx, double xCoord, Pen pen, float panY, float height)
    {
        ctx.DrawLine(pen, new Avalonia.Point(xCoord, panY), new Avalonia.Point(xCoord, height - panY));
    }
}

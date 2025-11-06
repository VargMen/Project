using Avalonia.Media;

namespace VargPlot;

public class TimeMarker
{
    public double time = 0.0;
    public Pen pen;
    public SolidColorBrush flagColor;
    public Pen flagOutlineColor;
}

public class TimeRect
{
    public double startTime = 0.0;
    public double endTime = 0.0;
    public SolidColorBrush fillColor;
    public Pen outlineColor;
    public List<TimeMarker> markers;
}

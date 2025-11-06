using Avalonia.Media;

namespace VargPlot;

public class TimeMarker
{
    public float time = 0.0f;
    public Pen pen;
    public SolidColorBrush flagColor;
    public Pen flagOutlineColor;
}

public class TimeRect
{
    public float startTime = 0.0f;
    public float endTime = 0.0f;
    public SolidColorBrush fillColor;
    public Pen outlineColor;
    public List<TimeMarker> markers;
}

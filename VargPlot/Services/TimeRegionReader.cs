using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public static class TimeRegionReader
{
    public static List<TimeRect> Load(string path)
    {
        var rects = new List<TimeRect>();
        if (!File.Exists(path)) return rects;

        bool started = false;
        TimeRect? current = null;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (!started)
            {
                if (line.Equals("start", StringComparison.OrdinalIgnoreCase))
                    started = true;
                continue;
            }

            if (line.Equals("end", StringComparison.OrdinalIgnoreCase))
                break;

            if (line.StartsWith("rect", StringComparison.OrdinalIgnoreCase))
            {
                ParseStartEndType(line, out float s, out float e, out float type);

                var fragColor = type == 0 ?
                    new SolidColorBrush(Color.FromArgb(64, 255, 255, 0)) :
                    new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));

                var outlineColor = type == 0 ?
                    new Pen(Brushes.Yellow, 2) :
                    new Pen(Brushes.Red, 2);

                current = new TimeRect
                {
                    startTime = s,
                    endTime = e,
                    fillColor = fragColor,
                    outlineColor = outlineColor,
                    markers = new List<TimeMarker>()
                };

                continue;
            }

            if (line.StartsWith("markers=", StringComparison.OrdinalIgnoreCase))
            {
                // count is informative; we’ll just read m-lines
                continue;
            }

            if (line.StartsWith("m ", StringComparison.OrdinalIgnoreCase))
            {
                if (current is null) continue;
                ParseMarker(line, out float t, out int flag);

                var fragColor = flag == 0 ?
                    new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)) :
                    new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
                var outlineColor = flag == 0 ?
                    new Pen(Brushes.Red) :
                    new Pen(Brushes.Green);

                current.markers!.Add(new TimeMarker
                {
                    time = t,
                    pen = new Pen(Brushes.White),
                    flagColor = fragColor,
                    flagOutlineColor = outlineColor
                });
                continue;
            }

            if (line.Equals("endrect", StringComparison.OrdinalIgnoreCase))
            {
                if (current is not null) rects.Add(current);
                current = null;
            }
        }

        return rects;
    }

    private static void ParseStartEndType(string line, out float start, out float end, out float type)
    {
        // "rect  start=12.5  end=18.0"
        start = 0; end = 0; type = 0;
        foreach (var tok in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (tok.StartsWith("start=", StringComparison.OrdinalIgnoreCase))
                start = float.Parse(tok.AsSpan(6), CultureInfo.InvariantCulture);
            else if (tok.StartsWith("end=", StringComparison.OrdinalIgnoreCase))
                end = float.Parse(tok.AsSpan(4), CultureInfo.InvariantCulture);
            else if (tok.StartsWith("type=", StringComparison.OrdinalIgnoreCase))
                type = int.Parse(tok.Last().ToString());
        }
    }

    private static void ParseMarker(string line, out float t, out int flag)
    {
        // "m t=13.0 flag=1"
        t = 0; flag = 0;
        foreach (var tok in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (tok.StartsWith("t=", StringComparison.OrdinalIgnoreCase))
                t = float.Parse(tok.AsSpan(2), CultureInfo.InvariantCulture);
            else if (tok.StartsWith("flag=", StringComparison.OrdinalIgnoreCase))
                flag = int.Parse(tok.AsSpan(5), CultureInfo.InvariantCulture);
        }
    }
}
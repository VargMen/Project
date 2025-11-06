using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot.Services;

public static class PredefinedPens
{
    public static readonly List<Pen> All = new()
    {
        new Pen(new SolidColorBrush(Colors.Tomato), 2), // PHI
        new Pen(new SolidColorBrush(Colors.DarkCyan), 2), // PL0
        new Pen(new SolidColorBrush(Colors.Blue), 2), // APT
        new Pen(new SolidColorBrush(Colors.Red), 2),
        new Pen(new SolidColorBrush(Colors.BlueViolet), 2),
        new Pen(new SolidColorBrush(Colors.LightSeaGreen), 2),
        new Pen(new SolidColorBrush(Colors.LightSalmon), 2),
        new Pen(new SolidColorBrush(Colors.Magenta), 2),
        new Pen(new SolidColorBrush(Colors.Lime), 2),
        new Pen(new SolidColorBrush(Colors.DeepSkyBlue), 2)
    };

    public static readonly List<SolidColorBrush> Brushes = new()
    {
        new SolidColorBrush(Colors.Tomato),
        new SolidColorBrush(Colors.DarkCyan),
        new SolidColorBrush(Colors.Blue),
        new SolidColorBrush(Colors.Red),
        new SolidColorBrush(Colors.BlueViolet),
        new SolidColorBrush(Colors.LightSeaGreen),
        new SolidColorBrush(Colors.LightSalmon),
        new SolidColorBrush(Colors.Magenta),
        new SolidColorBrush(Colors.Lime),
        new SolidColorBrush(Colors.DeepSkyBlue),
        new SolidColorBrush(Colors.Aquamarine),
        new SolidColorBrush(Colors.Azure),
        new SolidColorBrush(Colors.Bisque),
        new SolidColorBrush(Colors.DarkOrange),
        new SolidColorBrush(Colors.Goldenrod),
        new SolidColorBrush(Colors.HotPink),
        new SolidColorBrush(Colors.Lavender),
    };

    // Optional helper to get one by index safely:
    public static Pen Get(int index)
        => All[index % All.Count];
}

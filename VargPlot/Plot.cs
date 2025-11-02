using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class Plot
{
    /// <summary>
    /// This object is locked by the Render() methods.
    /// Logic that manipulates the plot (UI inputs or editing data)
    /// can lock this object to prevent rendering artifacts.
    /// </summary>
    public object Sync { get; } = new();

    /// <summary>
    /// In GUI environments this property holds a reference to the interactive plot control
    /// </summary>
    public IPlotControl? PlotControl { get; set; } = null;


}

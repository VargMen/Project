namespace VargPlot;

public interface IPlotControl
{
    /// <summary>
    /// The primary <see cref="Plot"/> displayed by this interactive control
    /// </summary>
    Plot Plot { get; }

    /// <summary>
    /// Render the plot and update the image
    /// </summary>
    void Refresh();

    /// <summary>
    /// This object takes in UI events and contains logic for how to respond to them.
    /// This is a newer alternative to the older <see cref="Interaction"/> system.
    /// </summary>
    Interactivity.UserInputProcessor UserInputProcessor { get; }
}
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VargPlot;

public class UserInputProcessor
{
    private AvaPlot _avaPlot;

    public Point _panOffset = new(0, 0);
    public Point PanOffset => _panOffset;

    private Point _lastMousePosition = new(0, 0);
    public bool _isPanning = false;

    private const double _minXScale = 1;
    private const double _maxXScale = 200.0;
    private float _xScale = 1;
    public float XScale => _xScale;

    private const double _zoomStep = 1.1;
    private double _wheelPanRemainderPx = 0.0;
    private const double _wheelWorldStep = 10.0;

    public bool _isAutoScrolling = true;

    public UserInputProcessor(AvaPlot avaPlot)
    {
        _avaPlot = avaPlot;
    }

    public void ProcessMouseDown(PointerPressedEventArgs e)
    {
        //_isPanning = true;
        _isAutoScrolling = false;
        _lastMousePosition = e.GetPosition(_avaPlot);
        e.Pointer.Capture(_avaPlot);
    }

    public void ProcessMouseUp(PointerReleasedEventArgs e)
    {
        _isPanning = false;
        _isAutoScrolling = true;
        e.Pointer.Capture(null);
    }

    public void ProcessMouseMove(PointerEventArgs e)
    {
        if (!_isPanning) return;

        var pos = e.GetPosition(_avaPlot);
        float dx = (float)(pos.X - _lastMousePosition.X);
        float dy = (float)(pos.Y - _lastMousePosition.Y);

        float currPanX = (float)_panOffset.X + dx;
        double currPanY = _panOffset.Y + dy;
        
        if (currPanX > 0)
            currPanX = 0;

        _panOffset = new Point(currPanX, currPanY);
        _lastMousePosition = pos;

        _avaPlot.Refresh();
    }

    public void ProcessMouseWheel(PointerWheelEventArgs e)
    {
        if (_avaPlot.Bounds.Width <= 0) return;

        if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            double raw = e.Delta.Y;

            // convert a world step into pixels so scroll "speed" is consistent across zooms
            double stepPx = (_wheelWorldStep + _xScale / 10);

            // accumulate fractional pixels to avoid stutter
            double deltaPxAcc = -raw * stepPx + _wheelPanRemainderPx;
            int deltaPxInt = (int)Math.Truncate(deltaPxAcc);            // keep sign
            _wheelPanRemainderPx = deltaPxAcc - deltaPxInt;             // remainder

            if (deltaPxInt != 0)
            {
                _panOffset = new Point(_panOffset.X + deltaPxInt, _panOffset.Y);
            }
        }
        else
        {
            double mouseX = e.GetPosition(_avaPlot).X;      // screen/pixel
            double oldScale = _xScale;
            double desired = e.Delta.Y > 0 ? _zoomStep : 1.0 / _zoomStep;
            double newScale = Math.Clamp(oldScale * desired, _minXScale, _maxXScale);
            if (Math.Abs(newScale - oldScale) < 1.0e-5) return;

            double factor = newScale / oldScale;

            // keep the time under the cursor fixed:
            double newPanX = _panOffset.X + (1 - factor) * (mouseX - _panOffset.X);

            _xScale = (float)newScale;
            _panOffset = new Point(newPanX, _panOffset.Y);
        }

        ClampPanX();
    }

    private void ClampPanX()
    {
        if (_panOffset.X > 0) _panOffset = new Point(0, _panOffset.Y);
        if (_avaPlot.PlotVM._plot.SampleTimes.Count == 0 || _avaPlot.Bounds.Width <= 0) return;

        double lastTime = _avaPlot.PlotVM._plot.SampleTimes.Last();
        double contentWidthPx = lastTime * _xScale;
        double minPanX = Math.Min(0, _avaPlot.Bounds.Width - contentWidthPx);
        if (_panOffset.X < minPanX)
            _panOffset = new Point(minPanX, _panOffset.Y);
    }

    public void ProcessKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _isAutoScrolling = false;
        }
        else if (e.Key == Key.F)
        {
            _avaPlot.PlotVM._timeRectChangesHandler.CreateNewTimeRect(_avaPlot.PlotVM._plot.SampleTimes.Last());
        }
        else if (e.Key == Key.OemPlus)
        {
            _avaPlot.PlotVM._timeRectChangesHandler.CreateTimeMarker(_avaPlot.PlotVM._plot.SampleTimes.Last(), 1);
        }
        else if (e.Key == Key.OemMinus)
        {
            _avaPlot.PlotVM._timeRectChangesHandler.CreateTimeMarker(_avaPlot.PlotVM._plot.SampleTimes.Last(), 0);
        }
        else if (e.Key == Key.Space)
        {
            _avaPlot.ToggleTimers();
        }

    }

    public void ProcessKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _isAutoScrolling = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class AutoScrollingProcessor
{
    private AvaPlot _avaPlot;

    public AutoScrollingProcessor(AvaPlot avaPlot)
    {
        _avaPlot = avaPlot;
    }

    private bool _isAutoScrolling = true;
    private float _autoTargetPanX = 0.0f;       // updated by TryAutoScroll
    private float _autoPanVelX = 0.0f;          // velocity for the smooth damp
    private float _lastAnimSec = 0.0f;

    private const float _marginStartPx = 80f;   // start auto-scroll when newest point gets this close
    private const float _marginStopPx = 120f;  // use a bit bigger margin to avoid chatter (hysteresis)
    private const float _smoothTimeSec = 0.15f; // ~100 ms response (tweak)
    private const float _maxSpeedPxSec = 5000f; // clamp excessive speeds (tweak)

    public void TryAutoScroll()
    {
        if (_avaPlot.Bounds.Width <= 0) return;
        if (_avaPlot.PlotVM._plot.SampleTimes.Count == 0 || _avaPlot.PlotVM._plot.SampleTimes.Count == 0) return;

        float minWorldX = (-_avaPlot.PanX) / _avaPlot.XScale;
        float viewWidthWorld = (float)_avaPlot.Bounds.Width / _avaPlot.XScale;
        float rightWorld = minWorldX + viewWidthWorld;

        float lastTime = _avaPlot.PlotVM._plot.SampleTimes.Last(); // most recent world X

        float marginStartWorld = _marginStartPx / _avaPlot.XScale;
        float marginStopWorld = _marginStopPx / _avaPlot.XScale;

        if (lastTime > rightWorld - marginStopWorld)
        {
            // Keep newest point at ~_marginStopPx from the right edge
            float newLeftWorld = lastTime - viewWidthWorld + marginStopWorld;
            float targetPanX = -newLeftWorld * _avaPlot.XScale;

            // Don’t allow panning into positive (empty space on the left)
            _autoTargetPanX = Math.Min(0.0f, targetPanX);
        }

        AnimatePan();
    }

    private static float SmoothDamp(float current, float target, ref float currentVelocity,
                                 float smoothTime, float maxSpeed, float deltaTime)
    {
        smoothTime = Math.Max(0.0001f, smoothTime);
        float omega = 2.0f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = current - target;
        float maxChange = maxSpeed * smoothTime;
        change = Math.Clamp(change, -maxChange, maxChange);

        float temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;

        float output = target + (change + temp) * exp;

        // Prevent overshoot
        if ((target - current > 0.0f) == (output > target))
        {
            output = target;
            currentVelocity = 0.0f;
        }
        return output;
    }

    private void AnimatePan()
    {
        float now = (float)_avaPlot.Stopwatch.Elapsed.TotalSeconds;
        float dt = Math.Max(0.0f, now - _lastAnimSec);
        _lastAnimSec = now;

        if (_isAutoScrolling)
        {
            float newX = SmoothDamp(_avaPlot.PanX, _autoTargetPanX, ref _autoPanVelX,
                                     _smoothTimeSec, _maxSpeedPxSec, dt);

            if (Math.Abs(newX - _avaPlot.PanX) > 0.01) // tiny deadzone
            {
                _avaPlot.UserInputProc._panOffset = new Avalonia.Point(newX, _avaPlot.PanY);
                _avaPlot.Refresh();
            }
        }
        else
        {
            // if user is panning manually, decay velocity so it doesn’t “snap back” later
            _autoPanVelX *= (float)Math.Exp(-6.0 * dt);
        }
    }
}

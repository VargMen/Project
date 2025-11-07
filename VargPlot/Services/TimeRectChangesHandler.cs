using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VargPlot.Services;

namespace VargPlot;

public class TimeRectChangesHandler
{
    public List<TimeRect> _timeRects;
    bool _isTimeRectBeingDrawn = false;
    bool _isRedRectBeingDrawn = false;
    private double kRedRectDuration = 8.0; // seconds

    public TimeRectChangesHandler(List<TimeRect> timeRects)
    {
        _timeRects = timeRects;
    }

    public void UpdateCurrentTimeRectEndTime(float currentTime, float xScale, TimeRegionRecorder recorder)
    {
        if (_isTimeRectBeingDrawn && _timeRects.Count > 0)
        {
            _timeRects[_timeRects.Count - 1].endTime = currentTime;
        }

        if (_isRedRectBeingDrawn && _timeRects.Count > 0)
        {
            _timeRects[_timeRects.Count - 1].endTime = currentTime;

            if (_timeRects[_timeRects.Count - 1].endTime - _timeRects[_timeRects.Count - 1].startTime >= kRedRectDuration)
            {
                _isRedRectBeingDrawn = false;
                int lastIndex = _timeRects.Count - 1;
                recorder.RecordTimeRect(_timeRects[lastIndex - 1]);
                recorder.RecordTimeRect(_timeRects[lastIndex]);
            }
        }
    }
    public void CreateNewTimeRect(float lastTime)
    {
        if (!_isTimeRectBeingDrawn)
        {
            _isTimeRectBeingDrawn = true;
            _timeRects.Add(new TimeRect
            {
                startTime = lastTime,
                endTime = lastTime,
                fillColor = new SolidColorBrush(Color.FromArgb(64, 255, 255, 0)),
                outlineColor = new Pen(Brushes.Yellow, 2),
                markers = new List<TimeMarker>()
            });
        }
        else
        {
            _isTimeRectBeingDrawn = false;
            _isRedRectBeingDrawn = true;
            _timeRects.Add(new TimeRect
            {
                startTime = lastTime,
                endTime = lastTime,
                fillColor = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0)),
                outlineColor = new Pen(Brushes.DarkRed, 2),
                markers = new List<TimeMarker>()
            });
        }
    }

    public void CreateTimeMarker(float time, int state)
    {
        var fragColor = state == 0 ?
            new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)) :
            new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        var outlineColor = state == 0 ?
            new Pen(Brushes.Red) :
            new Pen(Brushes.Green);

        if (_isRedRectBeingDrawn)
        {
            if (_timeRects.Last().markers.Count == 1)
            {
                _timeRects.Last().markers.First().flagColor = fragColor;
                _timeRects.Last().markers.First().flagOutlineColor = outlineColor;
            }
            else
            {
                _timeRects.Last().markers.Add(new TimeMarker
                {
                    time = time,
                    pen = new Pen(Brushes.White),
                    flagColor = fragColor,
                    flagOutlineColor = outlineColor
                });
            }
        }
    }
}

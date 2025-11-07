using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using VargPlot.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VargPlot;

public class AvaPlot : Avalonia.Controls.Control
{
    private CancellationTokenSource? _fileCts;
    private Task? _fileTask;
    private List<SineGenerator> _sineGenerators { get; set; }
    public UserInputProcessor UserInputProc { get; set; }
    public AutoScrollingProcessor AutoScrollProc { get; set; }

    public SignalValuesRecorder? _signalRecorder;

    private DispatcherTimer UiTimer;
    private System.Timers.Timer DataCreatorTimer;

    public Stopwatch Stopwatch;

    public Point Pan => UserInputProc.PanOffset;
    public float PanX => (float)UserInputProc.PanOffset.X;
    public float PanY => (float)UserInputProc.PanOffset.Y;
    public float XScale => UserInputProc.XScale;

    private ConcurrentQueue<Chunk> Pending;

    public MainViewModel VM => (MainViewModel)DataContext!;

    public AvaPlot()
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Setup();
    }

    public void Setup()
    {
        UserInputProc = new(this);
        AutoScrollProc = new(this);

        Focusable = true;

        UiTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(17),
            DispatcherPriority.Render,
            (_, _) => OnUiTick());

        DataCreatorTimer = new System.Timers.Timer(50) { AutoReset = true };
        DataCreatorTimer.Elapsed += (_, __) => OnDataCreatorTick();
        DataCreatorTimer.Start();

        Pending = new();
        Stopwatch = new Stopwatch();
        
        UiTimer.Start();

        if (VM.plotVM.LoadFileMode)
        {
            StartFileMode("PlotData/signal_values.csv");
            VM.plotVM._timeRects = TimeRegionReader.Load("PlotData/time_rects.tr");
        }
        else
        {
            _sineGenerators = SineGenerator.CreateMultiple(VM.plotVM._plot.Waveforms.Count);
            _signalRecorder = new SignalValuesRecorder("PlotData/signal_values.csv");
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (!VM.plotVM.LoadFileMode)
        {
            VM.plotVM._timeRegionRecorder.Dispose();
        }
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Black, Bounds);
        context.PushTransform(Matrix.CreateTranslation(Pan));

        if (PlotRenderer.HasData(VM.plotVM._plot))
        {
            float minX = -PanX;
            float maxX = -PanX + (float)Bounds.Width;

            var (start, end) = GetVisibleIndexRange(VM.plotVM._plot, minX / XScale, maxX / XScale);
            if (end - start < 1)
                return;

            float minViewTime = VM.plotVM._plot.SampleTimes[start];
            float maxViewTime = VM.plotVM._plot.SampleTimes[end];
            float rightPx = (float)(-PanX + Bounds.Width * XScale);
            //Debug.WriteLine(XScale);

            PlotRenderer.RenderPlot(context, VM.plotVM._plot, start, end, XScale, rightPx);
            VM.plotVM._timeRectChangesHandler.UpdateCurrentTimeRectEndTime(VM.plotVM._plot.SampleTimes.Last(), XScale, VM.plotVM._timeRegionRecorder);
            TimeLinesRenderer.RenderTimeLines(context, minViewTime, maxViewTime, GetTimeStep(XScale), XScale, PanY, (float)Bounds.Height);
            TimeRectsRenderer.RenderTimeRects(context, VM.plotVM._timeRects, minViewTime, maxViewTime, XScale, PanY, (float)Bounds.Height);
            NumbersRenderer.RenderNumbers(context, minViewTime, maxViewTime, 1, XScale, PanY, (float)Bounds.Height);
        }
    }

    private void OnUiTick()
    {
        if (UserInputProc._isAutoScrolling && !UserInputProc._isPanning)
            AutoScrollProc.TryDampAutoScroll();

        Refresh();
    }

    private void OnDataCreatorTick()
    {
        if (!VM.plotVM.LoadFileMode)
        {
            float t = (float)Stopwatch.Elapsed.TotalSeconds;
            List<float> values = new List<float>(VM.plotVM._plot.Waveforms.Count);

            foreach (var sineGenerator in _sineGenerators)
            {
                values.Add(sineGenerator.GetValue(t));
            }

            var fakeChunk = new Chunk(t, values);

            Pending.Enqueue(fakeChunk);
        }

        if (!VM.plotVM.LoadFileMode && Pending.TryDequeue(out var chunk))
        {
            _signalRecorder?.RecordChunk(chunk);
            VM.plotVM._plot.AppendChunk(chunk);
        }
    }

    public void Refresh()
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        UserInputProc.ProcessMouseDown(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        UserInputProc.ProcessMouseUp(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        UserInputProc.ProcessMouseMove(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        UserInputProc.ProcessMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        UserInputProc.ProcessKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        UserInputProc.ProcessKeyUp(e);
    }

    private static (int start, int end) GetVisibleIndexRange(Plot plot, float minWorldX, float maxWorldX)
    {
        int start = LowerBound(plot.SampleTimes, minWorldX);
        int end = UpperBound(plot.SampleTimes, maxWorldX);

        start = Math.Clamp(start, 0, plot.SampleTimes.Count - 1);
        end = Math.Min(plot.SampleTimes.Count - 1, end - 1);

        return (start, end);
    }

    private static int LowerBound(List<float> vals, float x)
    {
        int lo = 0, hi = vals.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (vals[mid] < x) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }

    private static int UpperBound(List<float> vals, float x)
    {
        int lo = 0, hi = vals.Count; // [lo, hi)
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (vals[mid] <= x) lo = mid + 1;
            else hi = mid;
        }
        return lo - 1;
    }

    public void StartFileMode(string path)
    {
        // cancel any previous run
        _fileCts?.Cancel();
        _fileCts = new CancellationTokenSource();

        // stream rows -> UI thread -> AppendChunk
        _fileTask = SessionFileLoader.Start(path, VM.plotVM._plot, _fileCts.Token);
    }

    public void StopFileMode()
    {
        _fileCts?.Cancel();
        _fileCts = null;
        _fileTask = null;
    }

    public void ToggleTimers()
    {
        if (DataCreatorTimer.Enabled)
        {
            Stopwatch.Stop();
            DataCreatorTimer.Stop();
        }
        else
        {
            Stopwatch.Start();
            DataCreatorTimer.Start();
        }
    }

    public static int GetTimeStep(float xScale)
    {
        // Handle negative values if needed
        if (xScale < 0)
            return 10; // Or throw an exception based on your requirements

        if (xScale >= 200)
            return 2;
        else if (xScale >= 50)
            return 5;
        else
            return 10;
    }
}


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

namespace VargPlot;

public class AvaPlot : Avalonia.Controls.Control
{
    private bool LoadFileMode = false;
    private CancellationTokenSource? _fileCts;
    private Task? _fileTask;

    public PlotViewModel PlotVM { get; set; }
    private List<SineGenerator> _sineGenerators { get; set; }
    public UserInputProcessor UserInputProc { get; set; }
    public AutoScrollingProcessor AutoScrollProc { get; set; }

    public SessionFileRecorder? _recorder;

    private DispatcherTimer UiTimer;
    private System.Timers.Timer DataCreatorTimer;

    public Stopwatch Stopwatch;

    public Point Pan => UserInputProc.PanOffset;
    public float PanX => (float)UserInputProc.PanOffset.X;
    public float PanY => (float)UserInputProc.PanOffset.Y;
    public float XScale => UserInputProc.XScale;

    private ConcurrentQueue<Chunk> Pending;

    private MainViewModel VM => (MainViewModel)DataContext!;

    public AvaPlot()
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        this.PlotVM = VM.plotVM;
        _sineGenerators = SineGenerator.CreateMultiple(PlotVM._plot.Waveforms.Count);

        UserInputProc = new(this);
        AutoScrollProc = new(this);

        Focusable = true;

        UiTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(17),
            DispatcherPriority.Render,
            (_, _) => OnUiTick());

        DataCreatorTimer = new System.Timers.Timer(50) { AutoReset = true };
        DataCreatorTimer.Elapsed += (_, __) => OnDataCreatorTick();

        Pending = new();
        Stopwatch = new Stopwatch();
        UiTimer.Start();

        if (LoadFileMode)
            StartFileMode("PlotData/session_20251106_051050.csv");
        else
            _recorder = new SessionFileRecorder("PlotData");
        Refresh();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Black, Bounds);
        context.PushTransform(Matrix.CreateTranslation(Pan));

        if (PlotRenderer.HasData(PlotVM._plot))
        {
            float minX = -PanX;
            float maxX = -PanX + (float)Bounds.Width;

            var (start, end) = GetVisibleIndexRange(PlotVM._plot, minX / XScale, maxX / XScale);
            if (end - start < 1)
                return;

            float minViewTime = PlotVM._plot.SampleTimes[start];
            float maxViewTime = PlotVM._plot.SampleTimes[end - 1];
            float rightPx = (float)(-PanX + Bounds.Width * XScale);

            PlotRenderer.RenderPlot(context, PlotVM._plot, start, end - 1, XScale, rightPx);
            PlotVM._timeRectChangesHandler.UpdateCurrentTimeRectEndTime(PlotVM._plot.SampleTimes.Last(), XScale);
            TimeLinesRenderer.RenderTimeLines(context, minViewTime, maxViewTime, 5, XScale, PanY, (float)Bounds.Height);
            TimeRectsRenderer.RenderTimeRects(context, PlotVM._timeRects, minViewTime, maxViewTime, XScale, PanY, (float)Bounds.Height);
        }
    }

    private void OnUiTick()
    {
        if (UserInputProc._isAutoScrolling && !UserInputProc._isPanning)
            AutoScrollProc.TryAutoScroll();

        Refresh();
    }

    private void OnDataCreatorTick()
    {
        if (!LoadFileMode)
        {
            float t = (float)Stopwatch.Elapsed.TotalSeconds;
            List<float> values = new List<float>(PlotVM._plot.Waveforms.Count);

            foreach (var sineGenerator in _sineGenerators)
            {
                values.Add(sineGenerator.GetValue(t));
            }

            var fakeChunk = new Chunk(t, values);

            Pending.Enqueue(fakeChunk);
        }

        if (!LoadFileMode && Pending.TryDequeue(out var chunk))
        {
            _recorder?.RecordChunk(chunk);
            PlotVM._plot.AppendChunk(chunk);
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
        int endExclusive = UpperBound(plot.SampleTimes, maxWorldX) + 1;

        start = Math.Clamp(start, 0, plot.SampleTimes.Count - 1);
        endExclusive = Math.Clamp(endExclusive, 0, plot.SampleTimes.Count);

        //int end = endExclusive - 1;
        if (endExclusive < start) return (0, 0);
        return (start, endExclusive);
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
        _fileTask = SessionFileLoader.Start(path, PlotVM._plot, _fileCts.Token);
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
}


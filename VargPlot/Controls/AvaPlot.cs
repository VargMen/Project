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
    private Task? _loadTask;
    private CancellationTokenSource? _loadCts;

    public PlotViewModel PlotVM { get; set; }
    private List<SineGenerator> _sineGenerators { get; set; }
    public UserInputProcessor UserInputProc { get; set; }
    public AutoScrollingProcessor AutoScrollProc { get; set; }

    public SessionFileRecorder? _recorder;

    private DispatcherTimer UiTimer;
    private System.Timers.Timer DataTimer;

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

        DataTimer = new System.Timers.Timer(50) { AutoReset = true };
        DataTimer.Elapsed += (_, __) => OnDataTick();

        Pending = new();
        Stopwatch = Stopwatch.StartNew();
        DataTimer.Start();
        UiTimer.Start();

        EnableFileMode("PlotData/session_20251106_043146.csv");

        //_recorder = new SessionFileRecorder("PlotData");
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

            PlotRenderer.RenderPlot(context, PlotVM._plot, start, end, XScale, rightPx);
            PlotVM._timeRectChangesHandler.UpdateCurrentTimeRectEndTime(PlotVM._plot.SampleTimes.Last(), XScale);
            TimeLinesRenderer.RenderTimeLines(context, minViewTime, maxViewTime, 5, XScale, PanY, (float)Bounds.Height);
            TimeRectsRenderer.RenderTimeRects(context, PlotVM._timeRects, minViewTime, maxViewTime, XScale, PanY, (float)Bounds.Height);
        }
    }

    private void OnUiTick()
    {
        if (Pending.TryDequeue(out var chunk))
        {
            _recorder?.RecordChunk(chunk.Time, chunk.Values);
            PlotVM._plot.AppendChunk(chunk.Time, chunk.Values);
            
            if (UserInputProc._isAutoScrolling && !UserInputProc._isPanning)
                AutoScrollProc.TryAutoScroll();
        }

        Refresh();
    }

    private void OnDataTick()
    {
        if (LoadFileMode)
        {
            return;
        }
        else
        {
            double t = Stopwatch.Elapsed.TotalSeconds;
            List<float> values = new List<float>(PlotVM._plot.Waveforms.Count);

            foreach (var sineGenerator in _sineGenerators)
            {
                values.Add(sineGenerator.GetValue(t));
            }

            var chunk = new Chunk()
            {
                Time = (float)t,
                Values = values
            };

            Pending.Enqueue(chunk);
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

    public void EnableFileMode(string path)
    {
        if (LoadFileMode) return;
        LoadFileMode = true;

        _loadCts = new CancellationTokenSource();
        _loadTask = SessionFileLoader.Start(path, Pending, _loadCts.Token);
    }

    public void DisableFileMode()
    {
        if (!LoadFileMode) return;
        LoadFileMode = false;

        _loadCts?.Cancel();
        _loadCts = null;
        _loadTask = null;
    }
}


using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;

namespace VargPlot;

public class VerticalSliderPanel : TemplatedControl
{
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, int>(nameof(Count), 10);

    public static readonly StyledProperty<double> SidePaddingProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(SidePadding), 30);

    public static readonly StyledProperty<double> SliderWidthProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(SliderWidth), 45);

    public static readonly StyledProperty<double> SliderHeightProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(SliderHeight), 25);

    public static readonly StyledProperty<double> DiamondSizeProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(DiamondSize), 20);

    public static readonly StyledProperty<double> ValueSensitivityProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(ValueSensitivity), 0.01);

    public static readonly StyledProperty<double> CenteringAxisOffsetProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, double>(nameof(CenteringAxisOffset), -11.5);

    public static readonly StyledProperty<IBrush[]> BrushesProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, IBrush[]>(nameof(Brushes), new IBrush[] { Avalonia.Media.Brushes.LightBlue });

    // external data (bind to your VM collections)
    public static readonly StyledProperty<ObservableCollection<double>> CentersProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, ObservableCollection<double>>(nameof(Centers));

    public static readonly StyledProperty<ObservableCollection<double>> ValuesProperty =
        AvaloniaProperty.Register<VerticalSliderPanel, ObservableCollection<double>>(nameof(Values));

    public int Count { get => GetValue(CountProperty); set => SetValue(CountProperty, value); }
    public double SidePadding { get => GetValue(SidePaddingProperty); set => SetValue(SidePaddingProperty, value); }
    public double SliderWidth { get => GetValue(SliderWidthProperty); set => SetValue(SliderWidthProperty, value); }
    public double SliderHeight { get => GetValue(SliderHeightProperty); set => SetValue(SliderHeightProperty, value); }
    public double DiamondSize { get => GetValue(DiamondSizeProperty); set => SetValue(DiamondSizeProperty, value); }
    public double ValueSensitivity { get => GetValue(ValueSensitivityProperty); set => SetValue(ValueSensitivityProperty, value); }
    public double CenteringAxisOffset { get => GetValue(CenteringAxisOffsetProperty); set => SetValue(CenteringAxisOffsetProperty, value); }
    public IBrush[] Brushes { get => GetValue(BrushesProperty); set => SetValue(BrushesProperty, value); }

    public ObservableCollection<double> Centers { get => GetValue(CentersProperty); set => SetValue(CentersProperty, value); }
    public ObservableCollection<double> Values { get => GetValue(ValuesProperty); set => SetValue(ValuesProperty, value); }

    private Canvas? _host;

    static VerticalSliderPanel()
    {
        CountProperty.Changed.AddClassHandler<VerticalSliderPanel>((p, _) => p.Rebuild());
        SidePaddingProperty.Changed.AddClassHandler<VerticalSliderPanel>((p, _) => p.Relayout());
        BrushesProperty.Changed.AddClassHandler<VerticalSliderPanel>((p, _) => p.Recolor());
        CentersProperty.Changed.AddClassHandler<VerticalSliderPanel>((p, _) => p.SyncFromVM());
        ValuesProperty.Changed.AddClassHandler<VerticalSliderPanel>((p, _) => p.SyncFromVM());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _host = e.NameScope.Find<Canvas>("PART_Canvas");

        if (_host is not null)
        {
            _host.SizeChanged += (_, __) => Relayout();
        }

        EnsureCollections();
        Rebuild();
    }

    private void EnsureCollections()
    {
        if (Centers is null) Centers = new ObservableCollection<double>();
        if (Values is null) Values = new ObservableCollection<double>();
        while (Centers.Count < Count) Centers.Add(0);
        while (Values.Count < Count) Values.Add(0);
    }

    private void Rebuild()
    {
        if (_host is null) return;

        _host.Children.Clear();
        EnsureCollections();

        double usableWidth = Math.Max(0, _host.Bounds.Width - 2 * SidePadding);
        double spacing = Count > 0 ? usableWidth / Count : 0;

        for (int i = 0; i < Count; i++)
        {
            var vs = new VerticalSlider
            {
                Index = i,
                SliderWidth = SliderWidth,
                SliderHeight = SliderHeight,
                DiamondSize = DiamondSize,
                ValueSensitivity = ValueSensitivity,
                CenteringAxisOffset = CenteringAxisOffset,
                TrackHeight = Math.Max(0, _host.Bounds.Height),
                BarBrush = PickBrush(i),
                Width = SliderWidth, // for hit testing
                Height = _host.Bounds.Height
            };

            // bind-ish sync (manual to keep it simple)
            vs.CenterY = Centers[i];
            vs.Value = Values[i];

            vs.PropertyChanged += (_, e) =>
            {
                if (e.Property == VerticalSlider.CenterYProperty) Centers[i] = vs.CenterY;
                else if (e.Property == VerticalSlider.ValueProperty) Values[i] = vs.Value;
            };

            // horizontal placement
            double x = SidePadding + i * spacing + (spacing - SliderWidth) / 2.0;
            Canvas.SetLeft(vs, x);
            Canvas.SetTop(vs, 0);

            _host.Children.Add(vs);
        }
    }

    private void Relayout()
    {
        if (_host is null || _host.Children.Count == 0) return;

        double usableWidth = Math.Max(0, _host.Bounds.Width - 2 * SidePadding);
        double spacing = Count > 0 ? usableWidth / Count : 0;

        for (int i = 0; i < _host.Children.Count; i++)
        {
            if (_host.Children[i] is not VerticalSlider vs) continue;

            double x = SidePadding + i * spacing + (spacing - SliderWidth) / 2.0;
            Canvas.SetLeft(vs, x);

            vs.TrackHeight = Math.Max(0, _host.Bounds.Height);
            vs.SliderWidth = SliderWidth;
            vs.SliderHeight = SliderHeight;
            vs.DiamondSize = DiamondSize;
            vs.CenteringAxisOffset = CenteringAxisOffset;

            // ensure internal visuals follow CenterY
            vs.CenterY = Centers[Math.Min(i, Centers.Count - 1)];
        }
    }

    private void Recolor()
    {
        if (_host is null) return;
        for (int i = 0; i < _host.Children.Count; i++)
            if (_host.Children[i] is VerticalSlider vs)
                vs.BarBrush = PickBrush(i);
    }

    private void SyncFromVM()
    {
        if (_host is null) return;
        EnsureCollections();

        for (int i = 0; i < Math.Min(Count, _host.Children.Count); i++)
        {
            if (_host.Children[i] is VerticalSlider vs)
            {
                vs.CenterY = Centers[i];
                vs.Value = Values[i];
            }
        }
    }

    private IBrush PickBrush(int i)
        => (Brushes is { Length: > 0 }) ? Brushes[i % Brushes.Length] : Avalonia.Media.Brushes.LightBlue;
}

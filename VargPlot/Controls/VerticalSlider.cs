using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace VargPlot;

public partial class VerticalSlider : TemplatedControl
{
    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<VerticalSlider, int>(nameof(Index));

    public static readonly StyledProperty<double> CenterYProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(CenterY));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(Value));

    public static readonly StyledProperty<double> TrackHeightProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(TrackHeight), 200);

    public static readonly StyledProperty<double> SliderWidthProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(SliderWidth), 45);

    public static readonly StyledProperty<double> SliderHeightProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(SliderHeight), 25);

    public static readonly StyledProperty<double> DiamondSizeProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(DiamondSize), 20);

    public static readonly StyledProperty<double> ValueSensitivityProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(ValueSensitivity), 0.01);

    public static readonly StyledProperty<double> CenteringAxisOffsetProperty =
        AvaloniaProperty.Register<VerticalSlider, double>(nameof(CenteringAxisOffset), -11.5);

    public static readonly StyledProperty<IBrush> BarBrushProperty =
        AvaloniaProperty.Register<VerticalSlider, IBrush>(nameof(BarBrush), Brushes.LightBlue);

    public int Index { get => GetValue(IndexProperty); set => SetValue(IndexProperty, value); }
    public double CenterY { get => GetValue(CenterYProperty); set => SetValue(CenterYProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double TrackHeight { get => GetValue(TrackHeightProperty); set => SetValue(TrackHeightProperty, value); }
    public double SliderWidth { get => GetValue(SliderWidthProperty); set => SetValue(SliderWidthProperty, value); }
    public double SliderHeight { get => GetValue(SliderHeightProperty); set => SetValue(SliderHeightProperty, value); }
    public double DiamondSize { get => GetValue(DiamondSizeProperty); set => SetValue(DiamondSizeProperty, value); }
    public double ValueSensitivity { get => GetValue(ValueSensitivityProperty); set => SetValue(ValueSensitivityProperty, value); }
    public double CenteringAxisOffset { get => GetValue(CenteringAxisOffsetProperty); set => SetValue(CenteringAxisOffsetProperty, value); }
    public IBrush BarBrush { get => GetValue(BarBrushProperty); set => SetValue(BarBrushProperty, value); }

    // Template parts
    private Border? _partRect;
    private Border? _partDiamond;

    // drag state
    private bool _draggingRect;
    private double _dragOffsetY;
    private bool _draggingDiamond;
    private double _valuePressY;
    private double _valueBaseAtPress;

    static VerticalSlider()
    {
        CenterYProperty.Changed.AddClassHandler<VerticalSlider>((s, _) => s.UpdateLayoutPositions());
        SliderHeightProperty.Changed.AddClassHandler<VerticalSlider>((s, _) => s.UpdateLayoutPositions());
        DiamondSizeProperty.Changed.AddClassHandler<VerticalSlider>((s, _) => s.UpdateLayoutPositions());
        CenteringAxisOffsetProperty.Changed.AddClassHandler<VerticalSlider>((s, _) => s.UpdateLayoutPositions());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // unhook old
        if (_partRect is not null)
        {
            _partRect.PointerPressed -= Rect_PointerPressed;
            _partRect.PointerMoved -= Rect_PointerMoved;
            _partRect.PointerReleased -= Rect_PointerReleased;
        }
        if (_partDiamond is not null)
        {
            _partDiamond.PointerPressed -= Diamond_PointerPressed;
            _partDiamond.PointerMoved -= Diamond_PointerMoved;
            _partDiamond.PointerReleased -= Diamond_PointerReleased;
        }

        _partRect = e.NameScope.Find<Border>("PART_Rect");
        _partDiamond = e.NameScope.Find<Border>("PART_Diamond");

        if (_partRect is not null)
        {
            _partRect.PointerPressed += Rect_PointerPressed;
            _partRect.PointerMoved += Rect_PointerMoved;
            _partRect.PointerReleased += Rect_PointerReleased;
        }
        if (_partDiamond is not null)
        {
            _partDiamond.PointerPressed += Diamond_PointerPressed;
            _partDiamond.PointerMoved += Diamond_PointerMoved;
            _partDiamond.PointerReleased += Diamond_PointerReleased;
        }

        UpdateLayoutPositions();
    }

    private void UpdateLayoutPositions()
    {
        if (_partRect is null || _partDiamond is null) return;

        // Position rectangle so its center is at CenterY
        double rectTop = CenterY - SliderHeight / 2.0;
        Canvas.SetTop(_partRect, Math.Clamp(rectTop, 0, Math.Max(0, TrackHeight - SliderHeight)) + CenteringAxisOffset);

        // Diamond centered at CenterY
        Canvas.SetTop(_partDiamond, CenterY - DiamondSize / 2.0 + CenteringAxisOffset);
    }

    private void Rect_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_partRect is null) return;
        if (!e.GetCurrentPoint(_partRect).Properties.IsLeftButtonPressed) return;
        if (_draggingDiamond) return;

        _draggingRect = true;
        e.Pointer.Capture(_partRect);

        var p = e.GetPosition(this);
        double currentTop = CenterY - SliderHeight / 2.0;
        _dragOffsetY = p.Y - currentTop;
        e.Handled = true;
    }

    private void Rect_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_draggingRect || _partRect is null) return;

        var p = e.GetPosition(this);
        double newTop = p.Y - _dragOffsetY;
        newTop = Math.Clamp(newTop, 0, Math.Max(0, TrackHeight - SliderHeight));
        CenterY = newTop + SliderHeight / 2.0;

        e.Handled = true;
    }

    private void Rect_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_partRect is null) return;
        _draggingRect = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void Diamond_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_partDiamond is null) return;
        if (!e.GetCurrentPoint(_partDiamond).Properties.IsLeftButtonPressed) return;
        if (_draggingRect) return;

        _draggingDiamond = true;
        e.Pointer.Capture(_partDiamond);

        var p = e.GetPosition(this);
        _valuePressY = p.Y;
        _valueBaseAtPress = Value;

        e.Handled = true;
    }

    private void Diamond_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_draggingDiamond) return;

        var p = e.GetPosition(this);
        double dy = p.Y - _valuePressY; // down = positive
        Value = _valueBaseAtPress + dy * ValueSensitivity; // clamp if needed
        e.Handled = true;
    }

    private void Diamond_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_partDiamond is null) return;
        _draggingDiamond = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
}

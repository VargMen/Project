using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;

namespace VargPlot
{
    public partial class MainWindow : Window
    {
        private const int SliderCount = 10;
        private const float SliderWidth = 45;
        private const float SliderHeight = 25;
        private const float SidePadding = 30;
        private const float DiamondSize = 20;
        private const float ValueSensitivity = 0.01f; // px -> value
        private const float centeringAxisOffset = -11.5f;

        // Drag state for rectangles (move slider)
        private int _dragIndex = -1;
        private float _dragOffsetY;

        // Drag state for diamonds (update value; slider doesn't move)
        private float _valuePressY;
        private float _valueBaseAtPress;

        private readonly List<Border> _rects = new();
        private readonly List<Border> _diamonds = new();

        private MainViewModel VM => (MainViewModel)DataContext!;
        
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            VM.SetCanvasAndSpacing(SliderCanvas, SliderCount, 60);
            SliderCanvas.AttachedToVisualTree += (_, __) =>
            {
                BuildSliders();
            };

            //SliderCanvas.SizeChanged += (_, __) => RepositionAll();
        }

        private void BuildSliders()
        {
            SliderCanvas.Children.Clear();
            _rects.Clear();
            _diamonds.Clear();

            float usableWidth = (float)Math.Max(0, SliderCanvas.Bounds.Width - 2 * SidePadding);
            float spacing = usableWidth / SliderCount;

            for (int i = 0; i < SliderCount; i++)
            {
                // === Rectangle (vertical slider) ===
                var rect = new Border
                {
                    Width = SliderWidth,
                    Height = SliderHeight,
                    Background = Services.PredefinedPens.Brushes[i],
                    CornerRadius = new CornerRadius(4),
                    Tag = i,
                    Cursor = new Cursor(StandardCursorType.SizeNorthSouth)
                };

                float x = SidePadding + i * spacing + (spacing - SliderWidth) / 2.0f;
                Canvas.SetLeft(rect, x);

                double top = VM.plotVM.GetWaveformOffset(i) - 23.5;
                Canvas.SetTop(rect, top);

                rect.PointerPressed += Slider_PointerPressed;
                rect.PointerMoved += Slider_PointerMoved;
                rect.PointerReleased += Slider_PointerReleased;

                SliderCanvas.Children.Add(rect);
                _rects.Add(rect);

                // === Diamond (rhombus handle) ===
                var diamond = new Border
                {
                    Width = DiamondSize,
                    Height = DiamondSize,
                    Background = Brushes.Orange,
                    Tag = i,
                    RenderTransform = new RotateTransform(45),
                    RenderTransformOrigin = new RelativePoint(0.95, 1.7, RelativeUnit.Relative),
                    Cursor = new Cursor(StandardCursorType.SizeNorthSouth)
                };

                CenterDiamond(i, rect, diamond);

                diamond.PointerPressed += Diamond_PointerPressed;
                diamond.PointerMoved += Diamond_PointerMoved;
                diamond.PointerReleased += Diamond_PointerReleased;

                SliderCanvas.Children.Add(diamond);
                _diamonds.Add(diamond);
            }
        }

        private void CenterDiamond(int i, Border rect, Border diamond)
        {
            double x = Canvas.GetLeft(rect);
            double centerY = VM.plotVM.GetWaveformOffset(i);

            Canvas.SetLeft(diamond, x + (SliderWidth - DiamondSize) / 2.0);
            Canvas.SetTop(diamond, centerY - DiamondSize / 2.0 + centeringAxisOffset);
        }

        // ===== Rectangle drag (moves slider) =====
        private void Slider_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border rect) return;
            if (!e.GetCurrentPoint(rect).Properties.IsLeftButtonPressed) return;
            if (_dragIndex >= 0) return; // ignore if diamond is being dragged

            _dragIndex = (int)rect.Tag!;
            e.Pointer.Capture(rect);

            var p = e.GetPosition(SliderCanvas);
            _dragOffsetY = (float)(p.Y - Canvas.GetTop(rect));

            e.Handled = true;
        }

        private void Slider_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragIndex < 0 || sender is not Border rect) return;

            var p = e.GetPosition(SliderCanvas);
            float newTop = (float)p.Y - _dragOffsetY;
            newTop = Math.Clamp(newTop, 0, (float)Math.Max(0, (float)SliderCanvas.Bounds.Height - SliderHeight));

            Canvas.SetTop(rect, newTop + centeringAxisOffset);
            VM.plotVM.SetWaveformOffset(_dragIndex, newTop + SliderHeight / 2.0f);
         

            // keep diamond centered on rect center
            CenterDiamond(_dragIndex, rect, _diamonds[_dragIndex]);
            
            e.Handled = true;
        }

        private void Slider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Pointer.Capture(null);
            _dragIndex = -1;
            e.Handled = true;
            e.Handled = true;
        }

        // ===== Diamond drag (updates value; slider doesn't move) =====
        private void Diamond_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border diamond) return;
            if (!e.GetCurrentPoint(diamond).Properties.IsLeftButtonPressed) return;
            if (_dragIndex >= 0) return; // ignore if rect is being dragged

            _dragIndex = (int)diamond.Tag!;
            e.Pointer.Capture(diamond);

            var p = e.GetPosition(SliderCanvas);
            _valuePressY = (float)p.Y;
            _valueBaseAtPress = VM.plotVM.GetWaveformScale(_dragIndex);

            e.Handled = true;
        }

        private void Diamond_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragIndex < 0) return;

            var p = e.GetPosition(SliderCanvas);
            float dy = (float)p.Y - _valuePressY; // down = positive

            float newValue = _valueBaseAtPress + dy * ValueSensitivity;
            // clamp if needed: newValue = Clamp(newValue, 0, 100);

            VM.plotVM.SetWaveformScale(_dragIndex, newValue);
            e.Handled = true;
        }

        private void Diamond_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Pointer.Capture(null);
            _dragIndex = -1;
            e.Handled = true;
        }
    }
}
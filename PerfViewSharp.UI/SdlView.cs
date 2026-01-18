using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using PerfViewSharp.Gfx;
using PerfViewSharp.Core;
using SDL3;

namespace PerfViewSharp.UI;

public class SdlView : Avalonia.Controls.Control
{
    private readonly GpuContext _context;
    private WriteableBitmap? _bitmap;
    private IntPtr _gpuTexture;
    private DispatcherTimer? _timer;
    private uint _width, _height;
    private bool _isRendering = false;
    
    public ViewState State { get; } = new ViewState();
    private TraceMetadata? _data;
    private TraceEngine _engine = new TraceEngine();

    public string SearchText { get; set; } = "";
    private Point _currentMousePos;
    private TraceBlock? _hoveredBlock;
    private TraceBlock? _selectedBlock;
    private bool _isDragging = false;
    private Point _lastMousePos;

    // 事件
    public event Action<TraceBlock?>? SelectionChanged;
    public event Action? ViewStateChanged; 
    public event Action<List<MethodStats>>? VisibleHotspotsChanged; // 新增：视口热点变化
    
    public TraceMetadata? Metadata => _data;

    private const double RulerHeight = 45;
    private const double BlockHeight = 24;
    private const double ThreadSpacing = 280;

    public SdlView(GpuContext context)
    {
        _context = context;
        _data = _engine.GenerateMockData(2000);
        State.TotalTime = _data.TotalDurationMSec;

        this.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(bounds => {
            State.ViewWidth = bounds.Width; State.ViewHeight = bounds.Height;
            InitializeResources((uint)bounds.Width, (uint)bounds.Height);
        }));

        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += (s, e) => _isDragging = false;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var mousePos = e.GetPosition(this);
        float oldZoom = State.Zoom;
        State.Zoom *= (float)Math.Pow(1.1, e.Delta.Y * 0.05);
        State.Zoom = Math.Clamp(State.Zoom, 0.0001f, 10000.0f);
        double m = State.Zoom / oldZoom;
        State.Offset = new Vector(mousePos.X - (mousePos.X - State.Offset.X) * m, mousePos.Y - (mousePos.Y - State.Offset.Y) * m);
        NotifyViewStateChanged();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            if (_hoveredBlock != _selectedBlock) { _selectedBlock = _hoveredBlock; SelectionChanged?.Invoke(_selectedBlock); }
            _isDragging = true; _lastMousePos = e.GetPosition(this);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        _currentMousePos = e.GetPosition(this);
        if (_isDragging) { State.Offset += (_currentMousePos - _lastMousePos); _lastMousePos = _currentMousePos; NotifyViewStateChanged(); } 
        else { UpdateHover(); }
        InvalidateVisual();
    }

    private void NotifyViewStateChanged()
    {
        ViewStateChanged?.Invoke();
        CalculateVisibleHotspots();
        InvalidateVisual();
    }

    private void CalculateVisibleHotspots()
    {
        if (_data == null) return;

        // 找出当前视口内的所有块
        var hotspots = _data.Blocks
            .Where(b => {
                double x = b.StartTimeMSec * 0.1 * State.Zoom + State.Offset.X;
                double w = b.DurationMSec * 0.1 * State.Zoom;
                return x + w > 0 && x < State.ViewWidth;
            })
            .GroupBy(b => b.Name)
            .Select(g => new MethodStats {
                Name = g.Key,
                TotalDurationMSec = g.Sum(b => b.DurationMSec),
                CallCount = g.Count()
            })
            .OrderByDescending(s => s.TotalDurationMSec)
            .Take(5)
            .ToList();

        VisibleHotspotsChanged?.Invoke(hotspots);
    }

    public void ApplyOffset(Vector newOffset) { State.Offset = newOffset; NotifyViewStateChanged(); }

    private void UpdateHover()
    {
        if (_data == null) return;
        TraceBlock? found = null;
        for (int i = _data.Blocks.Count - 1; i >= 0; i--) {
            var b = _data.Blocks[i];
            double x = b.StartTimeMSec * 0.1 * State.Zoom + State.Offset.X;
            double w = b.DurationMSec * 0.1 * State.Zoom;
            double y = b.ThreadId * ThreadSpacing + b.Depth * (BlockHeight + 2) + State.Offset.Y;
            if (_currentMousePos.X >= x && _currentMousePos.X <= x + w && _currentMousePos.Y >= y && _currentMousePos.Y <= y + BlockHeight) { found = b; break; }
        }
        if (_hoveredBlock != found) { _hoveredBlock = found; Cursor = found != null ? new Cursor(StandardCursorType.Hand) : null; }
    }

    public void TriggerManualJump()
    {
        if (_selectedBlock != null && !string.IsNullOrEmpty(_selectedBlock.SourceFile))
            Process.Start("open", _selectedBlock.SourceFile);
    }

    public void FocusMethod(string name)
    {
        var first = _data?.Blocks.FirstOrDefault(b => b.Name == name);
        if (first != null)
        {
            // 自动平移到该方法位置
            double targetX = (State.ViewWidth / 2) - (first.StartTimeMSec * 0.1 * State.Zoom);
            ApplyOffset(new Vector(targetX, State.Offset.Y));
            _selectedBlock = first;
            SelectionChanged?.Invoke(first);
        }
    }

    private void InitializeResources(uint w, uint h)
    {
        if (w < 10 || h < 10) return;
        if (w == _width && h == _height) return;
        _width = w; _height = h;
        _gpuTexture = _context.CreateTargetTexture(w, h);
        _bitmap = new WriteableBitmap(new PixelSize((int)w, (int)h), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        if (_timer == null) { _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (s, e) => InvalidateVisual()); _timer.Start(); }
        CalculateVisibleHotspots(); // 初始化计算一次
    }

    public override void Render(DrawingContext context)
    {
        if (_bitmap == null || _gpuTexture == IntPtr.Zero || _isRendering || _data == null) return;
        _isRendering = true;
        try {
            using (var buffer = _bitmap.Lock()) { _context.DrawBlocks(_gpuTexture, Enumerable.Empty<SDL.FRect>(), 0, 0, 0, _width, _height, buffer.Address); }
            context.DrawImage(_bitmap, new Rect(0, 0, _width, _height), new Rect(0, 0, Bounds.Width, Bounds.Height));

            var typeface = new Typeface("Arial");
            bool isSearching = !string.IsNullOrWhiteSpace(SearchText);

            foreach (var block in _data.Blocks) {
                double x = block.StartTimeMSec * 0.1 * State.Zoom + State.Offset.X;
                double w = block.DurationMSec * 0.1 * State.Zoom;
                double y = block.ThreadId * ThreadSpacing + block.Depth * (BlockHeight + 2) + State.Offset.Y;
                if (x + w < 0 || x > Bounds.Width || y + BlockHeight < RulerHeight || y > Bounds.Height) continue;

                bool isMatch = isSearching && block.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                double opacity = isSearching ? (isMatch ? 1.0 : 0.1) : 0.8;
                var color = GetBlockColor(block.Name);
                var brush = new SolidColorBrush(color, opacity);
                var pen = isMatch ? new Pen(Brushes.White, 1.2) : (block == _selectedBlock ? new Pen(Brushes.Yellow, 2) : new Pen(Brushes.Black, 0.1));
                context.DrawRectangle(brush, pen, new Rect(x, y, Math.Max(w, 0.5), BlockHeight), 2, 2);
                if (w > 60 && opacity > 0.5) {
                    var ft = new FormattedText(block.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, Brushes.Black);
                    using (context.PushClip(new Rect(x, y, w, BlockHeight))) { context.DrawText(ft, new Point(x + 4, y + 4)); }
                }
            }
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#F0F0F0")), null, new Rect(0, 0, Bounds.Width, RulerHeight));
            DrawTimeRuler(context, typeface);
            if (_hoveredBlock != null) DrawToolTip(context, _hoveredBlock, _currentMousePos, typeface);
        }
        finally { _isRendering = false; }
    }

    private void DrawTimeRuler(DrawingContext context, Typeface typeface) {
        double pixelStep = 100 * 0.1 * State.Zoom;
        while (pixelStep < 60) pixelStep *= 10;
        double startX = State.Offset.X % pixelStep;
        for (double x = startX; x < Bounds.Width; x += pixelStep) {
            double time = (x - State.Offset.X) / (0.1 * State.Zoom);
            context.DrawLine(new Pen(Brushes.Silver, 1), new Point(x, RulerHeight - 10), new Point(x, RulerHeight));
            context.DrawText(new FormattedText($"{time:F0}ms", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, Brushes.Gray), new Point(x + 3, RulerHeight - 25));
        }
    }

    private Color GetBlockColor(string name) {
        uint hash = (uint)name.GetHashCode();
        byte r = (byte)(140 + (hash % 60)); byte g = (byte)(170 + ((hash >> 8) % 60)); byte b = (byte)(140 + ((hash >> 16) % 60));
        return Color.FromArgb(255, r, g, b);
    }

    private void DrawToolTip(DrawingContext context, TraceBlock block, Point pos, Typeface typeface) {
        string text = $"【方法名】: {block.Name}\n耗时: {block.DurationMSec:F2} ms";
        var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.White);
        var rect = new Rect(pos.X + 20, pos.Y + 20, ft.Width + 24, ft.Height + 24);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#F01E1E1E")), new Pen(Brushes.Gray, 1), rect, 8, 8);
        context.DrawText(ft, new Point(rect.X + 12, rect.Y + 12));
    }

    private class AnonymousObserver<T> : IObserver<T> { private readonly Action<T> _onNext; public AnonymousObserver(Action<T> onNext) => _onNext = onNext; public void OnCompleted() { } public void OnError(Exception error) { } public void OnNext(T value) => _onNext(value); }
}

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PerfViewSharp.Core;

namespace PerfViewSharp.UI;

public class MinimapView : Avalonia.Controls.Control
{
    private TraceMetadata? _data;
    private ViewState? _viewState;
    public event Action<Vector>? RequestOffsetChange;

    public void SetData(TraceMetadata? data, ViewState viewState)
    {
        _data = data;
        _viewState = viewState;
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        HandleInteraction(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            HandleInteraction(e.GetPosition(this));
        }
    }

    private void HandleInteraction(Point pos)
    {
        if (_data == null || _viewState == null) return;
        
        double timeRatio = pos.X / Bounds.Width;
        double targetTime = timeRatio * _data.TotalDurationMSec;
        
        // 计算新的 OffsetX
        double newOffsetX = (_viewState.ViewWidth / 2) - (targetTime * 0.1 * _viewState.Zoom);
        RequestOffsetChange?.Invoke(new Vector(newOffsetX, _viewState.Offset.Y));
    }

    public override void Render(DrawingContext context)
    {
        if (_data == null || _viewState == null) return;

        double w = Bounds.Width;
        double h = Bounds.Height;
        double totalTime = _data.TotalDurationMSec;

        context.DrawRectangle(new SolidColorBrush(Color.Parse("#111111")), null, new Rect(0, 0, w, h));

        // 绘制缩影
        var brush = new SolidColorBrush(Color.Parse("#4A90E2"), 0.3);
        foreach (var block in _data.Blocks)
        {
            double x = (block.StartTimeMSec / totalTime) * w;
            double bw = (block.DurationMSec / totalTime) * w;
            double y = (block.ThreadId * (h / 5.0)) % h;
            context.DrawRectangle(brush, null, new Rect(x, y, Math.Max(bw, 1), 2));
        }

        // 绘制视口高亮
        double viewStartTime = -_viewState.Offset.X / (0.1 * _viewState.Zoom);
        double viewEndTime = (_viewState.ViewWidth - _viewState.Offset.X) / (0.1 * _viewState.Zoom);
        
        double vx = (viewStartTime / totalTime) * w;
        double vw = ((viewEndTime - viewStartTime) / totalTime) * w;

        // 限制在边界内
        vx = Math.Clamp(vx, 0, w);
        vw = Math.Min(vw, w - vx);

        var viewportRect = new Rect(vx, 0, Math.Max(vw, 2), h);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#30FFFFFF")), new Pen(Brushes.White, 1), viewportRect);
        
        // 遮罩
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#60000000")), null, new Rect(0, 0, vx, h));
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#60000000")), null, new Rect(vx + vw, 0, w - (vx + vw), h));
    }
}

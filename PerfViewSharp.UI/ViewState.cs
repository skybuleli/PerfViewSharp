using Avalonia;

namespace PerfViewSharp.UI;

public class ViewState
{
    public float Zoom { get; set; } = 1.0f;
    public Vector Offset { get; set; } = new Vector(100, 100);
    public double TotalTime { get; set; } = 10000;
    public double ViewWidth { get; set; }
    public double ViewHeight { get; set; }
}

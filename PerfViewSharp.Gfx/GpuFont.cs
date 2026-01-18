using System;

namespace PerfViewSharp.Gfx;

public class GpuFont : IDisposable
{
    public GpuFont(GpuContext context, string name, int size) { }
    public void DrawText(IntPtr renderer, string text, float x, float y, float r, float g, float b) { }
    public void Dispose() { }
}
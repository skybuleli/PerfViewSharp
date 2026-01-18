using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace PerfViewSharp.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static PerfViewSharp.Gfx.GpuContext? GpuContext { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        GpuContext = new PerfViewSharp.Gfx.GpuContext();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.Exit += (s, e) => GpuContext?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
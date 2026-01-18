using System;
using Avalonia;

namespace PerfViewSharp.UI;

class Program
{
    // Initialization code. Don't use any Avalonia, xaml, mtxc or any other Visual Studio generated code here.
    [STAThread]
    public static void Main(string[] args)
    {
        PerfViewSharp.Gfx.NativeResolver.Register();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
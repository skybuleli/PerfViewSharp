using System.Reflection;
using System.Runtime.InteropServices;

namespace PerfViewSharp.Gfx;

public static class NativeResolver
{
    public static void Register()
    {
        NativeLibrary.SetDllImportResolver(typeof(SDL3.SDL).Assembly, Resolve);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == "SDL3")
        {
            // 优先尝试系统路径和 Homebrew 路径
            var paths = new[]
            {
                "libSDL3.dylib",
                "/opt/homebrew/lib/libSDL3.dylib",
                "/usr/local/lib/libSDL3.dylib"
            };

            foreach (var path in paths)
            {
                if (NativeLibrary.TryLoad(path, out var handle))
                {
                    return handle;
                }
            }
        }
        return IntPtr.Zero;
    }
}

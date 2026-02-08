using System.Windows;

namespace Tray;

internal static class WpfBootstrapper
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        // IMPORTANT: fully qualify WPF Application
        var app = new System.Windows.Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };

        _initialized = true;
    }
}
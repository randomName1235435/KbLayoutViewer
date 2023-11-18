using System;
using System.Runtime.InteropServices;

namespace KbLayoutProtoWpf;

class ProcessFocus
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr WindowHandle);

    public const int SW_RESTORE = 9;
    public const int SW_MINIMIZED = 2;

    public static void FocusProcess()
    {
        var p = System.Diagnostics.Process.GetCurrentProcess();

        IntPtr hWnd = IntPtr.Zero;
        hWnd = p.MainWindowHandle;
        ShowWindowAsync(new HandleRef(null, hWnd), 0);
        ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
        SetForegroundWindow(p.MainWindowHandle);
    }
}
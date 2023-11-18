using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace KbLayoutProtoWpf;

public class GlobalHotkey
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public enum KeyModifiers
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
    }

    private IntPtr hwnd;
    private int hotkeyId;
    private Action hotkeyAction;

    public GlobalHotkey(Window window, KeyModifiers modifiers, System.Windows.Forms.Keys key, Action action)
    {
        this.hwnd = new WindowInteropHelper(window).EnsureHandle();
        this.hotkeyId = this.GetHashCode();
        this.hotkeyAction = action;

        if (!RegisterHotKey(this.hwnd, this.hotkeyId, (uint)modifiers, (uint)key))
        {
            throw new InvalidOperationException("Failed to register global hotkey.");
        }
    }

    public void Unregister()
    {
        if (this.hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(this.hwnd, this.hotkeyId);
            this.hwnd = IntPtr.Zero;
        }
    }
}
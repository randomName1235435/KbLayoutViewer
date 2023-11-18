using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KbLayoutProtoWpf;

public class GlobalKeyboardHook
{
    private readonly Keys _hookedKey;

    public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

    public struct keyboardHookStruct
    {
        public int vkCode;
    }

    const int WH_KEYBOARD_LL = 13;
    const int WM_KEYDOWN = 0x100;
    const int WM_KEYUP = 0x101;
    const int WM_SYSKEYDOWN = 0x104;
    const int WM_SYSKEYUP = 0x105;

    keyboardHookProc khp;

    IntPtr hhook = IntPtr.Zero;


    public event KeyEventHandler KeyDown;

    public event KeyEventHandler KeyUp;

    public GlobalKeyboardHook(Keys hookedKey)
    {
        this._hookedKey = hookedKey;
        this.khp = new keyboardHookProc(this.hookProc);
    }


    ~GlobalKeyboardHook()
    {
        this.Unhook();
    }

    public void Hook()
    {
        IntPtr hInstance = LoadLibrary("User32");
        this.hhook = SetWindowsHookEx(WH_KEYBOARD_LL, this.khp, hInstance, 0);
    }

    public void Unhook()
    {
        UnhookWindowsHookEx(this.hhook);
    }

    public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
    {
        if (code >= 0)
        {
            Keys key = (Keys)lParam.vkCode;
            if (this._hookedKey == key)
            {
                KeyEventArgs kea = new KeyEventArgs(key);
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                {
                    KeyDown(this, kea);
                }
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                {
                    KeyUp(this, kea);
                }
                if (kea.Handled)
                    return 1;
            }
        }
        return CallNextHookEx(this.hhook, code, wParam, ref lParam);
    }

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hInstance);

    [DllImport("user32.dll")]
    static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

    [DllImport("kernel32.dll")]
    static extern IntPtr LoadLibrary(string lpFileName);

}
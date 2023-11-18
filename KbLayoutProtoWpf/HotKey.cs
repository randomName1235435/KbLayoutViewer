﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace KbLayoutProtoWpf;

public sealed class HotKey : IDisposable
{
    private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const int WmHotKey = 0x0312;

    private bool _disposed = false;

    public Key Key { get; private set; }
    public KeyModifier KeyModifiers { get; private set; }
    public Action<HotKey> Action { get; private set; }
    public int Id { get; set; }

    public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
    {
        Key = k;
        KeyModifiers = keyModifiers;
        Action = action;
    }

    public bool Register()
    {
        int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
        Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
        bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

        if (_dictHotKeyToCalBackProc == null)
        {
            _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
        }
        ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
        if (!_dictHotKeyToCalBackProc.ContainsKey(Id))
            _dictHotKeyToCalBackProc.Add(Id, this);


        Debug.Print(result.ToString() + ", " + Id + ", " + virtualKeyCode);
        return result;
    }

    public void Unregister()
    {
        HotKey hotKey;
        if (_dictHotKeyToCalBackProc.TryGetValue(Id, out hotKey))
        {
            UnregisterHotKey(IntPtr.Zero, Id);
            _dictHotKeyToCalBackProc.Remove(Id);
        }
    }

    private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (handled) return;
        if (msg.message != WmHotKey) return;

        HotKey hotKey;

        if (!_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey)) return;

        if (hotKey.Action != null)
        {
            hotKey.Action.Invoke(hotKey);
        }
        handled = true;
    }
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (this._disposed) return;
        
        if (disposing)
        {
            this.Unregister();
        }
        this._disposed = true;
    }
}
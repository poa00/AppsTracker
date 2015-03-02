﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using AppsTracker.Common.Utils;
using System.Collections.Generic;

namespace AppsTracker.Hooks
{
    internal delegate IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam);

    internal sealed class KeyBoardHook : IHook<KeyboardHookArgs>
    {
        #region Fields

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;
        const int VK_CAPITAL = 0x14;

        bool _keyLoggerEnabled = true;
        bool _isDisposed;

        public event EventHandler<KeyboardHookArgs> HookProc;

        KeyboardHookCallback _hookCallBack;
        IntPtr _hookID = IntPtr.Zero;

        KeyboardMap _keyboardMap = new KeyboardMap();

        #endregion

        #region Constructors

        public KeyBoardHook()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != App.Current.Dispatcher.Thread.ManagedThreadId)
                App.Current.Dispatcher.Invoke(SetHook);
            else
                SetHook();
        }

        private void SetHook()
        {
            _hookCallBack = new KeyboardHookCallback(HookCallback);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    _hookID = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Ensure.Condition<InvalidOperationException>(_hookID != IntPtr.Zero, "Failed to set keyboardhook");
                }
            }
        }

        public void EnableHook(bool enable)
        {
            _keyLoggerEnabled = enable;
        }

        #endregion

        #region HookCallBack

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (!_keyLoggerEnabled || code < 0)
                return WinAPI.CallNextHookEx(_hookID, code, wParam, lParam);

            WinAPI.KBDLLHOOKSTRUCT keybStruct = (WinAPI.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.KBDLLHOOKSTRUCT));

            if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
            {
                bool isDownShift = ((WinAPI.GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
                bool isDownCapslock = (WinAPI.GetKeyState(VK_CAPITAL) != 0 ? true : false);
                StringBuilder builder = new StringBuilder(10);
                byte[] lpKeyState = new byte[256];
                if (WinAPI.GetKeyboardState(lpKeyState))
                {
                    if (WinAPI.ToUnicodeEx((uint)keybStruct.vkCode
                                            , (uint)keybStruct.scanCode
                                            , lpKeyState, builder
                                            , builder.Capacity
                                            , 0
                                            , WinAPI.GetKeyboardLayout(0)) != -1)
                    {
                        string keyTextRaw;
                        _keyboardMap.TryGetValue(keybStruct.vkCode, out keyTextRaw);
                        HookProc.InvokeSafely<KeyboardHookArgs>(this, new KeyboardHookArgs(keybStruct.vkCode, builder.ToString(), keyTextRaw));
                    }
                }
            }
            return WinAPI.CallNextHookEx(_hookID, code, wParam, lParam);
        }

        #endregion

        #region IDisposable Members

        ~KeyBoardHook()
        {
            Dispose(false);
            Debug.WriteLine("KeyboardHook Finalizer called");
        }

        private void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);
            if (_isDisposed) return;
            WinAPI.UnhookWindowsHookEx(_hookID);
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private sealed class KeyboardMap : Dictionary<int, string>
        {
            public KeyboardMap()
            {
                Add(8, "Backspace");
                Add(9, "Tab");
                Add(13, "Enter");
                Add(16, "Shift");
                Add(17, "Ctrl");
                Add(18, "Alt");
                Add(19, "Pause");
                Add(20, "Caps lock");
                Add(27, "Escape");
                Add(33, "Page up");
                Add(34, "Page down");
                Add(35, "End");
                Add(36, "Home");
                Add(37, "Left arrow");
                Add(38, "Up arrow");
                Add(39, "Right arrow");
                Add(40, "Down arrow");
                Add(45, "Insert");
                Add(46, "Delete");
                Add(91, "Left window");
                Add(92, "Right window");
                Add(93, "Select key");
                Add(96, "Numpad 0");
                Add(97, "Numpad 1");
                Add(98, "Numpad 2");
                Add(99, "Numpad 3");
                Add(100, "Numpad 4");
                Add(101, "Numpad 5");
                Add(102, "Numpad 6");
                Add(103, "Numpad 7");
                Add(104, "Numpad 8");
                Add(105, "Numpad 9");
                Add(106, "Multiply");
                Add(107, "Add");
                Add(109, "Subtract");
                Add(110, "Decimal point");
                Add(111, "Divide");
                Add(112, "F1");
                Add(113, "F2");
                Add(114, "F3");
                Add(115, "F4");
                Add(116, "F5");
                Add(117, "F6");
                Add(118, "F7");
                Add(119, "F8");
                Add(120, "F9");
                Add(121, "F10");
                Add(122, "F11");
                Add(123, "F12");
                Add(144, "Num lock");
                Add(145, "Scroll lock");
            }
        }
    }

}

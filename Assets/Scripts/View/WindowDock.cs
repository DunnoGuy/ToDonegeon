using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ToDonegeon.View
{
    // Docks the standalone window to a screen edge as an always-on-top strip, so the task list can sit
    // over other applications while they stay interactive
    public static class WindowDock
    {
        public static bool IsSupported => Application.platform == RuntimePlatform.WindowsPlayer;
        public static bool IsDocked { get; private set; }

        public const int DefaultDockWidth = 340;

        private static IntPtr _window;
        private static Rect2D _restoreRect;
        private static bool _hasRestoreRect;

        public static void Toggle(int width = DefaultDockWidth)
        {
            if (IsDocked) Undock();
            else Dock(width);
        }

        public static void Dock(int width = DefaultDockWidth)
        {
            if (!IsSupported) return;
            IntPtr window = Window();
            if (window == IntPtr.Zero) return;

            if (!IsDocked && GetWindowRect(window, out Rect2D current))
            {
                _restoreRect = current;
                _hasRestoreRect = true;
            }

            // a floating window is required for SetWindowPos to place the strip
            Screen.fullScreenMode = FullScreenMode.Windowed;

            Rect2D work = WorkArea();
            int height = work.bottom - work.top;
            int left = work.right - width;
            SetWindowPos(window, HwndTopmost, left, work.top, width, height, SwpNoActivate | SwpShowWindow);
            IsDocked = true;
        }

        public static void Undock()
        {
            if (!IsSupported) return;
            IntPtr window = Window();
            if (window == IntPtr.Zero) return;

            if (_hasRestoreRect)
            {
                int width = _restoreRect.right - _restoreRect.left;
                int height = _restoreRect.bottom - _restoreRect.top;
                SetWindowPos(window, HwndNoTopmost, _restoreRect.left, _restoreRect.top, width, height,
                    SwpNoActivate | SwpShowWindow);
            }
            else
            {
                SetWindowPos(window, HwndNoTopmost, 0, 0, 0, 0, SwpNoActivate | SwpNoMove | SwpNoSize);
            }
            IsDocked = false;
        }

        // captured once while the window is active (the click that docks it), then reused
        private static IntPtr Window()
        {
            if (_window == IntPtr.Zero) _window = GetActiveWindow();
            return _window;
        }

        private static Rect2D WorkArea()
        {
            var rect = new Rect2D();
            if (SystemParametersInfo(SpiGetWorkArea, 0, ref rect, 0)) return rect;

            rect.left = 0;
            rect.top = 0;
            rect.right = GetSystemMetrics(SmCxScreen);
            rect.bottom = GetSystemMetrics(SmCyScreen);
            return rect;
        }

        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpShowWindow = 0x0040;
        private const uint SpiGetWorkArea = 0x0030;
        private const int SmCxScreen = 0;
        private const int SmCyScreen = 1;
        private static readonly IntPtr HwndTopmost = new IntPtr(-1);
        private static readonly IntPtr HwndNoTopmost = new IntPtr(-2);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect2D
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect2D rect);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint action, uint param, ref Rect2D value, uint winIni);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);
    }
}

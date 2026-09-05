using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ToDonegeon.View
{
    // Docks the standalone window to a screen edge as an always-on-top strip, so the task list can sit
    // over other applications while they stay interactive. Undocking returns the game to fullscreen.
    // Unity owns the window size (Screen.SetResolution); the native calls set position and topmost, and
    // are re-applied for a few frames because a resolution change only lands at the end of the frame.
    // Windows standalone only; a no-op in the editor and on other platforms.
    public static class WindowDock
    {
        public static bool IsSupported => Application.platform == RuntimePlatform.WindowsPlayer;
        public static bool IsDocked { get; private set; }

        public const int DefaultDockWidth = 340;

        private static IntPtr _window;

        private static int _pendingFrames;
        private static int _targetX;
        private static int _targetY;
        private static int _targetWidth;
        private static int _targetHeight;
        private static IntPtr _targetOrder;
        private static uint _targetFlags;
        private const int ReassertFrames = 10;

        public static void Toggle(int width = DefaultDockWidth)
        {
            if (IsDocked) Undock();
            else Dock(width);
        }

        public static void Dock(int width = DefaultDockWidth)
        {
            if (!IsSupported || Window() == IntPtr.Zero) return;

            Rect2D work = WorkArea();
            int height = work.bottom - work.top;
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            SchedulePlacement(work.right - width, work.top, width, height, HwndTopmost, SwpNoActivate | SwpShowWindow);
            IsDocked = true;
        }

        public static void Undock()
        {
            if (!IsSupported) return;

            IsDocked = false;
            GoFullscreen();
            SchedulePlacement(0, 0, 0, 0, HwndNoTopmost, SwpNoMove | SwpNoSize | SwpNoActivate);
        }

        // Called at startup to restore fullscreen when we are not docked but stuck at the dock width.
        public static void NormalizeSize()
        {
            if (!IsSupported || IsDocked) return;
            if (Screen.width <= DefaultDockWidth) GoFullscreen();
        }

        private static void GoFullscreen()
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }

        private static void SchedulePlacement(int x, int y, int width, int height, IntPtr order, uint flags)
        {
            _targetX = x;
            _targetY = y;
            _targetWidth = width;
            _targetHeight = height;
            _targetOrder = order;
            _targetFlags = flags;
            _pendingFrames = ReassertFrames;
            DockRunner.Ensure();
        }

        // re-applies position, size, and topmost while the resolution change settles over the next frames
        internal static void ApplyPendingPlacement()
        {
            if (_pendingFrames <= 0) return;
            _pendingFrames--;

            IntPtr window = Window();
            if (window != IntPtr.Zero)
                SetWindowPos(window, _targetOrder, _targetX, _targetY, _targetWidth, _targetHeight, _targetFlags);
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
        private static extern bool SystemParametersInfo(uint action, uint param, ref Rect2D value, uint winIni);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);
    }
}

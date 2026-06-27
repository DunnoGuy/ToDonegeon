using System.Collections.Generic;
using UnityEngine;

namespace ToDonegeon.View
{
    // tracks the IMGUI panel rects drawn this frame so a world click landing on a panel can be ignored
    internal static class ScreenGuiRegistry
    {
        private static readonly List<Rect> Rects = new List<Rect>();
        private static int _frame = -1;

        public static void Register(Rect rect)
        {
            if (Time.frameCount != _frame)
            {
                Rects.Clear();
                _frame = Time.frameCount;
            }
            Rects.Add(rect);
        }
        
        public static bool IsPointerOverPanel(Vector2 pointer)
        {
            var guiPoint = new Vector2(pointer.x, Screen.height - pointer.y);
            for (int i = 0; i < Rects.Count; i++)
                if (Rects[i].Contains(guiPoint)) return true;
            return false;
        }
    }
}

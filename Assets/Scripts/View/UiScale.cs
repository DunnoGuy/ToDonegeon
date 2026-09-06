using UnityEngine;

namespace ToDonegeon.View
{
    // Uniformly enlarges all IMGUI
    public static class UiScale
    {
        public const float Factor = 1.5f;

        public static float VirtualWidth => Screen.width / Factor;
        public static float VirtualHeight => Screen.height / Factor;

        public static void Apply() => GUI.matrix = Matrix4x4.Scale(new Vector3(Factor, Factor, 1f));

        public static Vector2 PointerToGui(Vector2 screenPointer)
            => new Vector2(screenPointer.x / Factor, (Screen.height - screenPointer.y) / Factor);

        public static Vector2 ScreenToGui(Vector3 screenPoint)
            => new Vector2(screenPoint.x / Factor, (Screen.height - screenPoint.y) / Factor);
    }
}

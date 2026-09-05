using UnityEngine;

namespace ToDonegeon.View
{
    // Re-applies WindowDock's window placement for a few frames after a resolution change
    public sealed class DockRunner : MonoBehaviour
    {
        private static DockRunner _instance;

        public static void Ensure()
        {
            if (_instance != null) return;

            var host = new GameObject("DockRunner");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<DockRunner>();
        }

        private void LateUpdate() => WindowDock.ApplyPendingPlacement();
    }
}

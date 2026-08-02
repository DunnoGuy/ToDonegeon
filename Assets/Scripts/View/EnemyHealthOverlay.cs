using System.Collections.Generic;
using UnityEngine;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // a small health bar and current/max HP over every live enemy
    public sealed class EnemyHealthOverlay : MonoBehaviour
    {
        [SerializeField] private EntitySpawner _spawner;
        [SerializeField] private Camera _camera;
        [Tooltip("World units above the enemy to float the bar")]
        [SerializeField] private float _heightOffset = 0.6f;

        private const float BarWidth = 42f;
        private const float BarHeight = 5f;
        private const float LabelHeight = 15f;

        private static readonly Color BackColor = new Color(0f, 0f, 0f, 0.6f);

        private GUIStyle _labelStyle;

        private void Awake()
        {
            if (_spawner == null) _spawner = FindAnyObjectByType<EntitySpawner>();
            if (_camera == null) _camera = Camera.main;
        }

        private void OnGUI()
        {
            if (_spawner == null) return;
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10
            };

            IReadOnlyList<Enemy> enemies = _spawner.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                DrawEnemyHealth(enemy);
            }
        }

        private void DrawEnemyHealth(Enemy enemy)
        {
            Vector3 screen = _camera.WorldToScreenPoint(enemy.transform.position + Vector3.up * _heightOffset);
            if (screen.z <= 0f) return; // behind the camera
            if (screen.x < -BarWidth || screen.x > Screen.width + BarWidth) return;

            Health health = enemy.Character.Health;
            float fill = health.Max > 0 ? Mathf.Clamp01((float)health.Current / health.Max) : 0f;

            float left = screen.x - BarWidth * 0.5f;
            float top = Screen.height - screen.y - BarHeight;

            Color previous = GUI.color;
            GUI.color = BackColor;
            GUI.DrawTexture(new Rect(left, top, BarWidth, BarHeight), Texture2D.whiteTexture);
            GUI.color = Color.Lerp(Color.red, Color.green, fill);
            GUI.DrawTexture(new Rect(left, top, BarWidth * fill, BarHeight), Texture2D.whiteTexture);
            GUI.color = previous;

            GUI.Label(new Rect(left, top - LabelHeight, BarWidth, LabelHeight),
                $"{health.Current}/{health.Max}", _labelStyle);
        }
    }
}

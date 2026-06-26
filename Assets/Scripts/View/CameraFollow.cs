using UnityEngine;

namespace ToDonegeon.View
{
    // Keeps the camera centred on a target (the player), snapping instantly on large
    // jumps such as a dungeon regeneration.
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        [Tooltip("Approximate seconds to catch up to the target.")]
        [SerializeField] private float _smoothTime = 0.15f;

        [Tooltip("Orthographic half-height (zoom). 0 leaves the camera's current size untouched.")]
        [SerializeField] private float _orthographicSize = 9f;

        [Tooltip("If the target is farther than this, snap instead of gliding (handles teleports).")]
        [SerializeField] private float _snapDistance = 12f;

        private Camera _camera;
        private Vector3 _velocity;

        public void SetTarget(Transform target) => _target = target;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            if (_orthographicSize > 0f) _camera.orthographicSize = _orthographicSize;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 here = transform.position;
            var desired = new Vector3(_target.position.x, _target.position.y, here.z);

            if ((desired - here).sqrMagnitude > _snapDistance * _snapDistance)
                transform.position = desired;
            else
                transform.position = Vector3.SmoothDamp(here, desired, ref _velocity, _smoothTime);
        }
    }
}

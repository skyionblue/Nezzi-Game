using UnityEngine;

namespace OneWayTogether.Camera
{
    /// <summary>
    /// Isometric camera — maintains a fixed world-space offset above the target.
    /// No rotation, no orbit, no lag. The target is always at the same screen position.
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 12f, -10f);
        [SerializeField, Range(0f, 20f)] private float _smoothSpeed = 0f; // 0 = instant

        private void LateUpdate()
        {
            if (_target == null) return;
            Vector3 desired = _target.position + _offset;
            transform.position = _smoothSpeed > 0f
                ? Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime)
                : desired;
        }
    }
}

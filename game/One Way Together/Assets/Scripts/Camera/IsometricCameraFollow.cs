using UnityEngine;
using OneWayTogether.Events;
using OneWayTogether.Characters;

namespace OneWayTogether.Camera
{
    /// <summary>
    /// Isometric camera — maintains a fixed world-space offset above the active character.
    /// Switches target when GameEvents.OnActiveCharacterChanged fires (Tab switching).
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform _scarlet;
        [SerializeField] private Transform _dani;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);
        [SerializeField, Range(0f, 20f)] private float _smoothSpeed = 8f;

        private Transform _target;

        private void OnEnable()
        {
            GameEvents.OnActiveCharacterChanged += OnCharacterChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnActiveCharacterChanged -= OnCharacterChanged;
        }

        private void Start()
        {
            // Default to Scarlet until the first event fires.
            _target = _scarlet;
        }

        private void OnCharacterChanged(CharacterType activeCharacter)
        {
            _target = activeCharacter == CharacterType.Scarlet ? _scarlet : _dani;
        }

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

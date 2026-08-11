using UnityEngine;
using Unity.Cinemachine;
using OneWayTogether.Events;

namespace OneWayTogether.Camera
{
    /// <summary>
    /// Manages two Cinemachine Virtual Cameras:
    ///
    /// - <see cref="_singlePlayerCamera"/>: follows the currently active character
    ///   in single-player mode. Its Follow/LookAt target is swapped when the active
    ///   character changes.
    ///
    /// - <see cref="_coopCamera"/>: a framing-transposer camera that keeps both
    ///   characters in frame at all times during co-op. Activated when a second
    ///   player joins.
    ///
    /// Priority is used to switch between cameras: the higher priority wins the
    /// Cinemachine Brain blend.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Virtual Cameras")]
        [Tooltip("Virtual camera used during single-player — follows active character.")]
        [SerializeField] private CinemachineCamera _singlePlayerCamera;

        [Tooltip("Virtual camera used during co-op — frames both characters.")]
        [SerializeField] private CinemachineCamera _coopCamera;

        [Header("Character Transforms")]
        [Tooltip("Scarlet's transform — assign from scene.")]
        [SerializeField] private Transform _scarletTransform;

        [Tooltip("Dani's transform — assign from scene.")]
        [SerializeField] private Transform _daniTransform;

        [Header("Co-op Group Target")]
        [Tooltip("CinemachineTargetGroup that holds both characters for the co-op camera.")]
        [SerializeField] private CinemachineTargetGroup _targetGroup;

        // ── Priority constants ────────────────────────────────────────────────────

        private const int PriorityHigh = 20;
        private const int PriorityLow  = 10;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnActiveCharacterChanged += HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       += HandleCoopJoined;
            GameEvents.OnCoopPlayerLeft         += HandleCoopLeft;
        }

        private void OnDisable()
        {
            GameEvents.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       -= HandleCoopJoined;
            GameEvents.OnCoopPlayerLeft         -= HandleCoopLeft;
        }

        private void Start()
        {
            SetupTargetGroup();
            ActivateSinglePlayerCamera(CharacterType.Scarlet);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleActiveCharacterChanged(CharacterType activeType)
        {
            // Only applies in single-player; co-op camera is active when in co-op.
            if (_coopCamera.Priority > _singlePlayerCamera.Priority) return;
            FollowCharacter(activeType);
        }

        private void HandleCoopJoined(CharacterType _)
        {
            // Elevate co-op camera so Cinemachine Brain blends to it.
            _coopCamera.Priority   = PriorityHigh;
            _singlePlayerCamera.Priority = PriorityLow;
        }

        private void HandleCoopLeft()
        {
            _singlePlayerCamera.Priority = PriorityHigh;
            _coopCamera.Priority   = PriorityLow;
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void SetupTargetGroup()
        {
            if (_targetGroup == null || _scarletTransform == null || _daniTransform == null) return;

            _targetGroup.Targets.Clear();
            _targetGroup.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = _scarletTransform,
                Weight = 1f,
                Radius = 1f
            });
            _targetGroup.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = _daniTransform,
                Weight = 1f,
                Radius = 1f
            });
        }

        private void ActivateSinglePlayerCamera(CharacterType followType)
        {
            _singlePlayerCamera.Priority = PriorityHigh;
            _coopCamera.Priority   = PriorityLow;
            FollowCharacter(followType);
        }

        private void FollowCharacter(CharacterType type)
        {
            Transform target = type == CharacterType.Scarlet ? _scarletTransform : _daniTransform;
            if (target == null) return;

            _singlePlayerCamera.Follow   = target;
            _singlePlayerCamera.LookAt   = target;
        }
    }
}

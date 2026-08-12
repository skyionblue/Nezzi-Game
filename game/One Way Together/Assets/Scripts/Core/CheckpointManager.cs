using UnityEngine;
using System.Collections;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Tracks the active checkpoint position for both characters and executes
    /// the reset sequence when a failure occurs. Knows nothing about coins —
    /// <see cref="CoinManager"/> calls <see cref="RespawnInPlace"/> when the
    /// player elects to spend coins instead of resetting.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Seconds of freeze-frame before the reset animation begins.")]
        [SerializeField, Range(0f, 3f)] private float _failureDisplayDuration = 1.5f;

        [Tooltip("Seconds the fade/transition takes before characters reappear at checkpoint.")]
        [SerializeField, Range(0f, 2f)] private float _resetTransitionDuration = 0.75f;

        // Checkpoint positions, set by CheckpointTrigger when characters pass through.
        private Vector3 _scarletCheckpoint;
        private Vector3 _daniCheckpoint;

        // Character transform references, assigned by the characters registering themselves.
        private Transform _scarletTransform;
        private Transform _daniTransform;

        private bool _isResetting;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnCharacterFailed += HandleCharacterFailed;
            GameEvents.OnCheckpointActivated += HandleCheckpointActivated;
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterFailed -= HandleCharacterFailed;
            GameEvents.OnCheckpointActivated -= HandleCheckpointActivated;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Characters call this in their Awake so the manager has transform references
        /// for the reset teleport without using FindObjectOfType.
        /// </summary>
        public void RegisterCharacter(CharacterType type, Transform characterTransform, Vector3 startPosition)
        {
            if (type == CharacterType.Scarlet)
            {
                _scarletTransform = characterTransform;
                _scarletCheckpoint = startPosition;
            }
            else
            {
                _daniTransform = characterTransform;
                _daniCheckpoint = startPosition;
            }
        }

        /// <summary>
        /// Teleports both characters back to the last checkpoint. Called by CheckpointManager
        /// itself after failure, or by external UI when no coin respawn is chosen.
        /// </summary>
        public void ResetToCheckpoint()
        {
            if (_isResetting) return;
            StartCoroutine(ResetSequence());
        }

        /// <summary>
        /// Skips checkpoint reset — characters stay exactly where they are.
        /// Called by <see cref="CoinManager"/> after a successful coin-respawn purchase.
        /// </summary>
        public void RespawnInPlace()
        {
            if (_isResetting) return;
            GameManager.Instance.SetState(GameState.Playing);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleCharacterFailed(CharacterType _)
        {
            // Begin reset sequence only if we're not already mid-reset.
            if (!_isResetting)
                StartCoroutine(WaitThenReset());
        }

        private void HandleCheckpointActivated(Vector3 position)
        {
            // Store independent positions for each character.
            // CheckpointTrigger provides the authoritative position; each character's
            // actual position offset is calculated from the trigger's stored offsets.
            // For now, store the same position — CheckpointTrigger sets per-character
            // positions via RegisterCheckpoint.
            _scarletCheckpoint = position;
            _daniCheckpoint = position;
        }

        /// <summary>
        /// Called by CheckpointTrigger to store per-character checkpoint positions.
        /// This allows characters starting at different offsets within the same checkpoint zone.
        /// </summary>
        public void RegisterCheckpoint(CharacterType type, Vector3 position)
        {
            if (type == CharacterType.Scarlet)
                _scarletCheckpoint = position;
            else
                _daniCheckpoint = position;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private IEnumerator WaitThenReset()
        {
            yield return new WaitForSeconds(_failureDisplayDuration);
            // CoinManager gets a window here to intercept via the UI. If it doesn't,
            // we proceed with automatic checkpoint reset.
            // The UI layer sets GameState back to Playing (via RespawnInPlace) or
            // calls ResetToCheckpoint — so we wait for the GameManager state to resolve.
        }

        private IEnumerator ResetSequence()
        {
            _isResetting = true;

            // Trigger a screen fade here when a transition system is added.
            yield return new WaitForSeconds(_resetTransitionDuration);

            TeleportToCheckpoints();
            GameEvents.RaiseCheckpointReset();
            GameManager.Instance.SetState(GameState.Playing);

            _isResetting = false;
        }

        private void TeleportToCheckpoints()
        {
            Teleport(_scarletTransform, _scarletCheckpoint);
            Teleport(_daniTransform, _daniCheckpoint);
        }

        private void Teleport(Transform t, Vector3 position)
        {
            if (t == null) return;

            // CharacterController fights direct position changes while enabled.
            // Disable → set position → re-enable to guarantee the teleport sticks.
            CharacterController cc = t.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            t.position = position;
            if (cc != null) cc.enabled = true;

            // Clear accumulated fall velocity so the character doesn't immediately
            // re-fall after being placed on solid ground.
            CharacterBase cb = t.GetComponent<CharacterBase>();
            if (cb != null) cb.ResetVelocity();
        }
    }
}

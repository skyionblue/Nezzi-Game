using UnityEngine;
using OneWayTogether.Core;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Placed in scenes to mark checkpoint locations. When either character
    /// enters the trigger zone for the first time, the checkpoint is recorded
    /// in <see cref="CheckpointManager"/> with per-character spawn positions.
    ///
    /// A checkpoint is one-shot: it cannot be deactivated once triggered.
    /// Multiple checkpoints in a level are ordered by their scene position
    /// — the most recently entered one is always the active checkpoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [Header("Spawn Offsets")]
        [Tooltip("World-space offset from this trigger's position where Scarlet will respawn.")]
        [SerializeField] private Vector2 _scarletSpawnOffset = new Vector2(-0.5f, 0f);

        [Tooltip("World-space offset from this trigger's position where Dani will respawn.")]
        [SerializeField] private Vector2 _daniSpawnOffset = new Vector2(0.5f, 0f);

        [Header("Feedback")]
        [SerializeField] private GameObject _activatedFX;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _hasBeenActivated;
        private CheckpointManager _checkpointManager;

        private static int _characterLayer = -1;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            GetComponent<Collider2D>().isTrigger = true;
            _checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasBeenActivated) return;
            if (other.gameObject.layer != _characterLayer) return;

            Activate();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void Activate()
        {
            _hasBeenActivated = true;

            Vector3 origin = transform.position;
            Vector3 scarletPos = origin + (Vector3)_scarletSpawnOffset;
            Vector3 daniPos    = origin + (Vector3)_daniSpawnOffset;

            _checkpointManager?.RegisterCheckpoint(CharacterType.Scarlet, scarletPos);
            _checkpointManager?.RegisterCheckpoint(CharacterType.Dani, daniPos);

            GameEvents.RaiseCheckpointActivated(origin);

            if (_activatedFX != null)
                Instantiate(_activatedFX, origin, Quaternion.identity);

            Debug.Log($"[CheckpointTrigger] Checkpoint '{name}' activated at {origin}.");
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;

            // Scarlet spawn (orange)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawSphere(origin + (Vector3)_scarletSpawnOffset, 0.2f);

            // Dani spawn (cyan)
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawSphere(origin + (Vector3)_daniSpawnOffset, 0.15f);
        }
    }
}

using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// The win condition for every level. The level completes only when BOTH
    /// Scarlet and Dani are simultaneously inside this trigger zone.
    ///
    /// A single character entering does not trigger completion — they must
    /// physically be together. This mechanically reinforces the game's emotional
    /// core: reuniting is the goal, not reaching an exit.
    ///
    /// Place one per level. The trigger should be clearly signposted in the scene
    /// (a warm glow, particle effect, or distinct tile) so players know the goal.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ReunionTrigger : MonoBehaviour
    {
        [Header("Feedback")]
        [Tooltip("Particle or animation object played when both characters reunite.")]
        [SerializeField] private GameObject _reunionFX;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _scarletInside;
        private bool _daniInside;
        private bool _hasTriggered;

        private static int _characterLayer = -1;

        // Cache the tags to avoid per-frame string comparison.
        private const string ScarletTag = "Scarlet";
        private const string DaniTag    = "Dani";

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasTriggered) return;
            if (other.gameObject.layer != _characterLayer) return;

            if (other.CompareTag(ScarletTag)) _scarletInside = true;
            else if (other.CompareTag(DaniTag)) _daniInside = true;

            CheckReunion();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_hasTriggered) return;
            if (other.gameObject.layer != _characterLayer) return;

            if (other.CompareTag(ScarletTag)) _scarletInside = false;
            else if (other.CompareTag(DaniTag)) _daniInside = false;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void CheckReunion()
        {
            if (!_scarletInside || !_daniInside) return;

            _hasTriggered = true;

            if (_reunionFX != null)
                Instantiate(_reunionFX, transform.position, Quaternion.identity);

            GameEvents.RaiseReunionAchieved();
            Debug.Log("[ReunionTrigger] Both characters reunited — puzzle complete!");
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one);
        }
    }
}

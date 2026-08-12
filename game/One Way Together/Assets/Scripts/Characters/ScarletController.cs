using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Scarlet-specific abilities for HD-2D isometric movement: lift Dani to
    /// high ledges. Boulder-push is handled implicitly by the CharacterController
    /// walking into a physics Rigidbody — no special code needed.
    ///
    /// Pressure plate: handled entirely by the PressurePlate scene component —
    /// Scarlet does nothing special, just stands on it.
    /// </summary>
    public class ScarletController : CharacterBase
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Lift Settings")]
        [Tooltip("Local-space offset where Dani sits when being held.")]
        [SerializeField] private Vector3 _liftOffset = new Vector3(0f, 1.2f, 0f);

        [Tooltip("Radius around Scarlet's center checked for Dani's presence when Interact is pressed.")]
        [SerializeField, Range(0.1f, 2f)] private float _liftRange = 1f;

        [Tooltip("Layer on which Dani's collider lives — used by the overlap check.")]
        [SerializeField] private LayerMask _daniLayer;

        [Tooltip("Upward impulse added to Dani when she jumps off Scarlet's hands.")]
        [SerializeField, Range(1f, 20f)] private float _liftLaunchForce = 8f;

        // ── State ─────────────────────────────────────────────────────────────────

        private DaniController _heldDani;
        private bool _isHoldingDani;

        // ── CharacterBase overrides ───────────────────────────────────────────────

        /// <inheritdoc/>
        public override CharacterType CharacterType => CharacterType.Scarlet;

        // ── Input API ────────────────────────────────────────────────────────────

        public override void ReceiveInteract()
        {
            if (!IsControllable) return;

            if (_isHoldingDani)
                ReleaseDani();
            else
                TryLiftDani();
        }

        // ── Lift mechanics ────────────────────────────────────────────────────────

        /// <summary>
        /// Searches for Dani within lift range and, if found, parents her to
        /// Scarlet's hands so she rides along during movement.
        /// </summary>
        private void TryLiftDani()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _liftRange, _daniLayer);
            if (hits.Length == 0) return;

            DaniController dani = hits[0].GetComponentInParent<DaniController>();
            if (dani == null) return;

            _heldDani = dani;
            _isHoldingDani = true;

            dani.BeginLiftedState(transform, _liftOffset, _liftLaunchForce);
        }

        /// <summary>
        /// Releases Dani from the hold — she falls with gravity.
        /// Called when Interact fires a second time, or when Dani herself
        /// triggers a jump to reach a ledge.
        /// </summary>
        public void ReleaseDani()
        {
            if (!_isHoldingDani || _heldDani == null) return;

            _heldDani.EndLiftedState();
            _heldDani = null;
            _isHoldingDani = false;
        }

        // ── Gizmos ───────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _liftRange);
        }
    }
}

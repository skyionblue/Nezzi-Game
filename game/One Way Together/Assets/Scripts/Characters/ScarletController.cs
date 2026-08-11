using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Scarlet-specific abilities: push/roll boulders and stone blocks, stand on
    /// pressure plates, and lift Dani up to high ledges.
    ///
    /// Push: detected via Rigidbody2D on the boulder. Scarlet walks into it and
    /// the physics layer difference allows her to move it.
    ///
    /// Lift: when Dani is within the lift trigger zone and the Interact action fires,
    /// Dani's transform is parented to Scarlet and positioned at the lift offset until
    /// Dani jumps (at which point Dani is un-parented and given a launch impulse).
    ///
    /// Pressure plate: handled entirely by the PressurePlate scene component — Scarlet
    /// does nothing special, just stands on it.
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

        // ── Input System callbacks ────────────────────────────────────────────────

        /// <summary>
        /// Receives the Interact action. In single-player, this is always wired.
        /// In co-op Scarlet's PlayerInput fires this on Player 1's device.
        /// </summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!IsControllable) return;
            if (!context.performed) return;

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
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _liftRange, _daniLayer);
            if (hit == null) return;

            DaniController dani = hit.GetComponentInParent<DaniController>();
            if (dani == null) return;

            _heldDani = dani;
            _isHoldingDani = true;

            dani.BeginLiftedState(transform, _liftOffset, _liftLaunchForce);
        }

        /// <summary>
        /// Releases Dani from the hold — she drops with normal gravity.
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

using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Scarlet-specific abilities for HD-2D isometric movement: push boulders
    /// via OnControllerColliderHit, lift Dani to high ledges.
    ///
    /// Pressure plate: handled entirely by the PressurePlate scene component —
    /// Scarlet does nothing special, just stands on it.
    /// </summary>
    public class ScarletController : CharacterBase
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Boulder Push")]
        [Tooltip("Force applied to Rigidbodies Scarlet walks into (ForceMode.Force, per-frame).")]
        [SerializeField, Range(1f, 50f)] private float _pushForce = 15f;

        [Header("Lift Settings")]
        [Tooltip("Local-space offset where Dani sits when being held. Y = height (shoulders), Z = small forward lean.")]
        [SerializeField] private Vector3 _liftOffset = new Vector3(0f, 0.9f, 0.0f);

        [Tooltip("Radius around Scarlet's center checked for Dani's presence when Interact is pressed.")]
        [SerializeField, Range(0.1f, 3f)] private float _liftRange = 1.5f;

        [Tooltip("Layer on which Dani's collider lives — used by the overlap check.")]
        [SerializeField] private LayerMask _daniLayer;

        [Tooltip("Upward impulse added to Dani when she jumps off Scarlet's hands.")]
        [SerializeField, Range(1f, 20f)] private float _liftLaunchForce = 8f;

        // ── State ─────────────────────────────────────────────────────────────────

        private DaniController _heldDani;
        private bool _isHoldingDani;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            // Fall back to the Character layer if the mask was never set in the Inspector.
            if (_daniLayer.value == 0)
                _daniLayer = LayerMask.GetMask("Character");
        }

        // ── CharacterBase overrides ───────────────────────────────────────────────

        /// <inheritdoc/>
        public override CharacterType CharacterType => CharacterType.Scarlet;

        // ── Input API ────────────────────────────────────────────────────────────

        public override void ReceiveInteract()
        {
            if (!IsControllable) return;

            if (_isHoldingDani)
            {
                // While carrying Dani, let her try to press a nearby switch
                // from her elevated position before deciding to release her.
                if (_heldDani != null && _heldDani.TryActivateSwitchWhileLifted())
                    return; // Dani pressed a switch — stay in carry mode

                ReleaseDani(); // Nothing in range — release as normal
            }
            else
            {
                TryLiftDani();
            }
        }

        // ── Boulder push ─────────────────────────────────────────────────────────

        protected override void OnControllerColliderHit(ControllerColliderHit hit)
        {
            base.OnControllerColliderHit(hit);

            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;
            if (hit.moveDirection.y < -0.3f) return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            rb.AddForce(pushDir * _pushForce, ForceMode.Force);
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

            DaniController dani = null;
            foreach (Collider hit in hits)
            {
                dani = hit.GetComponentInParent<DaniController>();
                if (dani != null) break;
            }
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

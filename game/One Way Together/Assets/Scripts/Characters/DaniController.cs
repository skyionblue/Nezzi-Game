using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Dani-specific abilities for HD-2D isometric movement:
    ///
    /// - <b>Activate switch</b>: Interact action when overlapping a switch trigger
    ///   fires the switch via a direct interface call on the Interactable.
    ///
    /// - <b>Stack objects</b>: Interact action while overlapping a stackable object
    ///   picks it up; it is then parented to Dani and follows her. Interact again
    ///   drops it.
    ///
    /// - <b>Lifted state</b>: Scarlet calls <see cref="BeginLiftedState"/> to parent
    ///   Dani and <see cref="EndLiftedState"/> to release her.
    ///
    /// Note: Climb is deferred — ropes will be redesigned for the 3D isometric space.
    /// </summary>
    public class DaniController : CharacterBase
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Stack")]
        [Tooltip("Local-space offset above Dani's feet where carried objects are held.")]
        [SerializeField] private Vector3 _carryOffset = new Vector3(0f, 0.8f, 0f);

        [Tooltip("Radius around Dani checked for interactable objects (switches, stackables).")]
        [SerializeField, Range(0.1f, 2f)] private float _interactRange = 0.8f;

        [Tooltip("Layer mask for stackable objects.")]
        [SerializeField] private LayerMask _stackableLayer;

        [Tooltip("Layer mask for switches and levers.")]
        [SerializeField] private LayerMask _switchLayer;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isLifted;

        // Currently carried stackable object.
        private GameObject _carriedObject;
        private bool _isCarrying;

        private float _liftLaunchForce;
        private Transform _scarletTransform;

        // Animator hashes
        private static readonly int AnimCarry = Animator.StringToHash("Carry");

        // ── Properties ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override CharacterType CharacterType => CharacterType.Dani;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            if (_isLifted) return; // Scarlet drives position while holding Dani.
            base.Update();
        }

        // ── Input API ────────────────────────────────────────────────────────────

        public override void ReceiveMove(Vector2 move)
        {
            base.ReceiveMove(move);
        }

        public override void ReceiveInteract()
        {
            if (!IsControllable) return;

            if (_isCarrying)
            {
                DropObject();
                return;
            }

            if (!TryActivateSwitch())
                TryPickUpObject();
        }

        // ── Lifted state (called by Scarlet) ──────────────────────────────────────

        /// <summary>
        /// Puts Dani into a "held" state — disables her CharacterController and
        /// parents her to Scarlet's hand position.
        /// </summary>
        public void BeginLiftedState(Transform scarletTransform, Vector3 liftOffset, float launchForce)
        {
            _isLifted = true;
            _liftLaunchForce = launchForce;
            _scarletTransform = scarletTransform;

            _cc.enabled = false;

            transform.SetParent(scarletTransform);
            transform.localPosition = liftOffset;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Releases Dani from Scarlet's hold. Nudges her forward in Scarlet's facing
        /// direction and applies an upward impulse so she jumps off and lands on the ledge.
        /// </summary>
        public void EndLiftedState()
        {
            _isLifted = false;

            // Push Dani half a unit forward in Scarlet's facing direction so she clears
            // the ledge wall and lands on the surface above.
            if (_scarletTransform != null)
                transform.position += _scarletTransform.forward * 0.5f;
            _scarletTransform = null;

            transform.SetParent(null);
            _cc.enabled = true;

            // Upward impulse — Dani visibly jumps off Scarlet's shoulders.
            SetVerticalVelocity(_liftLaunchForce);
        }

        // ── Stack objects ─────────────────────────────────────────────────────────

        private bool TryPickUpObject()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _interactRange, _stackableLayer);
            if (hits.Length == 0) return false;

            _carriedObject = hits[0].gameObject;
            _isCarrying = true;

            // Disable the object's physics while carried.
            Rigidbody objectRb = _carriedObject.GetComponent<Rigidbody>();
            if (objectRb != null)
                objectRb.isKinematic = true;

            _carriedObject.transform.SetParent(transform);
            _carriedObject.transform.localPosition = _carryOffset;

            _animator.SetBool(AnimCarry, true);
            return true;
        }

        private void DropObject()
        {
            if (_carriedObject == null) return;

            _carriedObject.transform.SetParent(null);

            Rigidbody objectRb = _carriedObject.GetComponent<Rigidbody>();
            if (objectRb != null)
                objectRb.isKinematic = false;

            _carriedObject = null;
            _isCarrying = false;
            _animator.SetBool(AnimCarry, false);
        }

        // ── Switch activation ─────────────────────────────────────────────────────

        private bool TryActivateSwitch()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _interactRange, _switchLayer);
            if (hits.Length == 0) return false;

            IInteractable interactable = hits[0].GetComponentInParent<IInteractable>();
            interactable?.Interact(this);
            return true;
        }

        // ── Climb (deferred — kept as stub for RopeTrigger compatibility) ──────────

        /// <summary>
        /// Called by RopeTrigger when Dani enters or exits a climbable zone.
        /// Climb mechanics will be redesigned for the 3D isometric space.
        /// </summary>
        public void SetClimbingState(bool canClimb)
        {
            // Stub — climb is not yet implemented for the HD-2D isometric build.
            // RopeTrigger still calls this so the interface remains stable.
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
    }
}

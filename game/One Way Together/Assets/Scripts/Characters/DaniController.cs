using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Dani-specific abilities:
    ///
    /// - <b>Crawl</b>: reduces the collider height when crawl input is held while grounded.
    ///   Narrow tunnel triggers can check <see cref="IsCrawling"/> to allow or block passage.
    ///
    /// - <b>Climb</b>: when overlapping a rope/vine trigger, vertical input drives
    ///   Dani up or down at <see cref="_climbSpeed"/> and gravity is zeroed.
    ///
    /// - <b>Activate switch</b>: Interact action when overlapping a switch trigger
    ///   fires the switch via <see cref="GameEvents.RaisePressurePlateChanged"/> or
    ///   a direct interface call on the Interactable.
    ///
    /// - <b>Stack objects</b>: Interact action while overlapping a stackable object
    ///   picks it up; it is then parented to Dani and follows her. Interact again
    ///   on a valid stack zone drops it.
    ///
    /// - <b>Lifted state</b>: Scarlet calls <see cref="BeginLiftedState"/> to parent
    ///   Dani and <see cref="EndLiftedState"/> to release her.
    /// </summary>
    public class DaniController : CharacterBase
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Crawl")]
        [Tooltip("Fraction of full standing height used when crawling.")]
        [SerializeField, Range(0.2f, 0.9f)] private float _crawlHeightFraction = 0.5f;

        [Header("Climb")]
        [Tooltip("Vertical speed while climbing a rope or vine.")]
        [SerializeField, Range(1f, 10f)] private float _climbSpeed = 4f;

        [Header("Stack")]
        [Tooltip("Local-space offset above Dani's feet where carried objects are held.")]
        [SerializeField] private Vector3 _carryOffset = new Vector3(0f, 0.8f, 0f);

        [Tooltip("Radius around Dani checked for interactable objects (switches, stackables).")]
        [SerializeField, Range(0.1f, 2f)] private float _interactRange = 0.8f;

        [Tooltip("Layer mask for stackable objects.")]
        [SerializeField] private LayerMask _stackableLayer;

        [Tooltip("Layer mask for switches and levers.")]
        [SerializeField] private LayerMask _switchLayer;

        // ── Cached ────────────────────────────────────────────────────────────────

        private CapsuleCollider2D _capsuleCollider;
        private Vector2 _standingColliderSize;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isCrawling;
        private bool _isClimbing;
        private bool _isLifted;

        // Currently carried stackable object.
        private GameObject _carriedObject;
        private bool _isCarrying;

        // Climbing: reference to current rope trigger
        private Rigidbody2D _climbAnchor;
        private float _liftLaunchForce;


        // Animator hashes
        private static readonly int AnimCrawl  = Animator.StringToHash("Crawl");
        private static readonly int AnimClimb  = Animator.StringToHash("Climb");
        private static readonly int AnimCarry  = Animator.StringToHash("Carry");

        // ── Properties ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override CharacterType CharacterType => CharacterType.Dani;

        /// <summary>True when Dani is in a crouched crawl state.</summary>
        public bool IsCrawling => _isCrawling;

        /// <summary>True when Dani is currently climbing a rope or vine.</summary>
        public bool IsClimbing => _isClimbing;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            if (_capsuleCollider != null)
                _standingColliderSize = _capsuleCollider.size;
        }

        protected override void FixedUpdate()
        {
            if (_isLifted) return; // Scarlet drives position while holding Dani.

            if (_isClimbing)
            {
                HandleClimbPhysics();
                return;
            }

            base.FixedUpdate();
            UpdateCrawlCollider();
        }

        // ── Input System callbacks ────────────────────────────────────────────────

        public override void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
            base.OnMove(context);

            // Toggle crawl based on downward input while grounded.
            if (IsGrounded && !_isClimbing)
                SetCrawl(_moveInput.y < -0.5f);
        }

        /// <summary>Interact action — picks up objects, activates switches.</summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!IsControllable || !context.performed) return;

            if (_isCarrying)
            {
                DropObject();
                return;
            }

            // Priority: switches first, then stackables.
            if (!TryActivateSwitch())
                TryPickUpObject();
        }

        // ── Lifted state (called by Scarlet) ──────────────────────────────────────

        /// <summary>
        /// Puts Dani into a "held" state — disables her collider and physics,
        /// parents her to Scarlet's hand position.
        /// </summary>
        public void BeginLiftedState(Transform scarletTransform, Vector3 liftOffset, float launchForce)
        {
            _isLifted = true;
            _liftLaunchForce = launchForce;

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _col.enabled = false;

            transform.SetParent(scarletTransform);
            transform.localPosition = liftOffset;
        }

        /// <summary>
        /// Releases Dani from Scarlet's hold, restoring physics.
        /// If Dani is jumping at release, a launch impulse is applied.
        /// </summary>
        public void EndLiftedState()
        {
            _isLifted = false;

            transform.SetParent(null);
            _col.enabled = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;

            // Apply launch impulse upward so Dani reaches the ledge.
            _rb.AddForce(Vector2.up * _liftLaunchForce, ForceMode2D.Impulse);
        }

        // ── Climb ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by a RopeTrigger (Trigger2D) when Dani enters / exits a climbable zone.
        /// </summary>
        public void SetClimbingState(bool canClimb)
        {
            if (canClimb && !_isClimbing)
                StartClimbing();
            else if (!canClimb && _isClimbing)
                StopClimbing();
        }

        private void StartClimbing()
        {
            _isClimbing = true;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool(AnimClimb, true);
        }

        private void StopClimbing()
        {
            _isClimbing = false;
            _rb.gravityScale = 1f;
            _animator.SetBool(AnimClimb, false);
        }

        private void HandleClimbPhysics()
        {
            if (!IsControllable) return;
            float verticalInput = _moveInput.y;
            _rb.linearVelocity = new Vector2(0f, verticalInput * _climbSpeed);
        }

        // ── Crawl ─────────────────────────────────────────────────────────────────

        private void SetCrawl(bool crawling)
        {
            if (_isCrawling == crawling) return;
            _isCrawling = crawling;
            _animator.SetBool(AnimCrawl, crawling);
        }

        private void UpdateCrawlCollider()
        {
            if (_capsuleCollider == null) return;

            float targetHeight = _isCrawling
                ? _standingColliderSize.y * _crawlHeightFraction
                : _standingColliderSize.y;

            _capsuleCollider.size = new Vector2(_standingColliderSize.x, targetHeight);
        }

        // ── Stack objects ─────────────────────────────────────────────────────────

        private bool TryPickUpObject()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _interactRange, _stackableLayer);
            if (hit == null) return false;

            _carriedObject = hit.gameObject;
            _isCarrying = true;

            // Disable the object's physics while carried.
            Rigidbody2D objectRb = _carriedObject.GetComponent<Rigidbody2D>();
            if (objectRb != null)
                objectRb.bodyType = RigidbodyType2D.Kinematic;

            _carriedObject.transform.SetParent(transform);
            _carriedObject.transform.localPosition = _carryOffset;

            _animator.SetBool(AnimCarry, true);
            return true;
        }

        private void DropObject()
        {
            if (_carriedObject == null) return;

            _carriedObject.transform.SetParent(null);

            Rigidbody2D objectRb = _carriedObject.GetComponent<Rigidbody2D>();
            if (objectRb != null)
                objectRb.bodyType = RigidbodyType2D.Dynamic;

            _carriedObject = null;
            _isCarrying = false;
            _animator.SetBool(AnimCarry, false);
        }

        // ── Switch activation ─────────────────────────────────────────────────────

        private bool TryActivateSwitch()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _interactRange, _switchLayer);
            if (hit == null) return false;

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            interactable?.Interact(this);
            return true;
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
    }
}

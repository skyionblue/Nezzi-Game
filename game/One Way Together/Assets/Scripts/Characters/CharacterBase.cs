using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Core;
using OneWayTogether.Data;
using OneWayTogether.Events;
using OneWayTogether.Input;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Shared foundation for Scarlet and Dani. Owns movement, jump, ground detection,
    /// and animation state. Subclasses add character-specific abilities.
    ///
    /// Attach <see cref="CharacterData"/> to drive all tunable values from a ScriptableObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class CharacterBase : MonoBehaviour
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Character Configuration")]
        [SerializeField] protected CharacterData _data;

        [Header("Ground Detection")]
        [Tooltip("Transform positioned at the character's feet for ground overlap check.")]
        [SerializeField] protected Transform _groundCheck;

        [Header("Dependencies")]
        [Tooltip("Reference to the scene's InputRouter. Assign in the Inspector.")]
        [SerializeField] private InputRouter _inputRouter;

        // ── Cached components ─────────────────────────────────────────────────────

        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected Collider2D _col;

        // ── State ─────────────────────────────────────────────────────────────────

        protected Vector2 _moveInput;
        private bool _jumpQueued;
        private bool _isGrounded;
        private bool _isFacingRight = true;

        // Animator parameter IDs — cached to avoid per-frame string hashing.
        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");
        private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimJump     = Animator.StringToHash("Jump");

        // ── Properties ────────────────────────────────────────────────────────────

        /// <summary>Identifies which sibling this is — set by the concrete subclass.</summary>
        public abstract CharacterType CharacterType { get; }

        /// <summary>True when the character is standing on ground this frame.</summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>Current world-space velocity from the Rigidbody.</summary>
        public Vector2 Velocity => _rb.linearVelocity;

        /// <summary>
        /// True when input is being forwarded to this character. False for the
        /// inactive character in single-player mode.
        /// </summary>
        public bool IsControllable { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        protected virtual void Awake()
        {
            _rb       = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _col      = GetComponent<Collider2D>();

            if (_data != null && _data.AnimatorController != null)
                _animator.runtimeAnimatorController = _data.AnimatorController;

            // Register with CheckpointManager so it can teleport us on reset.
            CheckpointManager cm = FindAnyObjectByType<CheckpointManager>();
            cm?.RegisterCharacter(CharacterType, transform, transform.position);
        }

        protected virtual void OnEnable()
        {
            GameEvents.OnActiveCharacterChanged += HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       += HandleCoopJoined;
            GameEvents.OnCoopPlayerLeft         += HandleCoopLeft;
            GameEvents.OnGameStateChanged       += HandleGameStateChanged;

            // Default to controllable; InputRouter will correct this if needed.
            IsControllable = true;
        }

        protected virtual void OnDisable()
        {
            GameEvents.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       -= HandleCoopJoined;
            GameEvents.OnCoopPlayerLeft         -= HandleCoopLeft;
            GameEvents.OnGameStateChanged       -= HandleGameStateChanged;
        }

        protected virtual void FixedUpdate()
        {
            CheckGrounded();
            ApplyMovement();
            ApplyBetterJumpGravity();
            DriveAnimator();
        }

        // ── Input System callbacks ────────────────────────────────────────────────
        // PlayerInput SendMessages mode calls these with InputValue parameter.

        /// <summary>Receives Move action from PlayerInput (SendMessages).</summary>
        public virtual void OnMove(InputValue value)
        {
            if (!IsControllable) return;
            _moveInput = value.Get<Vector2>();
        }

        /// <summary>Receives Jump action from PlayerInput (SendMessages).</summary>
        public virtual void OnJump(InputValue value)
        {
            if (!IsControllable) return;
            if (value.isPressed && _isGrounded)
                _jumpQueued = true;
        }

        /// <summary>
        /// Receives SwitchCharacter action from PlayerInput (SendMessages).
        /// Relays to InputRouter since SendMessages can't cross GameObjects.
        /// </summary>
        public void OnSwitchCharacter(InputValue value)
        {
            if (value.isPressed)
                _inputRouter?.TrySwitchCharacter();
        }

        // ── Protected API for subclasses ──────────────────────────────────────────

        /// <summary>
        /// Subclasses can call this to apply an instantaneous velocity impulse,
        /// e.g. when Scarlet is thrown or Dani uses a rope launch.
        /// </summary>
        protected void ApplyImpulse(Vector2 force)
        {
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        // ── Private: physics ──────────────────────────────────────────────────────

        private void CheckGrounded()
        {
            if (_groundCheck == null || _data == null) return;
            _isGrounded = Physics2D.OverlapCircle(
                _groundCheck.position,
                _data.GroundCheckRadius,
                _data.GroundLayer);
        }

        private void ApplyMovement()
        {
            if (_data == null) return;

            float targetVelocityX = IsControllable
                ? _moveInput.x * _data.MoveSpeed
                : 0f;

            _rb.linearVelocity = new Vector2(targetVelocityX, _rb.linearVelocity.y);

            if (_jumpQueued)
            {
                // Signal the animator first so the transition evaluates this frame,
                // before physics launches the character on the same FixedUpdate.
                _animator.SetTrigger(AnimJump);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                _rb.AddForce(Vector2.up * _data.JumpForce, ForceMode2D.Impulse);
                _jumpQueued = false;
            }

            // Flip sprite to face direction of travel.
            if (_moveInput.x > 0.01f && !_isFacingRight) Flip();
            else if (_moveInput.x < -0.01f && _isFacingRight) Flip();
        }

        private void ApplyBetterJumpGravity()
        {
            if (_data == null) return;

            if (_rb.linearVelocity.y < 0f)
            {
                // Falling — add extra downward force for snappy arc.
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_data.FallMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (_rb.linearVelocity.y > 0f && !IsJumpHeld())
            {
                // Jump button released early — truncate rise for short hop.
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_data.LowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        private void DriveAnimator()
        {
            _animator.SetFloat(AnimSpeed, Mathf.Abs(_rb.linearVelocity.x));
            _animator.SetBool(AnimGrounded, _isGrounded);
        }

        private void Flip()
        {
            _isFacingRight = !_isFacingRight;
            // 3D models use Y-rotation to face directions rather than scale mirroring.
            // Scale mirroring inverts normals and causes lighting artifacts on 3D meshes.
            transform.Rotate(0f, 180f, 0f);
        }

        /// <summary>
        /// Queries the current jump action state from the Input System.
        /// Subclasses may override if they handle input differently.
        /// </summary>
        protected virtual bool IsJumpHeld()
        {
            // Default: the Input System low-level API is not directly accessible here
            // without a reference to the action. Subclasses with a PlayerInput reference
            // should override this to read the action's IsPressed() state.
            return false;
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleActiveCharacterChanged(CharacterType activeType)
        {
            // In single-player, only the newly active character is controllable.
            IsControllable = activeType == CharacterType;

            if (!IsControllable)
                _moveInput = Vector2.zero; // Stop sliding when deactivated.
        }

        private void HandleCoopJoined(CharacterType p2Type)
        {
            // In co-op both characters are always controllable.
            IsControllable = true;
        }

        private void HandleCoopLeft()
        {
            // Revert to single-player controllability — only the active character responds.
            // GameEvents.OnActiveCharacterChanged will be fired by InputRouter to sync state.
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Freeze input when not in Playing state.
            if (state != GameState.Playing)
                _moveInput = Vector2.zero;
        }
    }
}

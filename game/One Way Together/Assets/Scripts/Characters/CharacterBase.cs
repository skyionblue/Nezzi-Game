using UnityEngine;
using OneWayTogether.Core;
using OneWayTogether.Data;
using OneWayTogether.Events;

namespace OneWayTogether.Characters
{
    /// <summary>
    /// Shared foundation for Scarlet and Dani. Owns HD-2D isometric movement on
    /// the XZ horizontal plane, facing direction, and animation state.
    /// Subclasses add character-specific abilities.
    ///
    /// Physics model: Unity CharacterController. Gravity is applied manually so
    /// the character stays grounded on the XZ floor. Input X maps to world X;
    /// input Y (forward/back stick) maps to world -Z (screen-down = world -Z for
    /// a camera pitched ~52° from above looking south).
    ///
    /// Attach <see cref="CharacterData"/> to drive all tunable values from a ScriptableObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public abstract class CharacterBase : MonoBehaviour
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Character Configuration")]
        [SerializeField] protected CharacterData _data;

        // ── Cached components ─────────────────────────────────────────────────────

        protected CharacterController _cc;
        protected Animator _animator;

        // ── State ─────────────────────────────────────────────────────────────────

        protected Vector2 _moveInput;

        // Accumulated vertical velocity — used for gravity so the character stays
        // pressed against the floor and doesn't float after stepping off ledges.
        private float _verticalVelocity;

        /// <summary>Clears accumulated fall velocity so a checkpoint teleport doesn't immediately re-fall.</summary>
        public void ResetVelocity() => _verticalVelocity = 0f;

        // Animator parameter IDs — cached to avoid per-frame string hashing.
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");

        // ── Properties ────────────────────────────────────────────────────────────

        /// <summary>Identifies which sibling this is — set by the concrete subclass.</summary>
        public abstract CharacterType CharacterType { get; }

        /// <summary>
        /// True when input is being forwarded to this character. False for the
        /// inactive character in single-player mode.
        /// </summary>
        public bool IsControllable { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        protected virtual void Awake()
        {
            _cc       = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

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

        protected virtual void Update()
        {
            ApplyMovement();
            DriveAnimator();
        }

        // ── Input API (called by InputRouter) ────────────────────────────────────

        public virtual void ReceiveMove(Vector2 move)
        {
            if (!IsControllable) return;
            _moveInput = move;
        }

        public virtual void ReceiveInteract() { }

        public virtual void ReceiveStopMove()
        {
            _moveInput = Vector2.zero;
        }

        // ── Private: physics ──────────────────────────────────────────────────────

        private void ApplyMovement()
        {
            if (_data == null) return;

            // Camera at (52,0,0): screen-right = world +X, screen-up = world +Z.
            // Input X → world X, Input Y → world +Z.
            // Joystick magnitude drives walk/run: < 0.5 = walk, >= 0.5 = run.
            Vector3 rawInput  = new Vector3(_moveInput.x, 0f, _moveInput.y);
            float   inputMag  = Mathf.Clamp01(rawInput.magnitude);
            float   speed     = inputMag >= 0.5f ? _data.RunSpeed : _data.MoveSpeed;
            Vector3 move      = IsControllable && inputMag > 0.01f
                ? rawInput.normalized * speed
                : Vector3.zero;

            // Apply gravity so the character stays pressed to the floor.
            if (_cc.isGrounded)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;

            _cc.Move((move + Vector3.up * _verticalVelocity) * Time.deltaTime);

            // Rotate to face movement direction.
            // Standard formula for a character whose mesh faces +Z at Y=0.
            if (move.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void DriveAnimator()
        {
            // XZ speed only — ignores vertical fall velocity.
            // Pass raw speed so the animator's Walk/Run thresholds (0.1 / 5.5) can
            // select the correct clip. Walk clips play at 0.1–5.5, Run at 5.5+.
            Vector3 v = _cc.velocity;
            float speed = new Vector2(v.x, v.z).magnitude;
            _animator.SetFloat(AnimSpeed, speed);
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

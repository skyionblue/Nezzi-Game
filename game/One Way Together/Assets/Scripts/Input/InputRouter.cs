using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Characters;
using OneWayTogether.Events;

namespace OneWayTogether.Input
{
    /// <summary>
    /// Owns the input action asset and routes actions to the correct character.
    /// Top-down version: Jump action and handler have been removed — the game
    /// uses gravity-free XY movement with no jump mechanic.
    /// </summary>
    public class InputRouter : MonoBehaviour
    {
        [Header("Characters (assign in Inspector)")]
        [SerializeField] private CharacterBase _scarlet;
        [SerializeField] private CharacterBase _dani;

        [Header("Input")]
        [SerializeField] private InputActionAsset _actionAsset;

        [Header("Single-Player Start")]
        [SerializeField] private CharacterType _defaultActiveCharacter = CharacterType.Scarlet;

        public CharacterType ActiveCharacter { get; private set; }
        public bool IsCoopActive { get; private set; }

        private InputAction _move;
        private InputAction _interact;
        private InputAction _switchCharacter;

        private int _lastSwitchFrame = -1;

        // Input is only forwarded to characters while the game is in the Playing state.
        // During Failure / PuzzleComplete / Paused the characters must not move.
        private bool _inputEnabled = true;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            ActiveCharacter = _defaultActiveCharacter;

            var gameplay = _actionAsset.FindActionMap("Gameplay", throwIfNotFound: true);
            _move            = gameplay.FindAction("Move",            throwIfNotFound: true);
            _interact        = gameplay.FindAction("Interact",        throwIfNotFound: true);
            _switchCharacter = gameplay.FindAction("SwitchCharacter", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _actionAsset.Enable();

            _move.performed      += OnMove;
            _move.canceled       += OnMoveCanceled;
            _interact.performed  += OnInteract;
            _switchCharacter.performed += OnSwitchCharacter;

            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            _move.performed      -= OnMove;
            _move.canceled       -= OnMoveCanceled;
            _interact.performed  -= OnInteract;
            _switchCharacter.performed -= OnSwitchCharacter;

            GameEvents.OnGameStateChanged -= HandleGameStateChanged;

            _actionAsset.Disable();
        }

        private void Start()
        {
            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        // ── Public API (also called by the mobile touch bridge) ────────────────────

        /// <summary>Feeds a movement vector to the active character. Called by the
        /// on-screen joystick bridge each frame.</summary>
        public void SetMoveInput(Vector2 dir)
        {
            if (!_inputEnabled) return;
            Active()?.ReceiveMove(dir);
        }

        /// <summary>Wire this to the on-screen Interact button's OnClick.</summary>
        public void TriggerInteract()
        {
            if (!_inputEnabled) return;
            Active()?.ReceiveInteract();
        }

        public void TrySwitchCharacter()
        {
            if (!_inputEnabled) return;
            if (IsCoopActive) return;
            if (Time.frameCount == _lastSwitchFrame) return;
            _lastSwitchFrame = Time.frameCount;

            ActiveCharacter = ActiveCharacter == CharacterType.Scarlet
                ? CharacterType.Dani
                : CharacterType.Scarlet;

            _scarlet?.ReceiveStopMove();
            _dani?.ReceiveStopMove();

            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        // ── Private input handlers ────────────────────────────────────────────────

        private void OnMove(InputAction.CallbackContext ctx)
        {
            if (!_inputEnabled) return;
            Active()?.ReceiveMove(ctx.ReadValue<Vector2>());
        }

        // Always honour a movement release so a key/stick let go during a state
        // transition can never leave a character drifting.
        private void OnMoveCanceled(InputAction.CallbackContext ctx)
            => Active()?.ReceiveStopMove();

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!_inputEnabled) return;
            Active()?.ReceiveInteract();
        }

        private void OnSwitchCharacter(InputAction.CallbackContext ctx)
            => TrySwitchCharacter();

        // When the game leaves the Playing state (failure panel, pause, level
        // complete), suppress input and halt both characters so neither keeps
        // sliding on its last movement vector.
        private void HandleGameStateChanged(GameState state)
        {
            _inputEnabled = state == GameState.Playing;

            if (!_inputEnabled)
            {
                _scarlet?.ReceiveStopMove();
                _dani?.ReceiveStopMove();
            }
        }

        private CharacterBase Active()
            => ActiveCharacter == CharacterType.Scarlet ? _scarlet : _dani;
    }
}

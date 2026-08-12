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
        }

        private void OnDisable()
        {
            _move.performed      -= OnMove;
            _move.canceled       -= OnMoveCanceled;
            _interact.performed  -= OnInteract;
            _switchCharacter.performed -= OnSwitchCharacter;

            _actionAsset.Disable();
        }

        private void Start()
        {
            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public void TrySwitchCharacter()
        {
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
            => Active()?.ReceiveMove(ctx.ReadValue<Vector2>());

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
            => Active()?.ReceiveStopMove();

        private void OnInteract(InputAction.CallbackContext ctx)
            => Active()?.ReceiveInteract();

        private void OnSwitchCharacter(InputAction.CallbackContext ctx)
            => TrySwitchCharacter();

        private CharacterBase Active()
            => ActiveCharacter == CharacterType.Scarlet ? _scarlet : _dani;
    }
}

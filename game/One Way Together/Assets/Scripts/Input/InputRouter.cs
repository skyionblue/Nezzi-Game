using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Events;

namespace OneWayTogether.Input
{
    /// <summary>
    /// Manages player-to-character assignment and handles single-player character switching.
    ///
    /// Single-player: Player 1 controls the active character. Switch Character action
    /// toggles which sibling is active; the inactive sibling holds position.
    ///
    /// Co-op: When a second device sends any input, the second player is automatically
    /// assigned to the uncontrolled character. Both characters are then independently
    /// controlled.
    ///
    /// This component owns the <see cref="PlayerInputManager"/> callbacks and routes
    /// <see cref="PlayerInput"/> instances to the correct character controllers.
    /// </summary>
    [RequireComponent(typeof(PlayerInputManager))]
    public class InputRouter : MonoBehaviour
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Single-Player Start")]
        [Tooltip("Which character Player 1 controls at game start.")]
        [SerializeField] private CharacterType _defaultActiveCharacter = CharacterType.Scarlet;

        // ── State ─────────────────────────────────────────────────────────────────

        private PlayerInputManager _playerInputManager;

        /// <summary>Player 1's PlayerInput instance (always present).</summary>
        private PlayerInput _player1Input;

        /// <summary>Player 2's PlayerInput instance (null in single-player).</summary>
        private PlayerInput _player2Input;

        /// <summary>Current active character in single-player mode.</summary>
        public CharacterType ActiveCharacter { get; private set; }

        /// <summary>True when a second player has joined.</summary>
        public bool IsCoopActive => _player2Input != null;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            _playerInputManager = GetComponent<PlayerInputManager>();
            ActiveCharacter = _defaultActiveCharacter;
        }

        private void OnEnable()
        {
            _playerInputManager.onPlayerJoined += HandlePlayerJoined;
            _playerInputManager.onPlayerLeft += HandlePlayerLeft;
        }

        private void OnDisable()
        {
            _playerInputManager.onPlayerJoined -= HandlePlayerJoined;
            _playerInputManager.onPlayerLeft -= HandlePlayerLeft;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the active character's PlayerInput when the Switch Character
        /// action fires (single-player only).
        /// </summary>
        public void OnSwitchCharacter(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (IsCoopActive) return; // Switching is disabled in co-op.

            ActiveCharacter = ActiveCharacter == CharacterType.Scarlet
                ? CharacterType.Dani
                : CharacterType.Scarlet;

            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        /// <summary>
        /// Returns true if the given character type should accept move/jump input
        /// this frame. In single-player only the active character does; in co-op both do.
        /// </summary>
        public bool IsCharacterControllable(CharacterType type)
        {
            if (IsCoopActive) return true;
            return type == ActiveCharacter;
        }

        // ── PlayerInputManager callbacks ─────────────────────────────────────────

        private void HandlePlayerJoined(PlayerInput input)
        {
            if (_player1Input == null)
            {
                _player1Input = input;
                // Player 1 always starts controlling the default character.
                return;
            }

            if (_player2Input == null)
            {
                _player2Input = input;

                // Assign Player 2 the character that Player 1 is NOT controlling.
                CharacterType p2Character = ActiveCharacter == CharacterType.Scarlet
                    ? CharacterType.Dani
                    : CharacterType.Scarlet;

                GameEvents.RaiseCoopPlayerJoined(p2Character);
                Debug.Log($"[InputRouter] Co-op activated — Player 2 assigned {p2Character}.");
            }
        }

        private void HandlePlayerLeft(PlayerInput input)
        {
            if (input == _player2Input)
            {
                _player2Input = null;
                GameEvents.RaiseCoopPlayerLeft();
                Debug.Log("[InputRouter] Co-op player left — returning to single-player.");
            }
        }
    }
}

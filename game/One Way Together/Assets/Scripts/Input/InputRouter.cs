using UnityEngine;
using UnityEngine.InputSystem;
using OneWayTogether.Events;

namespace OneWayTogether.Input
{
    /// <summary>
    /// Manages player-to-character assignment and handles single-player character switching.
    ///
    /// Architecture: Scarlet and Dani each carry their own <see cref="PlayerInput"/>
    /// component (Send Messages mode). InputRouter holds direct serialised references to
    /// those two components, tracks which character is currently active in single-player
    /// mode, and emits <see cref="GameEvents"/> so both characters can react.
    ///
    /// Single-player: Player 1 controls the active character. Switch Character action
    /// toggles which sibling is active; the inactive sibling stops responding to input.
    ///
    /// Co-op: InputRouter detects a second device by watching for any action on the
    /// inactive character's PlayerInput. Both characters are then independently controlled.
    /// </summary>
    public class InputRouter : MonoBehaviour
    {
        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Character PlayerInputs (assign in Inspector)")]
        [Tooltip("The PlayerInput component on Scarlet's GameObject.")]
        [SerializeField] private PlayerInput _scarletInput;

        [Tooltip("The PlayerInput component on Dani's GameObject.")]
        [SerializeField] private PlayerInput _daniInput;

        [Header("Single-Player Start")]
        [Tooltip("Which character Player 1 controls at game start.")]
        [SerializeField] private CharacterType _defaultActiveCharacter = CharacterType.Scarlet;

        // ── State ─────────────────────────────────────────────────────────────────

        /// <summary>Current active character in single-player mode.</summary>
        public CharacterType ActiveCharacter { get; private set; }

        /// <summary>True when a second player has joined (co-op).</summary>
        public bool IsCoopActive { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            ActiveCharacter = _defaultActiveCharacter;
        }

        private void Start()
        {
            // Emit the initial active character so both characters initialise
            // their IsControllable state correctly on the first frame.
            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by CharacterBase.OnSwitchCharacter when the SwitchCharacter
        /// action fires on either character's PlayerInput.
        /// </summary>
        public void TrySwitchCharacter()
        {
            if (IsCoopActive) return;

            ActiveCharacter = ActiveCharacter == CharacterType.Scarlet
                ? CharacterType.Dani
                : CharacterType.Scarlet;

            GameEvents.RaiseActiveCharacterChanged(ActiveCharacter);
        }

        /// <summary>
        /// Returns true if the given character type should accept move/jump input.
        /// In single-player only the active character does; in co-op both do.
        /// </summary>
        public bool IsCharacterControllable(CharacterType type)
        {
            if (IsCoopActive) return true;
            return type == ActiveCharacter;
        }
    }
}

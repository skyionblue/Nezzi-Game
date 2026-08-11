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
    /// The inactive character's PlayerInput has its actions disabled so it does not
    /// consume or suppress device pairing for the active character.
    ///
    /// Co-op: Both characters' PlayerInputs are enabled so each can pair to a separate
    /// device independently.
    ///
    /// Root cause this solves: Unity's Input System assigns the keyboard (or other
    /// device) to whichever PlayerInput initialises first. If the second PlayerInput
    /// never pairs to a device, its SendMessages callbacks never fire regardless of
    /// IsControllable — so Dani would silently receive no jump/move events.
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

        // Both PlayerInputs receive Tab simultaneously. Track the last frame a
        // switch was processed so the second call in the same frame is ignored.
        private int _lastSwitchFrame = -1;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            ActiveCharacter = _defaultActiveCharacter;
        }

        private void Start()
        {
            // Apply initial device routing before emitting the character event.
            // This ensures the correct PlayerInput has an active device when
            // CharacterBase receives OnEnable and the first ActiveCharacterChanged.
            ApplySinglePlayerInputRouting(ActiveCharacter);

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
            if (Time.frameCount == _lastSwitchFrame) return;
            _lastSwitchFrame = Time.frameCount;

            ActiveCharacter = ActiveCharacter == CharacterType.Scarlet
                ? CharacterType.Dani
                : CharacterType.Scarlet;

            ApplySinglePlayerInputRouting(ActiveCharacter);
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

        // ── Private ───────────────────────────────────────────────────────────────

        /// <summary>
        /// In single-player, routes all device input to the active character by
        /// activating that character's PlayerInput actions and deactivating the
        /// inactive character's. This prevents the Input System from pairing the
        /// sole keyboard to only the first-initialised PlayerInput and silently
        /// dropping events for whichever character initialised second.
        /// </summary>
        private void ApplySinglePlayerInputRouting(CharacterType activeCharacter)
        {
            if (_scarletInput == null || _daniInput == null) return;

            bool scarletActive = activeCharacter == CharacterType.Scarlet;

            // ActivateInput / DeactivateInput enable or disable the underlying
            // InputActionAsset actions on each PlayerInput without destroying
            // the component. The active PlayerInput will (re)pair to an available
            // device on the next input event; the inactive one stops firing messages.
            if (scarletActive)
            {
                _daniInput.DeactivateInput();
                _scarletInput.ActivateInput();
            }
            else
            {
                _scarletInput.DeactivateInput();
                _daniInput.ActivateInput();
            }
        }
    }
}

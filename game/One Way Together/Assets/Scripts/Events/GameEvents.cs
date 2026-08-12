using System;
using UnityEngine;

namespace OneWayTogether.Events
{
    /// <summary>
    /// Central static event bus. All cross-system communication passes through here.
    /// Systems subscribe in OnEnable and unsubscribe in OnDisable — never hold
    /// subscriptions across scene loads.
    /// </summary>
    public static class GameEvents
    {
        // ── Game State ──────────────────────────────────────────────────────────

        /// <summary>Fired when game state changes (Playing, Paused, PuzzleComplete, Failure).</summary>
        public static event Action<GameState> OnGameStateChanged;

        // ── Characters ──────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when the active character switches in single-player mode.
        /// Payload: the CharacterType now being controlled.
        /// </summary>
        public static event Action<CharacterType> OnActiveCharacterChanged;

        /// <summary>
        /// Fired when a character dies / triggers a failure condition.
        /// Payload: which character failed.
        /// </summary>
        public static event Action<CharacterType> OnCharacterFailed;

        /// <summary>
        /// Fired when both characters have entered the ReunionTrigger zone.
        /// No payload — puzzle is complete.
        /// </summary>
        public static event Action OnReunionAchieved;

        // ── Checkpoints ─────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when a checkpoint is activated for the first time.
        /// Payload: world-space position of the checkpoint.
        /// </summary>
        public static event Action<Vector3> OnCheckpointActivated;

        /// <summary>
        /// Fired when both characters have been reset to the last checkpoint.
        /// </summary>
        public static event Action OnCheckpointReset;

        // ── Coins ───────────────────────────────────────────────────────────────

        /// <summary>Fired when a coin is collected. Payload: total coin count after collection.</summary>
        public static event Action<int> OnCoinCollected;

        /// <summary>Fired when coins are spent. Payload: total coin count after spending.</summary>
        public static event Action<int> OnCoinsSpent;

        // ── Collectibles ────────────────────────────────────────────────────────

        /// <summary>Fired when a secret key is collected. Payload: key identifier string.</summary>
        public static event Action<string> OnSecretKeyCollected;

        // ── Hints ───────────────────────────────────────────────────────────────

        /// <summary>Fired when a hint is purchased and revealed. Payload: hint text, tier (1-3).</summary>
        public static event Action<string, int> OnHintRevealed;

        /// <summary>Fired when a hint request is refused (none left, or not enough coins). Payload: reason.</summary>
        public static event Action<string> OnHintDenied;

        // ── Input / Co-op ───────────────────────────────────────────────────────

        /// <summary>
        /// Fired when a second player joins, switching from single-player to co-op.
        /// Payload: the CharacterType the second player has been assigned.
        /// </summary>
        public static event Action<CharacterType> OnCoopPlayerJoined;

        /// <summary>Fired when the second player disconnects, reverting to single-player.</summary>
        public static event Action OnCoopPlayerLeft;

        // ── Puzzle Elements ─────────────────────────────────────────────────────

        /// <summary>Fired when a pressure plate activation state changes.</summary>
        public static event Action<string, bool> OnPressurePlateChanged;

        /// <summary>Fired when a gate/door opens or closes. Payload: gate ID, isOpen.</summary>
        public static event Action<string, bool> OnGateStateChanged;

        // ── Invocation helpers ───────────────────────────────────────────────────
        // Centralised raise methods keep null-check boilerplate out of every caller.

        public static void RaiseGameStateChanged(GameState state) =>
            OnGameStateChanged?.Invoke(state);

        public static void RaiseActiveCharacterChanged(CharacterType type) =>
            OnActiveCharacterChanged?.Invoke(type);

        public static void RaiseCharacterFailed(CharacterType type) =>
            OnCharacterFailed?.Invoke(type);

        public static void RaiseReunionAchieved() =>
            OnReunionAchieved?.Invoke();

        public static void RaiseCheckpointActivated(Vector3 position) =>
            OnCheckpointActivated?.Invoke(position);

        public static void RaiseCheckpointReset() =>
            OnCheckpointReset?.Invoke();

        public static void RaiseCoinCollected(int total) =>
            OnCoinCollected?.Invoke(total);

        public static void RaiseCoinsSpent(int total) =>
            OnCoinsSpent?.Invoke(total);

        public static void RaiseSecretKeyCollected(string keyId) =>
            OnSecretKeyCollected?.Invoke(keyId);

        public static void RaiseHintRevealed(string text, int tier) =>
            OnHintRevealed?.Invoke(text, tier);

        public static void RaiseHintDenied(string reason) =>
            OnHintDenied?.Invoke(reason);

        public static void RaiseCoopPlayerJoined(CharacterType assignedType) =>
            OnCoopPlayerJoined?.Invoke(assignedType);

        public static void RaiseCoopPlayerLeft() =>
            OnCoopPlayerLeft?.Invoke();

        public static void RaisePressurePlateChanged(string plateId, bool isActive) =>
            OnPressurePlateChanged?.Invoke(plateId, isActive);

        public static void RaiseGateStateChanged(string gateId, bool isOpen) =>
            OnGateStateChanged?.Invoke(gateId, isOpen);
    }

    /// <summary>The four high-level states the game can be in at runtime.</summary>
    public enum GameState
    {
        /// <summary>Normal gameplay — characters are controllable.</summary>
        Playing,
        /// <summary>Pause menu is open — input suppressed.</summary>
        Paused,
        /// <summary>Both characters reunited — level complete sequence running.</summary>
        PuzzleComplete,
        /// <summary>A failure was triggered — awaiting checkpoint reset or coin respawn.</summary>
        Failure
    }

    /// <summary>Identifies which sibling a component or event refers to.</summary>
    public enum CharacterType
    {
        Scarlet,
        Dani
    }
}

using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Owns the authoritative game state machine. The only singleton in the project.
    /// All other systems communicate via <see cref="GameEvents"/> rather than referencing
    /// GameManager directly, keeping coupling minimal.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        public static GameManager Instance { get; private set; }

        // ── State ────────────────────────────────────────────────────────────────

        /// <summary>The current high-level game state.</summary>
        public GameState CurrentState { get; private set; } = GameState.Playing;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnCharacterFailed += HandleCharacterFailed;
            GameEvents.OnReunionAchieved += HandleReunionAchieved;
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterFailed -= HandleCharacterFailed;
            GameEvents.OnReunionAchieved -= HandleReunionAchieved;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Transitions to <see cref="GameState.Paused"/> if currently playing,
        /// or back to <see cref="GameState.Playing"/> if already paused.
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                SetState(GameState.Playing);
        }

        /// <summary>
        /// Forces a transition to a specific state. Use sparingly — prefer
        /// reacting to events over calling this directly.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            ApplyStateEffect(newState);
            GameEvents.RaiseGameStateChanged(newState);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleCharacterFailed(CharacterType _)
        {
            if (CurrentState != GameState.Playing) return;
            SetState(GameState.Failure);
        }

        private void HandleReunionAchieved()
        {
            if (CurrentState != GameState.Playing) return;
            SetState(GameState.PuzzleComplete);
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void ApplyStateEffect(GameState state)
        {
            switch (state)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.PuzzleComplete:
                case GameState.Failure:
                    // Keep timeScale at 1 — UI/animation drives pacing here.
                    break;
            }
        }
    }
}

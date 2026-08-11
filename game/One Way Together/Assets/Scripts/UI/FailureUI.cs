using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneWayTogether.Collectibles;
using OneWayTogether.Core;
using OneWayTogether.Events;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Shown when <see cref="GameState.Failure"/> is entered. Offers two choices:
    ///
    /// 1. Reset to checkpoint (free, always available).
    /// 2. Respawn in place (costs coins — button greyed out if insufficient).
    ///
    /// The panel does not drive the reset logic itself — it calls
    /// <see cref="CheckpointManager"/> and <see cref="CoinManager"/> which own
    /// the actual mechanics.
    /// </summary>
    public class FailureUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button _checkpointButton;
        [SerializeField] private Button _respawnButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _respawnCostLabel;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnCoinCollected    += HandleCoinChanged;
            GameEvents.OnCoinsSpent       += HandleCoinChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnCoinCollected    -= HandleCoinChanged;
            GameEvents.OnCoinsSpent       -= HandleCoinChanged;
        }

        private void Start()
        {
            _panelRoot?.SetActive(false);

            _checkpointButton?.onClick.AddListener(OnCheckpointPressed);
            _respawnButton?.onClick.AddListener(OnRespawnPressed);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleGameStateChanged(GameState state)
        {
            bool show = state == GameState.Failure;
            _panelRoot?.SetActive(show);

            if (show) RefreshRespawnButton();
        }

        private void HandleCoinChanged(int _) => RefreshRespawnButton();

        // ── Button callbacks ──────────────────────────────────────────────────────

        private void OnCheckpointPressed()
        {
            CheckpointManager cm = FindAnyObjectByType<CheckpointManager>();
            cm?.ResetToCheckpoint();
            _panelRoot?.SetActive(false);
        }

        private void OnRespawnPressed()
        {
            bool success = CoinManager.Instance?.TryRespawnInPlace() ?? false;
            if (success) _panelRoot?.SetActive(false);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void RefreshRespawnButton()
        {
            if (_respawnButton == null) return;

            CoinManager cm = CoinManager.Instance;
            if (cm == null) return;

            // Read cost directly from CoinManager — avoids coupling to CoinSystemData here.
            // CoinManager exposes this via TryRespawnInPlace internally; for UI display
            // we calculate it from the public CoinCount heuristic.
            // A future refactor can expose RespawnCost as a property on CoinManager.
            bool canAfford = cm.CoinCount >= 1; // Placeholder — wire to actual cost when exposed.
            _respawnButton.interactable = canAfford;

            if (_respawnCostLabel != null)
                _respawnCostLabel.text = canAfford ? "Respawn (coins)" : "Not enough coins";
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
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
    ///
    /// Uses legacy UnityEngine.UI.Text (not TextMeshPro): the project's TMP font
    /// atlas renders as tofu in the game view on this setup, while the built-in
    /// LegacyRuntime.ttf font always renders. See HUDController for the same choice.
    /// </summary>
    public class FailureUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button _checkpointButton;
        [SerializeField] private Button _respawnButton;

        [Header("Labels")]
        [Tooltip("Optional headline shown on the failure panel.")]
        [SerializeField] private Text _titleLabel;

        [Tooltip("Dynamic label on the respawn button — shows the coin cost / affordability.")]
        [SerializeField] private Text _respawnCostLabel;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            // Guarantee a renderable font on every label under the panel. The built-in
            // font cannot be set through the serialized 'font' property from tooling, and
            // the button child labels have no direct reference here, so font them all.
            if (_panelRoot != null)
            {
                Font legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                foreach (Text label in _panelRoot.GetComponentsInChildren<Text>(includeInactive: true))
                    if (label.font == null) label.font = legacy;
            }
        }

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
            if (_panelRoot != null) _panelRoot.SetActive(false);

            if (_checkpointButton != null) _checkpointButton.onClick.AddListener(OnCheckpointPressed);
            if (_respawnButton != null) _respawnButton.onClick.AddListener(OnRespawnPressed);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleGameStateChanged(GameState state)
        {
            bool show = state == GameState.Failure;
            if (_panelRoot != null) _panelRoot.SetActive(show);

            if (show)
            {
                if (_titleLabel != null) _titleLabel.text = "One of you is lost!";
                RefreshRespawnButton();
            }
        }

        private void HandleCoinChanged(int _) => RefreshRespawnButton();

        // ── Button callbacks ──────────────────────────────────────────────────────

        private void OnCheckpointPressed()
        {
            CheckpointManager cm = FindAnyObjectByType<CheckpointManager>();
            if (cm != null) cm.ResetToCheckpoint();
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void OnRespawnPressed()
        {
            bool success = CoinManager.Instance != null && CoinManager.Instance.TryRespawnInPlace();
            if (success && _panelRoot != null) _panelRoot.SetActive(false);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void RefreshRespawnButton()
        {
            if (_respawnButton == null) return;

            CoinManager cm = CoinManager.Instance;
            if (cm == null) return;

            int cost = cm.RespawnCost;
            bool canAfford = cm.CanAffordRespawn;

            _respawnButton.interactable = canAfford;

            if (_respawnCostLabel != null)
                _respawnCostLabel.text = canAfford
                    ? $"Respawn Here ({cost})"
                    : $"Need {cost} coins";
        }
    }
}

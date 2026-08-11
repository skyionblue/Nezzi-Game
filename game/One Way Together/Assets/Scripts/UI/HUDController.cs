using UnityEngine;
using TMPro;
using OneWayTogether.Events;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Drives the in-game HUD. Subscribes to coin events and updates the coin
    /// counter display. Additional HUD elements (active character indicator,
    /// key collection, pause state) slot in here.
    ///
    /// All text uses TextMeshPro to avoid GC from legacy string allocation.
    /// The coin count is cached as an int; string conversion uses a format cache.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Coin Display")]
        [SerializeField] private TextMeshProUGUI _coinCountText;

        [Header("Character Indicator")]
        [Tooltip("GameObject shown when Scarlet is active in single-player.")]
        [SerializeField] private GameObject _scarletIndicator;

        [Tooltip("GameObject shown when Dani is active in single-player.")]
        [SerializeField] private GameObject _daniIndicator;

        // Cache the format string to avoid runtime allocation.
        private static readonly string CoinFormat = "Coins: {0}";

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnCoinCollected          += HandleCoinChanged;
            GameEvents.OnCoinsSpent             += HandleCoinChanged;
            GameEvents.OnActiveCharacterChanged += HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       += HandleCoopJoined;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinCollected          -= HandleCoinChanged;
            GameEvents.OnCoinsSpent             -= HandleCoinChanged;
            GameEvents.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            GameEvents.OnCoopPlayerJoined       -= HandleCoopJoined;
        }

        private void Start()
        {
            UpdateCoinDisplay(0);
            SetActiveCharacterIndicator(CharacterType.Scarlet);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void HandleCoinChanged(int totalCoins) => UpdateCoinDisplay(totalCoins);

        private void HandleActiveCharacterChanged(CharacterType type) =>
            SetActiveCharacterIndicator(type);

        private void HandleCoopJoined(CharacterType _)
        {
            // In co-op, hide the single-player active character indicator.
            _scarletIndicator?.SetActive(false);
            _daniIndicator?.SetActive(false);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void UpdateCoinDisplay(int count)
        {
            if (_coinCountText == null) return;
            _coinCountText.SetText(CoinFormat, count);
        }

        private void SetActiveCharacterIndicator(CharacterType type)
        {
            _scarletIndicator?.SetActive(type == CharacterType.Scarlet);
            _daniIndicator?.SetActive(type == CharacterType.Dani);
        }
    }
}

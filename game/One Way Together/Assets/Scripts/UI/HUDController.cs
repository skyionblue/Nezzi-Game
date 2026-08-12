using UnityEngine;
using UnityEngine.UI;
using OneWayTogether.Events;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Drives the in-game HUD. Subscribes to coin events and updates the coin
    /// counter display. Additional HUD elements (active character indicator,
    /// key collection, pause state) slot in here.
    ///
    /// Uses legacy UnityEngine.UI.Text (not TextMeshPro): the project's TMP font
    /// atlas renders as tofu (empty boxes) in the game view on this setup, while
    /// the built-in LegacyRuntime.ttf font always renders. The on-screen SWAP/USE
    /// buttons use the same legacy Text for the same reason. The coin count updates
    /// only on pickup/spend, so the string allocation is negligible.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Coin Display")]
        [SerializeField] private Text _coinCountText;

        [Header("Character Indicator")]
        [Tooltip("GameObject shown when Scarlet is active in single-player.")]
        [SerializeField] private GameObject _scarletIndicator;

        [Tooltip("GameObject shown when Dani is active in single-player.")]
        [SerializeField] private GameObject _daniIndicator;

        // Cache the format string to avoid runtime allocation.
        private static readonly string CoinFormat = "Coins: {0}";

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            // Guarantee a renderable font. Unity no longer ships a default font on
            // new UI.Text components, and the built-in font cannot be assigned via
            // the serialized 'font' property from tooling, so assign it here.
            if (_coinCountText != null && _coinCountText.font == null)
                _coinCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

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
            // Use explicit null checks (not ?.) — Unity's "fake null" for an
            // unassigned Object field is not C# null, so ?. would not short-circuit
            // and SetActive would throw UnassignedReferenceException.
            if (_scarletIndicator != null) _scarletIndicator.SetActive(false);
            if (_daniIndicator != null) _daniIndicator.SetActive(false);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void UpdateCoinDisplay(int count)
        {
            if (_coinCountText == null) return;
            _coinCountText.text = string.Format(CoinFormat, count);
        }

        private void SetActiveCharacterIndicator(CharacterType type)
        {
            // Explicit null checks — see HandleCoopJoined for why ?. is unsafe here.
            if (_scarletIndicator != null) _scarletIndicator.SetActive(type == CharacterType.Scarlet);
            if (_daniIndicator != null) _daniIndicator.SetActive(type == CharacterType.Dani);
        }
    }
}

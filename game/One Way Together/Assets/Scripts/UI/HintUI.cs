using UnityEngine;
using UnityEngine.UI;
using OneWayTogether.Core;
using OneWayTogether.Events;

namespace OneWayTogether.UI
{
    /// <summary>
    /// On-screen hint button plus the panel that shows a revealed hint. The button
    /// label shows the coin cost of the next hint; pressing it asks
    /// <see cref="HintManager"/> to reveal the next tier (spending coins). Revealed
    /// hints — and "not enough coins" / "no more hints" feedback — appear in the panel.
    ///
    /// Uses legacy UnityEngine.UI.Text (see HUDController for why TMP is avoided here).
    /// </summary>
    public class HintUI : MonoBehaviour
    {
        [Header("Hint Button")]
        [SerializeField] private Button _hintButton;
        [SerializeField] private Text _hintButtonLabel;

        [Header("Hint Panel")]
        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private Text _hintText;
        [SerializeField] private Button _closeButton;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            // Guarantee a renderable font on all labels created via tooling.
            Font legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FontChildren(_hintButton, legacy);
            FontChildren(_hintPanel != null ? _hintPanel.transform : null, legacy);
        }

        private void OnEnable()
        {
            GameEvents.OnHintRevealed  += HandleHintRevealed;
            GameEvents.OnHintDenied    += HandleHintDenied;
            GameEvents.OnCoinCollected += HandleCoinChanged;
            GameEvents.OnCoinsSpent    += HandleCoinChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnHintRevealed  -= HandleHintRevealed;
            GameEvents.OnHintDenied    -= HandleHintDenied;
            GameEvents.OnCoinCollected -= HandleCoinChanged;
            GameEvents.OnCoinsSpent    -= HandleCoinChanged;
        }

        private void Start()
        {
            if (_hintPanel != null) _hintPanel.SetActive(false);

            if (_hintButton != null) _hintButton.onClick.AddListener(OnHintPressed);
            if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);

            RefreshButton();
        }

        // ── Button callbacks ──────────────────────────────────────────────────────

        private void OnHintPressed()
        {
            if (HintManager.Instance != null) HintManager.Instance.RequestNextHint();
        }

        private void ClosePanel()
        {
            if (_hintPanel != null) _hintPanel.SetActive(false);
        }

        // ── Event handlers ──────────────────────────────────────────────────────

        private void HandleHintRevealed(string text, int tier)
        {
            if (_hintText != null) _hintText.text = $"Hint {tier}\n\n{text}";
            if (_hintPanel != null) _hintPanel.SetActive(true);
            RefreshButton();
        }

        private void HandleHintDenied(string reason)
        {
            if (_hintText != null) _hintText.text = reason;
            if (_hintPanel != null) _hintPanel.SetActive(true);
        }

        private void HandleCoinChanged(int _) => RefreshButton();

        // ── Private ───────────────────────────────────────────────────────────────

        private void RefreshButton()
        {
            HintManager hm = HintManager.Instance;
            if (hm == null) return;

            bool hasMore = hm.HasMoreHints;
            if (_hintButton != null) _hintButton.interactable = hasMore;

            if (_hintButtonLabel != null)
                _hintButtonLabel.text = hasMore ? $"Hint ({hm.NextHintCost})" : "No hints";
        }

        private static void FontChildren(Component root, Font legacy)
        {
            if (root == null) return;
            foreach (Text label in root.GetComponentsInChildren<Text>(includeInactive: true))
                if (label.font == null) label.font = legacy;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OneWayTogether.Events;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Shown when <see cref="GameState.PuzzleComplete"/> is entered (both siblings reunite).
    /// Builds its own fullscreen overlay in code so no pre-wired prefab is required —
    /// just attach this script to any active GameObject in the scene.
    ///
    /// Uses legacy UnityEngine.UI.Text (not TextMeshPro) — the project's TMP font atlas
    /// renders as tofu in the game view on this setup. See FailureUI for the same choice.
    /// </summary>
    public class PuzzleCompleteUI : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text       _titleLabel;
        private Button     _playAgainButton;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            BuildPanel();
            _panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        // ── Event handler ─────────────────────────────────────────────────────────

        private void HandleGameStateChanged(GameState state)
        {
            bool show = state == GameState.PuzzleComplete;
            if (_panelRoot != null) _panelRoot.SetActive(show);
        }

        // ── Button callback ───────────────────────────────────────────────────────

        private void OnPlayAgainPressed()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // ── Panel construction ────────────────────────────────────────────────────

        private void BuildPanel()
        {
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Find or create a canvas to parent into.
            Canvas canvas = FindAnyObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : transform;

            // Fullscreen dark overlay.
            _panelRoot = new GameObject("PuzzleCompletePanel");
            _panelRoot.transform.SetParent(canvasTransform, false);

            RectTransform panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = _panelRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.78f);

            // "You found your way home!" headline.
            GameObject titleGO = new GameObject("TitleLabel");
            titleGO.transform.SetParent(_panelRoot.transform, false);

            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.55f);
            titleRect.anchorMax = new Vector2(0.9f, 0.75f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            _titleLabel = titleGO.AddComponent<Text>();
            _titleLabel.font      = legacyFont;
            _titleLabel.fontSize  = 48;
            _titleLabel.alignment = TextAnchor.MiddleCenter;
            _titleLabel.color     = new Color(1f, 0.95f, 0.6f);
            _titleLabel.text      = "You found your way home!";

            // Play Again button.
            GameObject btnGO = new GameObject("PlayAgainButton");
            btnGO.transform.SetParent(_panelRoot.transform, false);

            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.3f, 0.35f);
            btnRect.anchorMax = new Vector2(0.7f, 0.48f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.2f);

            _playAgainButton = btnGO.AddComponent<Button>();
            _playAgainButton.targetGraphic = btnImage;
            _playAgainButton.onClick.AddListener(OnPlayAgainPressed);

            GameObject btnTextGO = new GameObject("Label");
            btnTextGO.transform.SetParent(btnGO.transform, false);

            RectTransform btnTextRect = btnTextGO.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.font      = legacyFont;
            btnText.fontSize  = 32;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color     = Color.white;
            btnText.text      = "Play Again";
        }
    }
}

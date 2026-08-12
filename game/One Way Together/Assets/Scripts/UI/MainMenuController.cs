using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Drives the main menu. Builds its own Canvas + EventSystem entirely in code
    /// so the UI is independent of TMP and the original scene's Canvas setup.
    /// Attach this script to any GameObject in the MainMenu scene.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string _firstPuzzleScene = "World1_Puzzle1";

        private void Awake()
        {
            // Disable every existing Canvas so they don't intercept input.
            foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                c.gameObject.SetActive(false);

            // Ensure exactly one EventSystem exists.
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            BuildMenu();
        }

        public void PlayGame()  => SceneManager.LoadScene(_firstPuzzleScene);

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void BuildMenu()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasGO = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Dark background.
            MakeImage(canvasGO.transform, "Background",
                Vector2.zero, Vector2.one, new Color(0.08f, 0.12f, 0.18f));

            // Title text.
            MakeText(canvasGO.transform, "Title", "One Way Together",
                new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.80f), 72, Color.white);

            // Tagline.
            MakeText(canvasGO.transform, "Tagline", "Find your way home together.",
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.64f), 32,
                new Color(0.8f, 0.8f, 0.8f));

            // Play button.
            MakeButton(canvasGO.transform, "PlayButton", "Play",
                new Vector2(0.2f, 0.40f), new Vector2(0.8f, 0.52f),
                new Color(0.18f, 0.55f, 0.18f), font, PlayGame);

            // Quit button (desktop only).
            MakeButton(canvasGO.transform, "QuitButton", "Quit",
                new Vector2(0.2f, 0.27f), new Vector2(0.8f, 0.38f),
                new Color(0.45f, 0.12f, 0.12f), font, QuitGame);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private Image MakeImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            SetRect(go, anchorMin, anchorMax);
            Image img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text MakeText(Transform parent, string name, string content,
            Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            SetRect(go, anchorMin, anchorMax);
            Text t = go.AddComponent<Text>();
            t.font      = font;
            t.text      = content;
            t.fontSize  = fontSize;
            t.fontStyle = name == "Title" ? FontStyle.Bold : FontStyle.Normal;
            t.alignment = TextAnchor.MiddleCenter;
            t.color     = color;
            return t;
        }

        private Button MakeButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax,
            Color bgColor, Font font,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            SetRect(btnGO, anchorMin, anchorMax);

            Image img = btnGO.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.highlightedColor = Color.white * 1.2f;
            cb.pressedColor     = bgColor * 0.7f;
            btn.colors = cb;

            btn.onClick.AddListener(onClick);

            // Label inside button.
            MakeText(btnGO.transform, "Label", label,
                Vector2.zero, Vector2.one, 42, Color.white);

            return btn;
        }

        private void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Drives the main menu. Attach to any persistent GameObject in the MainMenu scene.
    ///
    /// The "Play" button onClick calls <see cref="PlayGame"/> — no LevelSequenceController
    /// reference is needed here because LSC lives inside World1_Puzzle1 and carries itself
    /// forward via DontDestroyOnLoad.
    ///
    /// Also replaces any TextMeshProUGUI components on the canvas with legacy
    /// UnityEngine.UI.Text at startup. The project's TMP font atlas renders as black
    /// tofu boxes in the game view; the built-in LegacyRuntime.ttf always renders.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("Scene name to load when Play is pressed. Must match Build Settings entry.")]
        [SerializeField] private string _firstPuzzleScene = "World1_Puzzle1";

        private void Awake()
        {
            FixTMPText();
        }

        /// <summary>Called by the Play button onClick event.</summary>
        public void PlayGame()
        {
            SceneManager.LoadScene(_firstPuzzleScene);
        }

        /// <summary>Called by the Quit button onClick event (desktop builds only).</summary>
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void FixTMPText()
        {
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Find every TMP component in the scene and replace it with a legacy Text.
            // TMP renders as black tofu boxes on this setup; legacy font always renders.
            var tmpComponents = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var tmp in tmpComponents)
            {
                string content = tmp.text;
                float size     = tmp.fontSize;
                Color color    = tmp.color;
                GameObject go  = tmp.gameObject;

                tmp.enabled = false; // hide without waiting for Destroy

                // Only add a legacy Text if one isn't already there.
                if (go.GetComponent<Text>() == null)
                {
                    Text legacyText       = go.AddComponent<Text>();
                    legacyText.font       = legacyFont;
                    legacyText.text       = content;
                    legacyText.fontSize   = Mathf.RoundToInt(size);
                    legacyText.color      = color;
                    legacyText.alignment  = TextAnchor.MiddleCenter;
                    legacyText.resizeTextForBestFit = false;
                }
            }
        }
    }
}

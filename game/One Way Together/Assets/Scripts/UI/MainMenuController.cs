using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneWayTogether.UI
{
    /// <summary>
    /// Drives the main menu. Attach to any persistent GameObject in the MainMenu scene.
    ///
    /// The "Play" button onClick calls <see cref="PlayGame"/> — no LevelSequenceController
    /// reference is needed here because LSC lives inside World1_Puzzle1 and carries itself
    /// forward via DontDestroyOnLoad.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("Scene name to load when Play is pressed. Must match Build Settings entry.")]
        [SerializeField] private string _firstPuzzleScene = "World1_Puzzle1";

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
    }
}

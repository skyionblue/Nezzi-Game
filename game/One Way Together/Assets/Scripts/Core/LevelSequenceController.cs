using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Owns the ordered list of puzzle scenes and drives level-to-level progression.
    ///
    /// Listens for <see cref="GameEvents.OnReunionAchieved"/> and loads the next
    /// scene in the sequence after a short completion delay. When all puzzles are
    /// solved it returns to MainMenu.
    ///
    /// DontDestroyOnLoad — attach to one GameObject in the first puzzle scene
    /// (World1_Puzzle1). The singleton pattern prevents a duplicate from loading
    /// when later scenes contain the same component.
    /// </summary>
    public class LevelSequenceController : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        public static LevelSequenceController Instance { get; private set; }

        // ── Serialised ───────────────────────────────────────────────────────────

        [Tooltip("Ordered list of scene names to load in sequence.")]
        [SerializeField] private string[] _levelScenes = { "World1_Puzzle1", "World1_Puzzle2", "World1_Puzzle3", "World1_Puzzle4" };

        [Tooltip("Seconds to wait after reunion before loading the next scene.")]
        [SerializeField] private float _completionDelay = 2.5f;

        // ── State ────────────────────────────────────────────────────────────────

        private int  _currentIndex;
        private bool _advancing;
        private Coroutine _advanceCoroutine;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

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

        private void OnEnable()  => GameEvents.OnReunionAchieved += HandleReunion;
        private void OnDisable() => GameEvents.OnReunionAchieved -= HandleReunion;

        private void OnDestroy()
        {
            if (_advanceCoroutine != null)
                StopCoroutine(_advanceCoroutine);

            if (Instance == this)
                Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resets the sequence index and loads the first puzzle scene.
        /// Call this from the MainMenu "Play" button (or from a boot scene).
        /// </summary>
        public void StartFromBeginning()
        {
            _currentIndex = 0;
            _advancing    = false;

            if (_levelScenes.Length == 0)
            {
                Debug.LogWarning("[LevelSequenceController] _levelScenes is empty — nothing to load.", this);
                return;
            }

            SceneManager.LoadScene(_levelScenes[0]);
        }

        // ── Event handler ────────────────────────────────────────────────────────

        private void HandleReunion()
        {
            if (_advancing) return;
            _advancing = true;
            _advanceCoroutine = StartCoroutine(AdvanceAfterDelay());
        }

        // ── Coroutine ────────────────────────────────────────────────────────────

        private IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSeconds(_completionDelay);

            _advancing        = false;
            _advanceCoroutine = null;
            _currentIndex++;

            if (_currentIndex < _levelScenes.Length)
                SceneManager.LoadScene(_levelScenes[_currentIndex]);
            else
                SceneManager.LoadScene("MainMenu");
        }
    }
}

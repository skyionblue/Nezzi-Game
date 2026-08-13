using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using OneWayTogether.Camera;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// V2 replacement for LevelSequenceController.
    ///
    /// After each reunion the camera dollies to the next puzzle zone, activates
    /// it, and resumes gameplay. After the final zone it returns to MainMenu.
    ///
    /// Each element in _zones describes one puzzle zone:
    ///   • zoneRoot  — the parent GO to activate when this zone becomes active
    ///                 (set to null for zone 0, which is active from the start)
    ///   • lookAt    — world position the camera frames (centre of the zone)
    ///   • height    — camera metres above lookAt
    ///   • fov       — camera field-of-view for this zone
    ///
    /// Zone 0 is always active at load. All other zoneRoots start inactive and
    /// are activated by this script just before the dolly lands.
    /// </summary>
    public class CityZoneSequencer : MonoBehaviour
    {
        // ── Zone data ─────────────────────────────────────────────────────────────

        [System.Serializable]
        public class ZoneAnchor
        {
            [Tooltip("Puzzle zone root GameObject. Leave null for zone 0 (active at start).")]
            public GameObject zoneRoot;

            [Tooltip("World position the camera looks at when this zone is active.")]
            public Vector3 lookAt;

            [Tooltip("Camera height above lookAt.")]
            public float height = 28f;

            [Tooltip("Camera FOV at this zone.")]
            public float fov = 52f;
        }

        // ── Inspector ─────────────────────────────────────────────────────────────

        [Header("Zones (ordered — zone 0 is active at load)")]
        [SerializeField] private ZoneAnchor[] _zones;

        [Header("Transition")]
        [Tooltip("Seconds between reunion and camera starting to move.")]
        [SerializeField] private float _celebrationDelay = 2.5f;

        [Tooltip("Seconds for the camera to travel between zones.")]
        [SerializeField] private float _dollyDuration = 4.5f;

        [SerializeField] private AnimationCurve _dollyCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _mainCamera;

        [Header("End of game")]
        [SerializeField] private string _mainMenuScene = "MainMenu";

        // ── State ─────────────────────────────────────────────────────────────────

        private int  _currentZone   = 0;
        private bool _transitioning = false;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void OnEnable()  => GameEvents.OnReunionAchieved += HandleReunion;
        private void OnDisable() => GameEvents.OnReunionAchieved -= HandleReunion;

        // ── Event handler ─────────────────────────────────────────────────────────

        private void HandleReunion()
        {
            if (_transitioning) return;
            _transitioning = true;
            StartCoroutine(AdvanceZone());
        }

        // ── Zone transition coroutine ─────────────────────────────────────────────

        private IEnumerator AdvanceZone()
        {
            bool isLastZone = _currentZone >= _zones.Length - 1;

            // Wait one frame so GameManager can process OnReunionAchieved first.
            yield return null;

            if (!isLastZone)
            {
                // Suppress "You found your way home!" UI — the dolly IS the celebration.
                GameEvents.RaiseGameStateChanged(GameState.ZoneTransition);
            }
            // If last zone: let PuzzleComplete state persist so the UI shows.

            yield return new WaitForSeconds(_celebrationDelay);

            if (isLastZone)
            {
                SceneManager.LoadScene(_mainMenuScene);
                yield break;
            }

            _currentZone++;
            var next = _zones[_currentZone];

            // ── Dolly ──────────────────────────────────────────────────────────

            // Disable follow so we drive the camera manually.
            var follow = _mainCamera != null
                ? _mainCamera.GetComponent<IsometricCameraFollow>()
                : null;
            if (follow) follow.enabled = false;

            Vector3    startPos = _mainCamera.transform.position;
            Quaternion startRot = _mainCamera.transform.rotation;
            float      startFOV = _mainCamera.fieldOfView;

            Vector3    endPos = next.lookAt + new Vector3(0f, next.height, -next.height * 0.85f);
            Quaternion endRot = Quaternion.LookRotation((next.lookAt - endPos).normalized);
            float      endFOV = next.fov;

            float t = 0f;
            while (t < _dollyDuration)
            {
                t += Time.deltaTime;
                float p = _dollyCurve.Evaluate(Mathf.Clamp01(t / _dollyDuration));
                if (_mainCamera)
                {
                    _mainCamera.transform.position = Vector3.Lerp(startPos, endPos, p);
                    _mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, p);
                    _mainCamera.fieldOfView        = Mathf.Lerp(startFOV, endFOV, p);
                }
                yield return null;
            }

            // Snap to final.
            if (_mainCamera)
            {
                _mainCamera.transform.position = endPos;
                _mainCamera.transform.rotation = endRot;
                _mainCamera.fieldOfView        = endFOV;
            }

            // ── Activate next zone ────────────────────────────────────────────

            if (next.zoneRoot != null)
                next.zoneRoot.SetActive(true);

            // Re-enable isometric follow.
            if (follow) follow.enabled = true;

            GameEvents.RaiseGameStateChanged(GameState.Playing);
            _transitioning = false;
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmos()
        {
            if (_zones == null) return;
            for (int i = 0; i < _zones.Length; i++)
            {
                var z = _zones[i];
                Gizmos.color = i == _currentZone
                    ? new Color(0f, 1f, 0.3f, 0.9f)
                    : new Color(0.3f, 0.7f, 1f, 0.5f);
                Gizmos.DrawWireSphere(z.lookAt, 1.5f);
                Vector3 camPos = z.lookAt + new Vector3(0f, z.height, -z.height * 0.85f);
                Gizmos.DrawLine(z.lookAt, camPos);
                Gizmos.DrawWireSphere(camPos, 0.5f);
                if (i < _zones.Length - 1)
                    Gizmos.DrawLine(z.lookAt, _zones[i + 1].lookAt);
            }
        }
    }
}

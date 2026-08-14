using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using OneWayTogether.Camera;
using OneWayTogether.Characters;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// V2 replacement for LevelSequenceController.
    ///
    /// After each reunion the camera dollies to the next puzzle zone, teleports
    /// characters to the zone's spawn point (mid-dolly, off-screen), activates the
    /// zone, then hands control back to the player.
    ///
    /// ZoneAnchor fields:
    ///   zoneRoot   — GO to activate (null for zone 0, active at load)
    ///   lookAt     — world position the camera frames
    ///   height     — camera height above lookAt
    ///   fov        — camera FOV
    ///   spawnCenter — where characters spawn; Scarlet offset −4 X, Dani offset +4 X
    ///                 Leave at (0,0,0) for zone 0 (they're already placed in scene).
    /// </summary>
    public class CityZoneSequencer : MonoBehaviour
    {
        [System.Serializable]
        public class ZoneAnchor
        {
            [Tooltip("Puzzle zone root GO. Null for zone 0 (active at start).")]
            public GameObject zoneRoot;

            [Tooltip("World position the camera looks at when framing this zone.")]
            public Vector3 lookAt;

            [Tooltip("Camera height above lookAt.")]
            public float height = 28f;

            [Tooltip("Camera FOV at this zone.")]
            public float fov = 52f;

            [Tooltip("Mid-dolly spawn position for both characters. " +
                     "Scarlet appears at spawnCenter + (-4,0,0), Dani at spawnCenter + (4,0,0). " +
                     "Leave (0,0,0) for zone 0 — characters are already in place.")]
            public Vector3 spawnCenter;
        }

        // ── Inspector ─────────────────────────────────────────────────────────────

        [Header("Zones (ordered — zone 0 is active at load)")]
        [SerializeField] private ZoneAnchor[] _zones;

        [Header("Transition")]
        [Tooltip("Seconds between reunion and camera starting to move.")]
        [SerializeField] private float _celebrationDelay = 1.2f;

        [Tooltip("Seconds for the camera to travel between zones.")]
        [SerializeField] private float _dollyDuration = 2.5f;

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
            StartCoroutine(TransitionSafetyNet());
        }

        /// <summary>
        /// Runs in parallel with AdvanceZone. If the transition coroutine errors
        /// mid-flight and never raises Playing, this forces it after a deadline.
        /// </summary>
        private IEnumerator TransitionSafetyNet()
        {
            float deadline = _celebrationDelay + _dollyDuration + 3f;
            yield return new WaitForSeconds(deadline);
            if (!_transitioning) yield break; // normal path completed first — nothing to do

            Debug.LogWarning("[CityZoneSequencer] Safety net triggered — forcing GameState.Playing.");
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            _transitioning = false;
        }

        // ── Zone transition ───────────────────────────────────────────────────────

        private IEnumerator AdvanceZone()
        {
            bool isLastZone = _currentZone >= _zones.Length - 1;

            // Wait one frame so GameManager can process OnReunionAchieved first.
            yield return null;

            if (!isLastZone)
                GameEvents.RaiseGameStateChanged(GameState.ZoneTransition);

            yield return new WaitForSeconds(_celebrationDelay);

            if (isLastZone)
            {
                SceneManager.LoadScene(_mainMenuScene);
                yield break;
            }

            _currentZone++;
            var next = _zones[_currentZone];

            // ── Begin dolly ───────────────────────────────────────────────────────

            var follow = _mainCamera != null
                ? _mainCamera.GetComponent<IsometricCameraFollow>()
                : null;
            if (follow) follow.enabled = false;

            if (_mainCamera == null)
            {
                Debug.LogError("[CityZoneSequencer] _mainCamera is null — skipping dolly.");
                if (next.zoneRoot != null) next.zoneRoot.SetActive(true);
                if (follow) follow.enabled = true;
                if (next.spawnCenter != Vector3.zero) SpawnCharacters(next.spawnCenter);
                GameEvents.RaiseGameStateChanged(GameState.Playing);
                _transitioning = false;
                yield break;
            }

            Vector3    startPos = _mainCamera.transform.position;
            Quaternion startRot = _mainCamera.transform.rotation;
            float      startFOV = _mainCamera.fieldOfView;

            Vector3    endPos = next.lookAt + new Vector3(0f, next.height, -next.height * 0.85f);
            Quaternion endRot = Quaternion.LookRotation((next.lookAt - endPos).normalized);
            float      endFOV = next.fov;

            float t = 0f;
            bool  spawned = false;

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

                // Teleport characters at the halfway point (camera between both zones).
                if (!spawned && p >= 0.5f && next.spawnCenter != Vector3.zero)
                {
                    spawned = true;
                    SpawnCharacters(next.spawnCenter);
                }

                yield return null;
            }

            // Snap final.
            if (_mainCamera)
            {
                _mainCamera.transform.position = endPos;
                _mainCamera.transform.rotation = endRot;
                _mainCamera.fieldOfView        = endFOV;
            }

            // Ensure teleport happened even if dolly was very short.
            if (!spawned && next.spawnCenter != Vector3.zero)
                SpawnCharacters(next.spawnCenter);

            // ── Activate zone and resume ──────────────────────────────────────────

            if (next.zoneRoot != null)
                next.zoneRoot.SetActive(true);

            if (follow) follow.enabled = true;

            GameEvents.RaiseGameStateChanged(GameState.Playing);
            _transitioning = false;
        }

        // ── Character teleport ────────────────────────────────────────────────────

        private static void SpawnCharacters(Vector3 center)
        {
            Vector3 scarletPos = center + new Vector3(-4f, 0f, 0f);
            Vector3 daniPos    = center + new Vector3( 4f, 0f, 0f);

            TeleportTagged("Scarlet", scarletPos);
            TeleportTagged("Dani",    daniPos);

            // Update checkpoints so failure respawns here, not at the previous zone.
            var cm = FindAnyObjectByType<CheckpointManager>();
            if (cm != null)
            {
                cm.RegisterCheckpoint(CharacterType.Scarlet, scarletPos);
                cm.RegisterCheckpoint(CharacterType.Dani,    daniPos);
            }
        }

        private static void TeleportTagged(string tag, Vector3 pos)
        {
            var go = GameObject.FindWithTag(tag);
            if (go == null || !go.activeInHierarchy) return;

            var cc = go.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                go.transform.position = pos;
                cc.enabled = true;
            }
            else
            {
                go.transform.position = pos;
            }

            // Zero any accumulated vertical velocity.
            var cb = go.GetComponent<CharacterBase>();
            if (cb != null)
            {
                var fi = typeof(CharacterBase).GetField("_verticalVelocity",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (fi != null) fi.SetValue(cb, 0f);
            }
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
                if (z.spawnCenter != Vector3.zero)
                {
                    Gizmos.color = new Color(1f, 0.8f, 0f, 0.9f);
                    Gizmos.DrawWireSphere(z.spawnCenter, 1f);
                }
                if (i < _zones.Length - 1)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
                    Gizmos.DrawLine(z.lookAt, _zones[i + 1].lookAt);
                }
            }
        }
    }
}

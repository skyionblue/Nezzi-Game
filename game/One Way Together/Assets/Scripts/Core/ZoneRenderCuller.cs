using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Disables CC demo-city root groups that are more than _renderRadius metres
    /// from the active puzzle zone centre. Called once per zone transition —
    /// no per-frame cost. Re-enables groups when a closer zone becomes active.
    ///
    /// On a 680m × 540m city with a 250m render radius, typically 60-80 % of
    /// city objects are culled at any given time, cutting draw calls significantly.
    /// </summary>
    public class ZoneRenderCuller : MonoBehaviour
    {
        [Tooltip("City root GameObjects to manage. These are the top-level groups " +
                 "in the CC demo scene hierarchy.")]
        [SerializeField] private GameObject[] _cityRoots;

        [Tooltip("Groups within this world-space radius of the active zone centre " +
                 "stay enabled. Groups beyond it are disabled.")]
        [SerializeField] private float _renderRadius = 280f;

        private CityZoneSequencer _sequencer;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _sequencer = GetComponent<CityZoneSequencer>();
            if (_sequencer == null)
                _sequencer = FindAnyObjectByType<CityZoneSequencer>();
        }

        private void OnEnable()  => GameEvents.OnGameStateChanged += HandleStateChanged;
        private void OnDisable() => GameEvents.OnGameStateChanged -= HandleStateChanged;

        // ── Event handler ─────────────────────────────────────────────────────────

        private void HandleStateChanged(GameState state)
        {
            // Cull whenever gameplay resumes (fires at start and after every
            // zone transition when CityZoneSequencer raises Playing again).
            if (state == GameState.Playing)
                CullToActiveZone();
        }

        // ── Culling logic ─────────────────────────────────────────────────────────

        private void CullToActiveZone()
        {
            if (_sequencer == null || _cityRoots == null) return;

            Vector3 centre = GetActiveZoneCentre();

            foreach (var root in _cityRoots)
            {
                if (root == null) continue;

                // Use the group root's world position as its representative location.
                float dist = Vector3.Distance(root.transform.position, centre);
                bool  shouldBeActive = dist <= _renderRadius;

                if (root.activeSelf != shouldBeActive)
                    root.SetActive(shouldBeActive);
            }
        }

        private Vector3 GetActiveZoneCentre()
        {
            // Read _currentZone and _zones from the sequencer via reflection
            // (fields are private to avoid cluttering the public API).
            var bf = System.Reflection.BindingFlags.NonPublic |
                     System.Reflection.BindingFlags.Instance;
            var seqType  = typeof(CityZoneSequencer);
            var zones    = seqType.GetField("_zones",       bf)?.GetValue(_sequencer) as System.Array;
            var current  = seqType.GetField("_currentZone", bf)?.GetValue(_sequencer);
            if (zones == null || current == null) return transform.position;

            int idx = (int)current;
            if (idx < 0 || idx >= zones.Length) return transform.position;

            var anchor   = zones.GetValue(idx);
            var zoneType = seqType.GetNestedType("ZoneAnchor");
            var lookAt   = (Vector3)(zoneType?.GetField("lookAt")?.GetValue(anchor) ?? Vector3.zero);
            return lookAt; // lookAt is the camera focus — good proxy for zone centre
        }
    }
}

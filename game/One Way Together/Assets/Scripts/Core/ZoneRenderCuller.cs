using System.Collections.Generic;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Disables CC demo-city root groups whose content centroid is more than
    /// _renderRadius metres from the active puzzle zone centre.
    ///
    /// Called once per zone transition (no per-frame cost). Calculates each
    /// group's content centroid from its direct children at Start() so the
    /// root pivot position (often at 0,0,0) is never used for distance checks.
    ///
    /// The intro camera flies from Y=648 — culling does NOT run until the first
    /// GameState.Playing fires at the end of the intro dolly, so the sky overview
    /// always sees the full city.
    /// </summary>
    public class ZoneRenderCuller : MonoBehaviour
    {
        [SerializeField] private GameObject[] _cityRoots;
        [SerializeField] private float        _renderRadius = 280f;

        private CityZoneSequencer             _sequencer;
        private readonly Dictionary<GameObject, Vector3> _centroids = new();
        private bool _initialised;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _sequencer = GetComponent<CityZoneSequencer>()
                      ?? FindAnyObjectByType<CityZoneSequencer>();
        }

        private void Start()
        {
            // Pre-compute each group's content centroid from its direct children.
            // Group root pivots are often at (0,0,0) — useless for distance checks.
            if (_cityRoots == null) return;
            foreach (var root in _cityRoots)
            {
                if (root == null) continue;
                var sum   = Vector3.zero;
                int count = 0;
                foreach (Transform child in root.transform)
                {
                    sum += child.position;
                    count++;
                }
                _centroids[root] = count > 0 ? sum / count : root.transform.position;
            }
            _initialised = true;
        }

        private void OnEnable()  => GameEvents.OnGameStateChanged += HandleStateChanged;
        private void OnDisable() => GameEvents.OnGameStateChanged -= HandleStateChanged;

        // ── Event handler ─────────────────────────────────────────────────────────

        private void HandleStateChanged(GameState state)
        {
            // Only cull after Start() has run AND centroids are computed.
            // The first Playing event fires at the END of the intro dolly,
            // at which point Start() has long since completed.
            if (state == GameState.Playing && _initialised)
                CullToActiveZone();
        }

        // ── Culling ───────────────────────────────────────────────────────────────

        private void CullToActiveZone()
        {
            if (_sequencer == null || _cityRoots == null) return;

            Vector3 zoneCentre = GetActiveZoneCentre();

            foreach (var root in _cityRoots)
            {
                if (root == null) continue;
                if (!_centroids.TryGetValue(root, out Vector3 centroid))
                    centroid = root.transform.position;

                bool shouldBeActive = Vector3.Distance(centroid, zoneCentre) <= _renderRadius;
                if (root.activeSelf != shouldBeActive)
                    root.SetActive(shouldBeActive);
            }
        }

        private Vector3 GetActiveZoneCentre()
        {
            var bf      = System.Reflection.BindingFlags.NonPublic |
                          System.Reflection.BindingFlags.Instance;
            var seqType = typeof(CityZoneSequencer);
            var zones   = seqType.GetField("_zones",       bf)?.GetValue(_sequencer) as System.Array;
            var current = seqType.GetField("_currentZone", bf)?.GetValue(_sequencer);
            if (zones == null || current == null) return Vector3.zero;

            int idx = (int)current;
            if (idx < 0 || idx >= zones.Length) return Vector3.zero;

            var anchor   = zones.GetValue(idx);
            var zoneType = seqType.GetNestedType("ZoneAnchor");
            return (Vector3)(zoneType?.GetField("lookAt")?.GetValue(anchor) ?? Vector3.zero);
        }
    }
}

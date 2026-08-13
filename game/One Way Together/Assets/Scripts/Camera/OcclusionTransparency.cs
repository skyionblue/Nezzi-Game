using System.Collections.Generic;
using UnityEngine;

namespace OneWayTogether.Camera
{
    /// <summary>
    /// Attached to the Main Camera. Each LateUpdate it casts a ray from the camera
    /// to each character and makes any building/prop renderers in the way semi-transparent,
    /// restoring them when they no longer occlude.
    /// </summary>
    public class OcclusionTransparency : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform _scarlet;
        [SerializeField] private Transform _dani;

        [Header("Transparency")]
        [SerializeField, Range(0f, 1f)] private float _occludedAlpha = 0.18f;

        [Header("Occluder Groups (root GameObject names to scan)")]
        [SerializeField] private string[] _occluderRoots =
            { "Houses", "Skyscraper", "Institutioanal", "Props" };

        // ── State ──────────────────────────────────────────────────────────────────

        private readonly List<Renderer>                    _candidates       = new();
        private readonly HashSet<Renderer>                 _currentOccluders = new();
        private readonly HashSet<Renderer>                 _prevOccluders    = new();
        private readonly Dictionary<Renderer, Material[]> _savedMaterials   = new();
        private readonly Dictionary<Material, Material>   _transparentCache = new();

        // ── Unity lifecycle ────────────────────────────────────────────────────────

        private void Start()
        {
            foreach (var rootName in _occluderRoots)
            {
                var root = GameObject.Find(rootName);
                if (root == null) continue;
                _candidates.AddRange(root.GetComponentsInChildren<Renderer>(includeInactive: false));
            }
        }

        private void LateUpdate()
        {
            // Swap current → previous
            _prevOccluders.Clear();
            foreach (var r in _currentOccluders) _prevOccluders.Add(r);
            _currentOccluders.Clear();

            CheckTarget(_scarlet);
            CheckTarget(_dani);

            // Restore renderers that are no longer occluding
            foreach (var r in _prevOccluders)
            {
                if (_currentOccluders.Contains(r) || !_savedMaterials.ContainsKey(r)) continue;
                if (r != null) r.sharedMaterials = _savedMaterials[r];
                _savedMaterials.Remove(r);
            }

            // Apply transparency to newly occluding renderers
            foreach (var r in _currentOccluders)
            {
                if (_savedMaterials.ContainsKey(r)) continue;
                if (r == null) continue;

                _savedMaterials[r] = r.sharedMaterials;

                var transparent = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < transparent.Length; i++)
                    transparent[i] = GetTransparentClone(r.sharedMaterials[i]);
                r.materials = transparent; // creates per-renderer instances
            }
        }

        private void OnDestroy()
        {
            foreach (var kvp in _savedMaterials)
                if (kvp.Key != null) kvp.Key.sharedMaterials = kvp.Value;

            foreach (var mat in _transparentCache.Values)
                if (mat != null) Destroy(mat);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private void CheckTarget(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return;

            Vector3 camPos  = transform.position;
            Vector3 charPos = target.position + Vector3.up * 0.8f; // aim at torso
            Vector3 dir     = charPos - camPos;
            float   dist    = dir.magnitude;
            var     ray     = new Ray(camPos, dir.normalized);

            foreach (var r in _candidates)
            {
                if (r == null) continue;
                if (r.bounds.IntersectRay(ray, out float d) && d > 0.5f && d < dist)
                    _currentOccluders.Add(r);
            }
        }

        private Material GetTransparentClone(Material src)
        {
            if (src == null) return null;
            if (_transparentCache.TryGetValue(src, out var cached)) return cached;

            var clone = new Material(src) { name = src.name + "_occluded" };
            clone.SetFloat("_Surface", 1f);         // Transparent surface type
            clone.SetFloat("_Blend",   0f);
            clone.SetFloat("_ZWrite",  0f);
            clone.SetFloat("_SrcBlend", 5f);         // SrcAlpha
            clone.SetFloat("_DstBlend", 10f);        // OneMinusSrcAlpha
            clone.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            clone.renderQueue = 3000;

            var c = clone.HasProperty("_BaseColor")
                ? clone.GetColor("_BaseColor")
                : Color.white;
            c.a = _occludedAlpha;
            clone.SetColor("_BaseColor", c);

            _transparentCache[src] = clone;
            return clone;
        }
    }
}

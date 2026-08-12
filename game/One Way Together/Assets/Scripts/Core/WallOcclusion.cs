using System.Collections.Generic;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Fades walls (and any occluder tagged "Wall") that sit between the camera
    /// and either character. Attach to the Main Camera.
    ///
    /// Each frame raycasts from the camera toward each character. Any Renderer
    /// hit on the "Wall" or "Ground" layer gets a cached transparent material
    /// swapped in; renderers that leave the line of sight are restored.
    /// </summary>
    public class WallOcclusion : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Both siblings — drag Scarlet and Dani here.")]
        [SerializeField] private Transform[] _characters;

        [Header("Fade")]
        [Tooltip("Alpha applied to occluding renderers.")]
        [SerializeField, Range(0f, 1f)] private float _fadeAlpha = 0.25f;

        [Tooltip("Layers whose renderers can be faded. Match whatever layer your walls are on.")]
        [SerializeField] private LayerMask _occludeLayers;

        // Renderers currently made transparent, mapped to their original materials.
        private readonly Dictionary<Renderer, Material[]> _faded = new();
        // Renderers hit this frame — used to detect ones that should be restored.
        private readonly HashSet<Renderer> _hitThisFrame = new();

        private static readonly int SrcBlend    = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend    = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWrite      = Shader.PropertyToID("_ZWrite");
        private static readonly int Surface     = Shader.PropertyToID("_Surface");
        private static readonly int BaseColor   = Shader.PropertyToID("_BaseColor");

        private void LateUpdate()
        {
            _hitThisFrame.Clear();

            foreach (Transform target in _characters)
            {
                if (target == null) continue;
                CastAndFade(target.position + Vector3.up * 0.5f);
            }

            // Restore renderers that were faded last frame but not hit this frame.
            var toRestore = new List<Renderer>();
            foreach (var kv in _faded)
            {
                if (!_hitThisFrame.Contains(kv.Key))
                    toRestore.Add(kv.Key);
            }
            foreach (Renderer r in toRestore)
                Restore(r);
        }

        private void OnDisable()
        {
            // Restore everything when the script is disabled.
            foreach (var r in new List<Renderer>(_faded.Keys))
                Restore(r);
        }

        private void CastAndFade(Vector3 targetPos)
        {
            Vector3 camPos = transform.position;
            Vector3 dir    = targetPos - camPos;
            float   dist   = dir.magnitude;

            RaycastHit[] hits = Physics.RaycastAll(camPos, dir.normalized, dist, _occludeLayers);
            foreach (RaycastHit hit in hits)
            {
                Renderer r = hit.collider.GetComponent<Renderer>();
                if (r == null) r = hit.collider.GetComponentInParent<Renderer>();
                if (r == null) continue;

                _hitThisFrame.Add(r);
                if (!_faded.ContainsKey(r))
                    Fade(r);
            }
        }

        private void Fade(Renderer r)
        {
            _faded[r] = r.sharedMaterials;

            Material[] fadedMats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                Material src = r.sharedMaterials[i];
                Material m = new Material(src);

                // Switch URP Lit to transparent mode.
                m.SetFloat(Surface, 1f);         // Surface = Transparent
                m.SetFloat(SrcBlend, 5f);        // SrcAlpha
                m.SetFloat(DstBlend, 10f);       // OneMinusSrcAlpha
                m.SetFloat(ZWrite, 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;

                Color c = m.HasProperty(BaseColor) ? m.GetColor(BaseColor) : Color.white;
                c.a = _fadeAlpha;
                m.SetColor(BaseColor, c);

                fadedMats[i] = m;
            }

            r.materials = fadedMats;
        }

        private void Restore(Renderer r)
        {
            if (!_faded.TryGetValue(r, out Material[] originals)) return;
            if (r != null) r.sharedMaterials = originals;
            _faded.Remove(r);
        }
    }
}

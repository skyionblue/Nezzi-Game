using System.Collections;
using UnityEngine;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Flashes the full surface of an invisible boundary wall red when a character
    /// hits it, so players know a wall is there.
    ///
    /// Attach to any P1_Wall GameObject that has a BoxCollider.
    /// A wall-sized quad is built at runtime; the material is a URP Unlit
    /// transparent instance whose color is driven directly (no MaterialPropertyBlock).
    /// Called from CharacterBase.OnControllerColliderHit.
    /// </summary>
    public class WallFlash : MonoBehaviour
    {
        [SerializeField] private float _flashDuration = 0.5f;
        [SerializeField] private float _peakAlpha     = 0.5f;

        private MeshRenderer _quad;
        private Material     _mat;        // per-instance, never shared
        private bool         _flashing;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildWallQuad();
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public void Flash()
        {
            if (_flashing || _quad == null) return;
            StopAllCoroutines();
            StartCoroutine(DoFlash());
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private IEnumerator DoFlash()
        {
            _flashing = true;
            SetAlpha(_peakAlpha);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _flashDuration;
                SetAlpha(Mathf.Lerp(_peakAlpha, 0f, t));
                yield return null;
            }

            SetAlpha(0f);
            _flashing = false;
        }

        private void SetAlpha(float a)
        {
            if (_quad == null || _mat == null) return;
            _quad.enabled = a > 0.005f;
            _mat.color    = new Color(1f, 0.05f, 0.05f, a);
        }

        // ── Quad builder ─────────────────────────────────────────────────────────

        private void BuildWallQuad()
        {
            var bc = GetComponent<BoxCollider>();
            if (bc == null) return;

            // Determine wall face orientation from the thin axis of the BoxCollider.
            float sx = bc.size.x, sy = bc.size.y, sz = bc.size.z;
            bool  thinInX = sx <= sz;   // East/West walls are thin in X

            var go = new GameObject("WallFlashQuad") { layer = gameObject.layer };
            go.transform.SetParent(transform, false);

            // Orient and scale the quad to fill the wall face exactly.
            if (thinInX)
            {
                // Wall runs north-south (Z). Face the interior (X direction).
                go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                go.transform.localScale    = new Vector3(sz, sy, 1f);
                // Nudge slightly toward puzzle interior.
                float nudge = transform.position.x < -100f ? 0.15f : -0.15f;
                go.transform.localPosition = new Vector3(nudge, 0f, 0f);
            }
            else
            {
                // Wall runs east-west (X). Face the interior (Z direction).
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale    = new Vector3(sx, sy, 1f);
                float nudge = transform.position.z < 291f ? 0.15f : -0.15f;
                go.transform.localPosition = new Vector3(0f, 0f, nudge);
            }

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = BuildQuadMesh();

            _quad = go.AddComponent<MeshRenderer>();
            _quad.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
            _quad.receiveShadows       = false;

            // Build a URP-compatible transparent unlit material.
            _mat         = CreateTransparentMat();
            _quad.material = _mat;   // assigns and stores per-renderer instance
            _mat           = _quad.material;  // re-capture the actual instance Unity created
            _mat.color     = new Color(1f, 0.05f, 0.05f, 0f);
            _quad.enabled  = false;
        }

        private static Mesh BuildQuadMesh()
        {
            var mesh = new Mesh { name = "WallFlashQuad" };
            mesh.vertices  = new Vector3[] {
                new(-0.5f, -0.5f, 0f),
                new( 0.5f, -0.5f, 0f),
                new(-0.5f,  0.5f, 0f),
                new( 0.5f,  0.5f, 0f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1,   // front
                                          0, 1, 2, 2, 1, 3 }; // back (double-sided)
            mesh.normals   = new Vector3[] {
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
            };
            mesh.uv = new Vector2[] {
                new(0,0), new(1,0), new(0,1), new(1,1)
            };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material CreateTransparentMat()
        {
            // Use URP Unlit shader — guaranteed present in any URP project.
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var mat = new Material(shader) { name = "M_WallFlash_instance" };

            // Configure for alpha transparency in URP.
            mat.SetFloat("_Surface",  1f);   // 1 = Transparent
            mat.SetFloat("_Blend",    0f);   // Alpha blend
            mat.SetFloat("_SrcBlend", 5f);   // SrcAlpha
            mat.SetFloat("_DstBlend", 10f);  // OneMinusSrcAlpha
            mat.SetFloat("_ZWrite",   0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            mat.color       = Color.clear;
            return mat;
        }
    }
}

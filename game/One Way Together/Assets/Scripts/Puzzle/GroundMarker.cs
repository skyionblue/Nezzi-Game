using System.Collections;
using UnityEngine;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Pulsing X marker painted on the ground to show players where to push a bin.
    /// Builds its own geometry in Awake — no prefab or art assets required.
    /// Turns off permanently when a Rigidbody enters its trigger zone (bin arrived).
    /// </summary>
    public class GroundMarker : MonoBehaviour
    {
        [SerializeField] private Color  _color       = new Color(1f, 0.85f, 0f, 1f); // gold
        [SerializeField] private float  _pulseSpeed  = 1.8f;
        [SerializeField] private float  _minAlpha    = 0.25f;
        [SerializeField] private float  _maxAlpha    = 0.95f;
        [SerializeField] private float  _radius      = 1.5f;   // marker visual radius
        [SerializeField] private float  _armWidth    = 0.35f;  // X arm thickness

        private Material _mat;
        private bool     _done;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildX();
            StartCoroutine(Pulse());

            // Trigger zone — detects when a Rigidbody bin lands here
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size      = new Vector3(_radius * 2f, 1.5f, _radius * 2f);
            bc.center    = new Vector3(0f, 0.75f, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_done || other.attachedRigidbody == null) return;
            _done = true;
            StopAllCoroutines();
            // Hold fully visible briefly then turn off
            StartCoroutine(FadeOut());
        }

        // ── Visual construction ───────────────────────────────────────────────────

        private void BuildX()
        {
            _mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                name = "GroundMarker_mat"
            };
            ConfigureTransparent(_mat, _color);

            // Two arms of the X — flat quads on the ground
            CreateArm(  45f);
            CreateArm(-45f);
        }

        private void CreateArm(float yRot)
        {
            var go = new GameObject("Arm");
            go.transform.SetParent(transform, false);
            go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            go.transform.localPosition = Vector3.zero;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = BuildQuadMesh(_radius * 2f, _armWidth);

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static Mesh BuildQuadMesh(float length, float width)
        {
            float hl = length * 0.5f, hw = width * 0.5f;
            var mesh = new Mesh { name = "XArm" };
            mesh.vertices  = new Vector3[] {
                new(-hl, 0.02f, -hw), new(hl, 0.02f, -hw),
                new(-hl, 0.02f,  hw), new(hl, 0.02f,  hw)
            };
            mesh.triangles = new int[] { 0,2,1, 2,3,1,  1,2,0, 1,3,2 }; // double-sided
            mesh.uv        = new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void ConfigureTransparent(Material mat, Color col)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_SrcBlend", 5f);
            mat.SetFloat("_DstBlend", 10f);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            mat.color = col;
        }

        // ── Animation ─────────────────────────────────────────────────────────────

        private IEnumerator Pulse()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * _pulseSpeed * Mathf.PI) + 1f) * 0.5f;
                float a = Mathf.Lerp(_minAlpha, _maxAlpha, t);
                _mat.color = new Color(_color.r, _color.g, _color.b, a);
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            float a = _mat.color.a;
            while (a > 0f)
            {
                a -= Time.deltaTime * 2f;
                _mat.color = new Color(_color.r, _color.g, _color.b, a);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}

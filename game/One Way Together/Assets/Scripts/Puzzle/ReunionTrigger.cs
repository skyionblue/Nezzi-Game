using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// The win condition for every level. The level completes only when BOTH
    /// Scarlet and Dani are simultaneously inside this trigger zone.
    ///
    /// Uses 3D trigger callbacks (OnTriggerEnter/Exit) because CharacterController
    /// automatically creates a 3D CapsuleCollider — 2D physics is not used in
    /// the HD-2D isometric build.
    ///
    /// A single character entering does not trigger completion — they must
    /// physically be together. This mechanically reinforces the game's emotional
    /// core: reuniting is the goal, not reaching an exit.
    ///
    /// Place one per level. The trigger should be clearly signposted in the scene
    /// (a warm glow, particle effect, or distinct tile) so players know the goal.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ReunionTrigger : MonoBehaviour
    {
        [Header("Feedback")]
        [Tooltip("Particle or animation object played when both characters reunite.")]
        [SerializeField] private GameObject _reunionFX;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _scarletInside;
        private bool _daniInside;
        private bool _hasTriggered;

        private static int _characterLayer = -1;

        // Cache the tags to avoid per-frame string comparison.
        private const string ScarletTag = "Scarlet";
        private const string DaniTag    = "Dani";

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            GetComponent<Collider>().isTrigger = true;
            CreateFloorMarker();
        }

        private void CreateFloorMarker()
        {
            // Flat disc on the floor so players can see the goal without a separate art asset.
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "GoalMarker";
            marker.transform.SetParent(transform);
            marker.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            marker.transform.localScale    = new Vector3(5f, 0.01f, 5f);

            // Remove the capsule collider — the parent trigger handles detection.
            Destroy(marker.GetComponent<Collider>());

            // Warm golden glow — clearly distinct from the environment.
            MeshRenderer mr = marker.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color             = new Color(1f, 0.85f, 0.1f, 1f);
            mat.SetFloat("_Metallic",  0f);
            mat.SetFloat("_Smoothness", 0.8f);
            mr.material = mat;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered) return;
            if (other.gameObject.layer != _characterLayer) return;

            if (other.CompareTag(ScarletTag)) _scarletInside = true;
            else if (other.CompareTag(DaniTag)) _daniInside = true;

            CheckReunion();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_hasTriggered) return;
            if (other.gameObject.layer != _characterLayer) return;

            if (other.CompareTag(ScarletTag)) _scarletInside = false;
            else if (other.CompareTag(DaniTag)) _daniInside = false;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void CheckReunion()
        {
            if (!_scarletInside || !_daniInside) return;

            _hasTriggered = true;

            if (_reunionFX != null)
                Instantiate(_reunionFX, transform.position, Quaternion.identity);

            GameEvents.RaiseReunionAchieved();
            Debug.Log("[ReunionTrigger] Both characters reunited — puzzle complete!");
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.5f);
            Collider col = GetComponent<Collider>();
            if (col != null)
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            else
                Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}

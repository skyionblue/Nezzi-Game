using System.Collections;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Flat walkable platform. Starts above the camera (retractOffset units above
    /// its authored world position) and smoothly lowers into the gap when the linked
    /// lever fires. Uses the same OnPressurePlateChanged bus as Gate/Lever.
    ///
    /// Init is called by LevelBuilder immediately after Instantiate.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Bridge : MonoBehaviour
    {
        [SerializeField] private string _plateId;

        [Tooltip("Distance ABOVE the authored position the bridge starts. Must be positive — high enough to be off-camera.")]
        [SerializeField] private float _retractOffset = 20f;

        [Tooltip("Speed in world units per second at which the bridge descends into place.")]
        [SerializeField, Range(1f, 30f)] private float _lowerSpeed = 12f;

        private Vector3 _deployedPos;
        private Vector3 _retractedPos;
        private bool    _initialised;
        private bool    _isDeployed;
        private Coroutine _moveCoroutine;

        // ── Public API (called by LevelBuilder) ──────────────────────────────────

        /// <summary>
        /// Binds the bridge to a plate ID and hides it above the scene.
        /// Must be called immediately after Instantiate, before any frame update.
        /// </summary>
        public void Init(string plateId)
        {
            _plateId      = plateId;
            _deployedPos  = transform.position;
            _retractedPos = _deployedPos + Vector3.up * _retractOffset;
            transform.position = _retractedPos;
            _initialised  = true;
            _isDeployed   = false;
        }

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable()
        {
            GameEvents.OnPressurePlateChanged -= HandlePlate;
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        }

        // ── Event handler ────────────────────────────────────────────────────────

        private void HandlePlate(string id, bool active)
        {
            if (id != _plateId || !_initialised) return;

            Vector3 target = active ? _deployedPos : _retractedPos;
            bool    deploy = active;

            if (deploy == _isDeployed) return; // already in the right state
            _isDeployed = deploy;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveTo(target));
        }

        // ── Movement coroutine ────────────────────────────────────────────────────

        private IEnumerator MoveTo(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, _lowerSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            _moveCoroutine = null;
        }
    }
}

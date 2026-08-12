using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Flat walkable platform. Starts <see cref="_retractOffset"/> units below its
    /// authored world position. Snaps to the authored position (deployed) when the
    /// linked pressure-plate ID fires active, and retracts when it fires inactive.
    ///
    /// Uses the same <see cref="GameEvents.OnPressurePlateChanged"/> bus as Gate/Lever
    /// so a single Lever can raise the bridge without any direct reference.
    ///
    /// Init is called by LevelBuilder after Instantiate, which sets _plateId and
    /// calculates deployed/retracted positions from the authored transform.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Bridge : MonoBehaviour
    {
        [SerializeField] private string _plateId;

        [Tooltip("Distance below the authored position the bridge sits when hidden.")]
        [SerializeField] private float _retractOffset = -10f;

        private Vector3 _deployedPos;
        private Vector3 _retractedPos;
        private bool _initialised;

        // ── Public API (called by LevelBuilder) ──────────────────────────────────

        /// <summary>
        /// Binds the bridge to a plate ID and hides it below the floor.
        /// Must be called immediately after Instantiate, before any frame update.
        /// </summary>
        public void Init(string plateId)
        {
            _plateId      = plateId;
            _deployedPos  = transform.position;
            _retractedPos = _deployedPos + Vector3.up * _retractOffset;
            transform.position = _retractedPos;
            _initialised = true;
        }

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable() => GameEvents.OnPressurePlateChanged -= HandlePlate;

        // ── Event handler ────────────────────────────────────────────────────────

        private void HandlePlate(string id, bool active)
        {
            if (id != _plateId) return;

            // Guard against an event firing before Init has run (edge case during
            // scene load ordering), though LevelBuilder always calls Init first.
            if (!_initialised) return;

            transform.position = active ? _deployedPos : _retractedPos;
        }
    }
}

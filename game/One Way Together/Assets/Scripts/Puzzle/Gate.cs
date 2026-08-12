using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A gate or door that responds to pressure plate and lever events.
    /// Configure <see cref="_triggerPlateId"/> to match the ID of the
    /// <see cref="PressurePlate"/> or lever that should control this gate.
    ///
    /// The gate moves between <see cref="_closedPosition"/> and
    /// <see cref="_openPosition"/> using a simple lerp.
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique ID for this gate (published by GameEvents.OnGateStateChanged).")]
        [SerializeField] private string _gateId = "Gate_01";

        [Header("Control")]
        [Tooltip("Plate ID to listen for. Must match the PressurePlate or Lever that controls this gate.")]
        [SerializeField] private string _triggerPlateId = "Plate_01";

        [Tooltip("When true the gate starts open and closes when the plate is active.")]
        [SerializeField] private bool _invertLogic = false;

        [Header("Movement")]
        [SerializeField] private Vector3 _openPosition  = new Vector3(0f, 2f, 0f);
        [SerializeField] private Vector3 _closedPosition = Vector3.zero;
        [SerializeField, Range(0.1f, 5f)] private float _moveSpeed = 2f;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isOpen;
        private Vector3 _targetPosition;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            _targetPosition = _closedPosition;
            transform.localPosition = _closedPosition;
        }

        private void OnEnable()
        {
            GameEvents.OnPressurePlateChanged += HandlePlateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPressurePlateChanged -= HandlePlateChanged;
        }

        private void Update()
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                _targetPosition,
                _moveSpeed * Time.deltaTime);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void HandlePlateChanged(string plateId, bool isActive)
        {
            if (plateId != _triggerPlateId) return;

            bool shouldOpen = _invertLogic ? !isActive : isActive;
            SetOpen(shouldOpen);
        }

        private void SetOpen(bool open)
        {
            if (_isOpen == open) return;
            _isOpen = open;
            _targetPosition = open ? _openPosition : _closedPosition;
            GameEvents.RaiseGateStateChanged(_gateId, _isOpen);
        }

        // ── Runtime configuration ─────────────────────────────────────────────────

        /// <summary>
        /// Configures the gate at runtime — call immediately after Instantiate, before
        /// OnEnable fires the subscription. Safe to call before Awake because Awake
        /// will not have run yet on a freshly instantiated object.
        ///
        /// <paramref name="gateId"/> is the unique ID published to GameEvents.
        /// <paramref name="plateId"/> must match the Lever or PressurePlate that controls this gate.
        /// <paramref name="openOffset"/> is the local-space position offset when open.
        /// </summary>
        public void Init(string gateId, string plateId, Vector3 openOffset)
        {
            _gateId         = gateId;
            _triggerPlateId = plateId;
            _openPosition   = openOffset;
        }

        // ── Gizmos ────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(transform.position + _openPosition, Vector3.one * 0.3f);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(transform.position + _closedPosition, Vector3.one * 0.3f);
        }
    }
}

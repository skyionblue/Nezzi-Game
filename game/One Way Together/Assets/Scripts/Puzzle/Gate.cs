using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A double-door gate that swings open in response to pressure-plate / lever
    /// events (matched by <see cref="_triggerPlateId"/>). Two leaf transforms
    /// rotate about their hinges between a closed (rest) pose and an open pose;
    /// an optional blocker collider is disabled while open so characters can pass.
    ///
    /// Replaces the old "raise the whole gate up" translation with a hinged swing.
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

        [Header("Doors")]
        [Tooltip("Left door leaf — its pivot must sit at the hinge (outer edge).")]
        [SerializeField] private Transform _leftLeaf;

        [Tooltip("Right door leaf — its pivot must sit at the hinge (outer edge).")]
        [SerializeField] private Transform _rightLeaf;

        [Tooltip("Degrees each leaf swings open from its closed (rest) pose. The left leaf uses +angle, the right uses -angle.")]
        [SerializeField] private float _openAngle = 90f;

        [Tooltip("Swing speed in degrees/second.")]
        [SerializeField, Range(30f, 720f)] private float _openSpeed = 160f;

        [Tooltip("Local axis each leaf rotates about — up (Y) for a vertical hinge.")]
        [SerializeField] private Vector3 _hingeAxis = Vector3.up;

        [Header("Blocking")]
        [Tooltip("Collider that blocks passage while closed and is disabled when open. Optional.")]
        [SerializeField] private Collider _blocker;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isOpen;
        // The mesh is modeled CLOSED, so the leaves' authored pose is the closed rest pose.
        private Quaternion _leftClosed = Quaternion.identity;
        private Quaternion _rightClosed = Quaternion.identity;
        private Quaternion _leftTarget = Quaternion.identity;
        private Quaternion _rightTarget = Quaternion.identity;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (_leftLeaf != null) _leftClosed = _leftLeaf.localRotation;
            if (_rightLeaf != null) _rightClosed = _rightLeaf.localRotation;

            _isOpen = false;
            UpdateTargets(instant: true);
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
            if (_leftLeaf != null && _leftLeaf.localRotation != _leftTarget)
                _leftLeaf.localRotation = Quaternion.RotateTowards(
                    _leftLeaf.localRotation, _leftTarget, _openSpeed * Time.deltaTime);

            if (_rightLeaf != null && _rightLeaf.localRotation != _rightTarget)
                _rightLeaf.localRotation = Quaternion.RotateTowards(
                    _rightLeaf.localRotation, _rightTarget, _openSpeed * Time.deltaTime);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void HandlePlateChanged(string plateId, bool isActive)
        {
            if (plateId != _triggerPlateId) return;
            SetOpen(_invertLogic ? !isActive : isActive);
        }

        private void SetOpen(bool open)
        {
            if (_isOpen == open) return;
            _isOpen = open;
            UpdateTargets(instant: false);
            GameEvents.RaiseGateStateChanged(_gateId, _isOpen);
        }

        private void UpdateTargets(bool instant)
        {
            // Closed = the authored rest pose; open = swung outward by _openAngle (mirrored per leaf).
            _leftTarget  = _isOpen ? _leftClosed  * Quaternion.AngleAxis( _openAngle, _hingeAxis) : _leftClosed;
            _rightTarget = _isOpen ? _rightClosed * Quaternion.AngleAxis(-_openAngle, _hingeAxis) : _rightClosed;

            if (instant)
            {
                if (_leftLeaf != null) _leftLeaf.localRotation = _leftTarget;
                if (_rightLeaf != null) _rightLeaf.localRotation = _rightTarget;
            }

            // Blocker is solid while closed, cleared while open.
            if (_blocker != null) _blocker.enabled = !_isOpen;
        }

        // ── Runtime configuration ─────────────────────────────────────────────────

        /// <summary>
        /// Configures the gate at runtime — call immediately after Instantiate.
        /// <paramref name="openOffset"/> is retained for API compatibility with
        /// <see cref="OneWayTogether.Core.LevelBuilder"/>; swing doors open
        /// rotationally and ignore it.
        /// </summary>
        public void Init(string gateId, string plateId, Vector3 openOffset)
        {
            _gateId         = gateId;
            _triggerPlateId = plateId;
        }
    }
}

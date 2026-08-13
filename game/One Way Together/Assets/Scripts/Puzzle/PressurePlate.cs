using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A pressure plate that activates while a character or heavy object stands on it.
    /// Broadcasts state changes through <see cref="GameEvents.OnPressurePlateChanged"/>
    /// so gates and other listeners can respond without direct references.
    ///
    /// Uses 3D trigger callbacks (OnTriggerEnter/Exit) because CharacterController
    /// automatically creates a 3D CapsuleCollider — 2D physics is not used in
    /// the HD-2D isometric build.
    ///
    /// A plate stays active as long as the activating collider is inside the trigger.
    /// When it exits, the plate deactivates unless <see cref="_lockable"/> is true
    /// and it has been locked by a lever/switch.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PressurePlate : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique ID broadcast with the state-change event. Gates listen for this ID.")]
        [SerializeField] private string _plateId = "Plate_01";

        [Header("Behaviour")]
        [Tooltip("When true, the plate can be permanently locked in the activated state by an external signal (e.g., a lever).")]
        [SerializeField] private bool _lockable = false;

        [Header("Visuals")]
        [Tooltip("Renderer whose material colour changes to show activation state.")]
        [SerializeField] private Renderer _indicator;
        [SerializeField] private Color _activeColor   = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color _inactiveColor = new Color(0.6f, 0.6f, 0.6f);

        // ── State ─────────────────────────────────────────────────────────────────

        private int _overlappingObjects;
        private bool _isActive;
        private bool _isLocked;

        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>True when the plate is currently depressed.</summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Sets the plate ID at spawn time. Call this immediately after Instantiate,
        /// before the first physics frame, so gates that listen for this ID are bound
        /// to the correct plate.
        /// </summary>
        public void Init(string plateId)
        {
            _plateId = plateId;
        }

        /// <summary>
        /// Permanently activates the plate regardless of whether anything is standing on it.
        /// Called by levers or external puzzle logic.
        /// </summary>
        public void Lock()
        {
            if (!_lockable) return;
            _isLocked = true;
            SetActive(true);
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            RefreshVisual();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isLocked) return;
            _overlappingObjects++;
            if (_overlappingObjects == 1) SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isLocked) return;
            _overlappingObjects = Mathf.Max(0, _overlappingObjects - 1);
            if (_overlappingObjects == 0) SetActive(false);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void SetActive(bool active)
        {
            if (_isActive == active) return;
            _isActive = active;
            RefreshVisual();
            GameEvents.RaisePressurePlateChanged(_plateId, _isActive);
        }

        private void RefreshVisual()
        {
            if (_indicator == null) return;
            // Use MaterialPropertyBlock to avoid creating material instances.
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _indicator.GetPropertyBlock(block);
            block.SetColor(ColorId, _isActive ? _activeColor : _inactiveColor);
            _indicator.SetPropertyBlock(block);
        }
    }
}

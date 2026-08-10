using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A pressure plate that activates while a character or heavy object stands on it.
    /// Broadcasts state changes through <see cref="GameEvents.OnPressurePlateChanged"/>
    /// so gates and other listeners can respond without direct references.
    ///
    /// A plate stays active as long as the activating collider is inside the trigger.
    /// When it exits, the plate deactivates unless <see cref="_lockable"/> is true
    /// and it has been locked by a lever/switch.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
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
        [SerializeField] private SpriteRenderer _indicator;
        [SerializeField] private Color _activeColor   = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color _inactiveColor = new Color(0.6f, 0.6f, 0.6f);

        // ── State ─────────────────────────────────────────────────────────────────

        private int _overlappingObjects;
        private bool _isActive;
        private bool _isLocked;

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>True when the plate is currently depressed.</summary>
        public bool IsActive => _isActive;

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
            GetComponent<Collider2D>().isTrigger = true;
            RefreshVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isLocked) return;
            _overlappingObjects++;
            if (_overlappingObjects == 1) SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
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
            _indicator.color = _isActive ? _activeColor : _inactiveColor;
        }
    }
}

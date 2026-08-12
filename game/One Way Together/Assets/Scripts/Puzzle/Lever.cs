using UnityEngine;
using OneWayTogether.Characters;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A lever or switch activated by Dani via the <see cref="IInteractable"/> interface.
    /// When pulled, fires <see cref="GameEvents.OnPressurePlateChanged"/> using the
    /// configured plate ID — gates listen to the same event regardless of whether it
    /// came from a pressure plate or a lever.
    ///
    /// Uses a 3D Collider trigger for the proximity zone because CharacterController
    /// creates 3D CapsuleColliders — 2D physics is not used in the HD-2D isometric build.
    ///
    /// Levers are toggle-based by default; set <see cref="_oneShot"/> to make them
    /// single-use (e.g., a mechanism that fires only once and locks the bridge).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Lever : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        [Tooltip("Must match the Gate's TriggerPlateId to connect them.")]
        [SerializeField] private string _plateId = "Plate_01";

        [Header("Behaviour")]
        [Tooltip("When true, the lever can only be used once — subsequent Interact calls do nothing.")]
        [SerializeField] private bool _oneShot = false;

        [Tooltip("When true, the lever starts in the activated state.")]
        [SerializeField] private bool _startsActivated = false;

        [Header("Handle Animation")]
        [Tooltip("The handle transform that swings when the lever is thrown. Its pivot must sit at the hub.")]
        [SerializeField] private Transform _handle;

        [Tooltip("Local euler rotation added to the handle's rest pose when the lever is activated (thrown).")]
        [SerializeField] private Vector3 _thrownEuler = new Vector3(-60f, 0f, 0f);

        [Tooltip("How fast the handle swings between rest and thrown, in degrees/second.")]
        [SerializeField, Range(30f, 720f)] private float _rotateSpeed = 300f;

        [Header("Visuals (optional colour tint)")]
        [SerializeField] private Renderer _leverRenderer;
        [SerializeField] private Color _inactiveColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _activeColor   = new Color(0.2f, 1f, 0.2f);

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isActivated;
        private bool _hasBeenUsed;

        // Handle rest pose (captured at Awake) and the pose it is currently swinging toward.
        private Quaternion _restRot = Quaternion.identity;
        private Quaternion _targetRot = Quaternion.identity;

        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _isActivated = _startsActivated;

            if (_handle != null) _restRot = _handle.localRotation;

            RefreshVisual();
            UpdateHandleTarget(instant: true);
        }

        private void Update()
        {
            if (_handle == null) return;
            if (_handle.localRotation != _targetRot)
                _handle.localRotation = Quaternion.RotateTowards(
                    _handle.localRotation, _targetRot, _rotateSpeed * Time.deltaTime);
        }

        // ── IInteractable ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called by DaniController when she interacts with this lever.
        /// Only Dani can pull levers (too small/light for Scarlet is enforced
        /// at design level — the switch layer mask on DaniController excludes
        /// Scarlet's interact range).
        /// </summary>
        public void Interact(CharacterBase source)
        {
            if (_oneShot && _hasBeenUsed) return;

            _isActivated = !_isActivated;
            _hasBeenUsed = true;

            RefreshVisual();
            UpdateHandleTarget();
            GameEvents.RaisePressurePlateChanged(_plateId, _isActivated);
        }

        // ── Runtime configuration ─────────────────────────────────────────────────

        /// <summary>
        /// Configures the lever at runtime — call immediately after Instantiate.
        /// <paramref name="plateId"/> must match the <see cref="Gate._triggerPlateId"/> on
        /// the gate this lever should control.
        /// </summary>
        public void Init(string plateId, bool oneShot = false)
        {
            _plateId = plateId;
            _oneShot = oneShot;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        /// <summary>Points the handle at its rest or thrown pose based on activation state.</summary>
        private void UpdateHandleTarget(bool instant = false)
        {
            if (_handle == null) return;
            _targetRot = _isActivated ? _restRot * Quaternion.Euler(_thrownEuler) : _restRot;
            if (instant) _handle.localRotation = _targetRot;
        }

        private void RefreshVisual()
        {
            if (_leverRenderer == null) return;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _leverRenderer.GetPropertyBlock(block);
            block.SetColor(ColorId, _isActivated ? _activeColor : _inactiveColor);
            _leverRenderer.SetPropertyBlock(block);
        }
    }
}

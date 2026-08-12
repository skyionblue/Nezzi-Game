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

        [Header("Visuals")]
        [SerializeField] private Renderer _leverRenderer;
        [SerializeField] private Color _inactiveColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _activeColor   = new Color(0.2f, 1f, 0.2f);

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isActivated;
        private bool _hasBeenUsed;

        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _isActivated = _startsActivated;
            RefreshVisual();
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

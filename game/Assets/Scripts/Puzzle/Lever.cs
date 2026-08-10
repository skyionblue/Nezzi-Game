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
    /// Levers are toggle-based by default; set <see cref="_oneShot"/> to make them
    /// single-use (e.g., a mechanism that fires only once and locks the bridge).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
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
        [SerializeField] private Sprite _inactiveSprite;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private SpriteRenderer _leverRenderer;

        // ── State ─────────────────────────────────────────────────────────────────

        private bool _isActivated;
        private bool _hasBeenUsed;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
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

        // ── Private ───────────────────────────────────────────────────────────────

        private void RefreshVisual()
        {
            if (_leverRenderer == null) return;
            _leverRenderer.sprite = _isActivated ? _activeSprite : _inactiveSprite;
        }
    }
}

using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// A trap/hazard trigger volume — one of the design-sanctioned failure causes
    /// ("fall, trap, stuck"). When a character enters, it raises
    /// <see cref="GameEvents.RaiseCharacterFailed"/>, which the
    /// <see cref="OneWayTogether.Core.GameManager"/> turns into
    /// <see cref="GameState.Failure"/>. The FailureUI then offers the player a
    /// checkpoint reset (free) or a respawn-in-place (spend coins).
    ///
    /// Uses a 3D trigger collider because characters move on <see cref="CharacterController"/>
    /// capsules — 2D physics is not used in the HD-2D isometric build.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hazard : MonoBehaviour
    {
        [Tooltip("When false, the hazard is inert (e.g. disarmed by a puzzle mechanism).")]
        [SerializeField] private bool _armed = true;

        // Cached once — the layer characters live on. Registered in TagManager as "Character".
        private static int _characterLayer = -1;

        public bool Armed { get => _armed; set => _armed = value; }

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_armed) return;
            if (other.gameObject.layer != _characterLayer) return;

            // Identify the sibling by tag so the failure event reports who was lost.
            CharacterType who = other.CompareTag("Dani") ? CharacterType.Dani : CharacterType.Scarlet;
            GameEvents.RaiseCharacterFailed(who);
        }
    }
}

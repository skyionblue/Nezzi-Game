using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Collectibles
{
    /// <summary>
    /// Placed on secret key prefabs hidden throughout each level.
    /// When either character overlaps the trigger, the key is collected,
    /// a <see cref="GameEvents.OnSecretKeyCollected"/> event is raised,
    /// and the object is disabled so it cannot be collected twice in one session.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SecretKeyPickup : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this key. Used to unlock specific lore doors.")]
        [SerializeField] private string _keyId = "Key_World1_Level1_A";

        [Header("Feedback")]
        [SerializeField] private GameObject _collectFX;
        [SerializeField] private AudioClip _collectSound;

        private static int _characterLayer = -1;

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != _characterLayer) return;

            if (_collectFX != null)
                Instantiate(_collectFX, transform.position, Quaternion.identity);

            if (_collectSound != null)
                AudioSource.PlayClipAtPoint(_collectSound, transform.position);

            GameEvents.RaiseSecretKeyCollected(_keyId);
            gameObject.SetActive(false);
        }
    }
}

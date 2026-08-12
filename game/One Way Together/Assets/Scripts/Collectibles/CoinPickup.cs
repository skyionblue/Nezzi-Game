using UnityEngine;

namespace OneWayTogether.Collectibles
{
    /// <summary>
    /// Placed on coin prefabs in the scene. Detects character overlap and notifies
    /// <see cref="CoinManager"/> then disables itself. The CoinManager owns the
    /// economy; this component only signals the pickup event.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CoinPickup : MonoBehaviour
    {
        [Header("Feedback")]
        [Tooltip("Optional particle effect spawned when the coin is collected.")]
        [SerializeField] private GameObject _collectFX;

        [Tooltip("Optional audio clip played on collection. Routed through CoinManager's AudioSource.")]
        [SerializeField] private AudioClip _collectSound;

        // Layer mask checked in Awake so character layer changes don't require script edits.
        private static int _characterLayer = -1;

        private void Awake()
        {
            if (_characterLayer == -1)
                _characterLayer = LayerMask.NameToLayer("Character");

            // Ensure the collider is a trigger.
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != _characterLayer) return;

            CoinManager coinManager = CoinManager.Instance;
            if (coinManager == null)
            {
                Debug.LogWarning("[CoinPickup] No CoinManager found in scene.", this);
                return;
            }

            coinManager.CollectCoin(_collectSound);
            SpawnFX();
            gameObject.SetActive(false);
        }

        private void SpawnFX()
        {
            if (_collectFX == null) return;
            Instantiate(_collectFX, transform.position, Quaternion.identity);
        }
    }
}

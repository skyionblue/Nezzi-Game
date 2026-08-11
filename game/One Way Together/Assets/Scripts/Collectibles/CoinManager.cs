using UnityEngine;
using OneWayTogether.Core;
using OneWayTogether.Data;
using OneWayTogether.Events;

namespace OneWayTogether.Collectibles
{
    /// <summary>
    /// Tracks the player's coin count for the current level session. Handles:
    ///
    /// - Recording coins collected by <see cref="CoinPickup"/> triggers.
    /// - Spending coins for respawn-in-place or hint tiers.
    /// - Persisting coin count across checkpoint resets within the same level.
    ///
    /// Coin count does NOT persist across level loads (session-scoped).
    /// Save/load for total lifetime coins is handled by a future SaveSystem.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CoinManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────────

        public static CoinManager Instance { get; private set; }

        // ── Serialised ────────────────────────────────────────────────────────────

        [Header("Configuration")]
        [SerializeField] private CoinSystemData _data;

        // ── Cached ────────────────────────────────────────────────────────────────

        private AudioSource _audioSource;
        private CheckpointManager _checkpointManager;

        // ── State ─────────────────────────────────────────────────────────────────

        /// <summary>Number of coins the player currently holds.</summary>
        public int CoinCount { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            _checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds one coin to the player's total. Called by <see cref="CoinPickup"/>.
        /// </summary>
        /// <param name="pickupSound">Optional sound clip for this specific coin.</param>
        public void CollectCoin(AudioClip pickupSound = null)
        {
            CoinCount++;

            if (pickupSound != null)
                _audioSource.PlayOneShot(pickupSound);

            GameEvents.RaiseCoinCollected(CoinCount);
        }

        /// <summary>
        /// Attempts to spend coins to respawn both characters in place.
        /// Returns true if there were enough coins and the spend succeeded.
        /// </summary>
        public bool TryRespawnInPlace()
        {
            if (_data == null)
            {
                Debug.LogError("[CoinManager] CoinSystemData not assigned.", this);
                return false;
            }

            if (CoinCount < _data.RespawnCost) return false;

            SpendCoins(_data.RespawnCost);
            _checkpointManager?.RespawnInPlace();
            return true;
        }

        /// <summary>
        /// Attempts to purchase the specified hint tier.
        /// Returns the hint tier purchased (1, 2, or 3), or 0 if insufficient coins.
        /// </summary>
        public int TryPurchaseHint()
        {
            if (_data == null) return 0;

            // Purchase the best hint the player can currently afford.
            if (CoinCount >= _data.Hint3Cost)
            {
                SpendCoins(_data.Hint3Cost);
                return 3;
            }
            if (CoinCount >= _data.Hint2Cost)
            {
                SpendCoins(_data.Hint2Cost);
                return 2;
            }
            if (CoinCount >= _data.Hint1Cost)
            {
                SpendCoins(_data.Hint1Cost);
                return 1;
            }

            return 0;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void SpendCoins(int amount)
        {
            CoinCount = Mathf.Max(0, CoinCount - amount);
            GameEvents.RaiseCoinsSpent(CoinCount);
        }
    }
}

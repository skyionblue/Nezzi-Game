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

        /// <summary>Coin cost to respawn in place, read from CoinSystemData (0 if unassigned).</summary>
        public int RespawnCost => _data != null ? _data.RespawnCost : 0;

        /// <summary>True when the player holds enough coins to pay for a respawn-in-place.</summary>
        public bool CanAffordRespawn => _data != null && CoinCount >= _data.RespawnCost;

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
            _checkpointManager = FindAnyObjectByType<CheckpointManager>();
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

        /// <summary>Coin cost of a specific hint tier (1-3). Returns 0 for an invalid tier or missing data.</summary>
        public int HintCost(int tier)
        {
            if (_data == null) return 0;
            return tier switch
            {
                1 => _data.Hint1Cost,
                2 => _data.Hint2Cost,
                3 => _data.Hint3Cost,
                _ => 0,
            };
        }

        /// <summary>True when the player can afford the given hint tier.</summary>
        public bool CanAffordHint(int tier)
        {
            int cost = HintCost(tier);
            return cost > 0 && CoinCount >= cost;
        }

        /// <summary>
        /// Attempts to purchase a specific hint tier (hints are revealed progressively,
        /// so the caller — <see cref="OneWayTogether.Core.HintManager"/> — asks for the
        /// next tier in sequence). Returns true if the coins were spent.
        /// </summary>
        public bool TryPurchaseHint(int tier)
        {
            int cost = HintCost(tier);
            if (cost <= 0 || CoinCount < cost) return false;

            SpendCoins(cost);
            return true;
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void SpendCoins(int amount)
        {
            CoinCount = Mathf.Max(0, CoinCount - amount);
            GameEvents.RaiseCoinsSpent(CoinCount);
        }
    }
}

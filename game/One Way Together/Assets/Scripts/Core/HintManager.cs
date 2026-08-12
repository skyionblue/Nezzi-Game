using UnityEngine;
using OneWayTogether.Collectibles;
using OneWayTogether.Data;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Dispenses the current level's hints progressively. The player spends coins
    /// for each successive hint (tier 1 = vague nudge, tier 2 = specific, tier 3 =
    /// full solution), with each tier costing more — the second coin sink alongside
    /// respawn-in-place.
    ///
    /// Hint text is authored in the level's <see cref="LevelData.hints"/> list so
    /// hints stay data-driven, like the rest of the level. Coin costs live in
    /// <see cref="OneWayTogether.Data.CoinSystemData"/> via <see cref="CoinManager"/>.
    /// </summary>
    public class HintManager : MonoBehaviour
    {
        public static HintManager Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("The same LevelData asset the LevelBuilder uses — supplies the hint text.")]
        [SerializeField] private LevelData _levelData;

        // How many hints have been revealed so far this level (0-3).
        private int _hintsRevealed;

        // ── Public read-only state (drives the hint button UI) ─────────────────────

        /// <summary>Total authored hints, capped at the 3 supported tiers.</summary>
        public int TotalHints => _levelData != null ? Mathf.Min(3, _levelData.hints.Count) : 0;

        /// <summary>True while at least one more hint remains to be revealed.</summary>
        public bool HasMoreHints => _hintsRevealed < TotalHints;

        /// <summary>The tier that the next hint request would reveal (1-3).</summary>
        public int NextTier => _hintsRevealed + 1;

        /// <summary>Coin cost of the next hint, or 0 when none remain.</summary>
        public int NextHintCost =>
            HasMoreHints && CoinManager.Instance != null ? CoinManager.Instance.HintCost(NextTier) : 0;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Reveals the next hint if one remains and the player can pay for it.
        /// Raises <see cref="GameEvents.OnHintRevealed"/> on success, or
        /// <see cref="GameEvents.OnHintDenied"/> with a reason otherwise.
        /// </summary>
        public void RequestNextHint()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("[HintManager] No LevelData assigned — cannot serve hints.", this);
                return;
            }

            if (!HasMoreHints)
            {
                GameEvents.RaiseHintDenied("No more hints");
                return;
            }

            CoinManager coins = CoinManager.Instance;
            if (coins == null) return;

            int tier = NextTier;
            if (!coins.TryPurchaseHint(tier))
            {
                GameEvents.RaiseHintDenied($"Need {coins.HintCost(tier)} coins");
                return;
            }

            _hintsRevealed = tier;
            GameEvents.RaiseHintRevealed(_levelData.hints[tier - 1], tier);
        }
    }
}

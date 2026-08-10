using UnityEngine;

namespace OneWayTogether.Data
{
    /// <summary>
    /// ScriptableObject that holds all tunable values for the coin economy.
    /// Create via Assets > Create > OneWayTogether > Coin System Data.
    /// </summary>
    [CreateAssetMenu(fileName = "CoinSystemData", menuName = "OneWayTogether/Coin System Data")]
    public class CoinSystemData : ScriptableObject
    {
        [Header("Respawn")]
        [Tooltip("Number of coins spent to respawn both characters in place instead of resetting to checkpoint.")]
        [SerializeField, Range(1, 20)] private int _respawnCost = 5;

        [Header("Hints")]
        [Tooltip("Coin cost for the first (vague nudge) hint tier.")]
        [SerializeField, Range(1, 10)] private int _hint1Cost = 2;

        [Tooltip("Coin cost for the second (more specific) hint tier.")]
        [SerializeField, Range(1, 20)] private int _hint2Cost = 5;

        [Tooltip("Coin cost for the third (full solution reveal) hint tier.")]
        [SerializeField, Range(1, 30)] private int _hint3Cost = 10;

        // ── Public accessors ─────────────────────────────────────────────────────

        public int RespawnCost => _respawnCost;
        public int Hint1Cost => _hint1Cost;
        public int Hint2Cost => _hint2Cost;
        public int Hint3Cost => _hint3Cost;
    }
}

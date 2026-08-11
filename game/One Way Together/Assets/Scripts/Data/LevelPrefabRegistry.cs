using UnityEngine;

namespace OneWayTogether.Data
{
    /// <summary>
    /// Single-source registry that maps every prefab type to a concrete asset.
    /// LevelBuilder reads this registry to instantiate objects and platforms
    /// without storing hard asset references per-level.
    ///
    /// Create via Assets > Create > OneWayTogether > Level Prefab Registry.
    /// One registry is shared by all levels.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelPrefabRegistry", menuName = "OneWayTogether/Level Prefab Registry")]
    public class LevelPrefabRegistry : ScriptableObject
    {
        [Header("Environment Platforms (3D models)")]
        [Tooltip("Forest dirt/wood platform prefab. Must have BoxCollider2D on the Ground layer.")]
        public GameObject forestPlatformPrefab;

        [Tooltip("Stone wall segment prefab. Must have BoxCollider2D on the Ground layer.")]
        public GameObject stoneWallPrefab;

        [Tooltip("Decorative background wall — no collider needed.")]
        public GameObject backgroundWallPrefab;

        [Header("Puzzle Objects")]
        [Tooltip("Gate prefab — must have Gate.cs attached.")]
        public GameObject gatePrefab;

        [Tooltip("Lever prefab — must have Lever.cs attached.")]
        public GameObject leverPrefab;

        [Tooltip("ReunionTrigger prefab — must have ReunionTrigger.cs attached.")]
        public GameObject reunionTriggerPrefab;

        [Tooltip("CheckpointTrigger prefab — must have CheckpointTrigger.cs attached.")]
        public GameObject checkpointPrefab;

        [Tooltip("RopeTrigger prefab — must have RopeTrigger.cs attached.")]
        public GameObject ropeTriggerPrefab;

        [Header("Collectibles")]
        [Tooltip("Coin pickup prefab — must have CoinPickup.cs attached.")]
        public GameObject coinPrefab;
    }
}

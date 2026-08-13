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
        [Header("Legacy Environment Platforms")]
        [Tooltip("Forest dirt/wood platform prefab.")]
        public GameObject forestPlatformPrefab;

        [Tooltip("Stone wall segment prefab.")]
        public GameObject stoneWallPrefab;

        [Tooltip("Decorative background wall — no collider needed.")]
        public GameObject backgroundWallPrefab;

        [Header("Polyworks — RPG Tiles")]
        public GameObject rpgFloorCoveredPrefab;
        public GameObject rpgFloorDirtPrefab;
        public GameObject rpgFloorStonePrefab;
        public GameObject rpgFloorWaterPrefab;
        public GameObject rpgRampCoveredPrefab;

        [Header("Polyworks — Vegetation")]
        public GameObject vegetationBushLargePrefab;
        public GameObject vegetationBushSmallPrefab;
        public GameObject vegetationFernPrefab;

        [Header("Polyworks — Rocks")]
        public GameObject rockBoulderPrefab;
        public GameObject rockChunksPrefab;

        [Header("Polyworks — Ruins")]
        public GameObject ancientRuinsArchPrefab;
        public GameObject ancientRuinsColumnPrefab;
        public GameObject ancientRuinsWallPrefab;

        [Header("Polyworks — Tall Trees")]
        public GameObject vegetationTreePinePrefab;
        public GameObject vegetationTreeAlpinePrefab;

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

        [Tooltip("Bridge prefab — must have Bridge.cs attached.")]
        public GameObject bridgePrefab;

        [Header("Props")]
        [Tooltip("Pushable boulder — must have a Rigidbody and be on the Stackable layer.")]
        public GameObject pushBoulderPrefab;

        [Tooltip("Stone pressure plate — triggers on Rigidbody or Character overlap.")]
        public GameObject stonePressurePlatePrefab;

        [Header("Collectibles")]
        [Tooltip("Coin pickup prefab — must have CoinPickup.cs attached.")]
        public GameObject coinPrefab;
    }
}

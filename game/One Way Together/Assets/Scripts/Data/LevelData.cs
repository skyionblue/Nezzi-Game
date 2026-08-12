using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneWayTogether.Data
{
    // ── Enums ─────────────────────────────────────────────────────────────────

    public enum PlatformType
    {
        ForestPlatform,
        StoneWall,
        BackgroundWall,

        // Polyworks RPG Tiles
        RPGFloorCovered,    // grass-covered ground square
        RPGFloorDirt,       // dirt ground square
        RPGFloorStone,      // stone ground square
        RPGFloorWater,      // water square
        RPGRampCovered,     // grass ramp (for slopes)

        // Polyworks vegetation
        VegetationBushLarge,
        VegetationBushSmall,
        VegetationFern,

        // Polyworks rocks
        RockBoulder,
        RockChunks,

        // Polyworks ruins
        AncientRuinsArch,
        AncientRuinsColumn,
        AncientRuinsWall,
    }

    // ── Platform definition ───────────────────────────────────────────────────

    /// <summary>
    /// A single 3D platform/wall tile entry placed at runtime by LevelBuilder.
    /// Scale and yRotation let one prefab serve many layout needs without
    /// requiring separate art assets for each orientation.
    /// </summary>
    [Serializable]
    public class PlatformDef
    {
        public PlatformType type;
        public Vector3      position;
        public Vector3      scale     = Vector3.one;
        public float        yRotation = 0f;
    }

    // ── Level ScriptableObject ────────────────────────────────────────────────

    /// <summary>
    /// Data-driven level definition. No tilemap — platforms are 3D prefab
    /// instances; interactive objects are placed by type with runtime config.
    ///
    /// All positions are full 3D world-space Vector3 values matching the XZ
    /// horizontal plane used by the HD-2D isometric camera.
    ///
    /// Create via Assets > Create > OneWayTogether > Level Data.
    /// One asset per level; reference from the LevelBuilder component in the
    /// scene.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevel", menuName = "OneWayTogether/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string levelName = "Untitled Level";

        [Header("Characters — 3D world-space start positions (XZ floor plane)")]
        public Vector3 scarletStart = new Vector3(-3f, 0f, 0f);
        public Vector3 daniStart    = new Vector3(6f,  0f, 0f);

        [Header("Platforms (3D models)")]
        public List<PlatformDef> platforms = new List<PlatformDef>();

        [Header("Interactive Objects")]
        [Tooltip("Gates, levers, coins, triggers — populated by type at runtime.")]
        public List<LevelObjectData> objects = new List<LevelObjectData>();

        [Header("Hints (progressive — tier 1 vague → tier 3 full solution)")]
        [Tooltip("Up to 3 hints for this puzzle, revealed in order for escalating coin cost.")]
        [TextArea(2, 4)]
        public List<string> hints = new List<string>();

        [Header("Camera")]
        [Tooltip("Camera.backgroundColor applied at level load.")]
        public Color skyColor         = new Color(0.1f, 0.15f, 0.1f, 1f);
        [Tooltip("Unused in perspective mode — kept for legacy reference.")]
        public float orthographicSize = 8f;
    }
}

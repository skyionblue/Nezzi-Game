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
    /// Create via Assets > Create > OneWayTogether > Level Data.
    /// One asset per level; reference from the LevelBuilder component in the
    /// scene.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevel", menuName = "OneWayTogether/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string levelName = "Untitled Level";

        [Header("Characters")]
        public Vector2 scarletStart = new Vector2(-2f,  1.9f);
        public Vector2 daniStart    = new Vector2(9f,   1.432f);

        [Header("Platforms (3D models)")]
        public List<PlatformDef> platforms = new List<PlatformDef>();

        [Header("Interactive Objects")]
        [Tooltip("Gates, levers, coins, triggers — populated by type at runtime.")]
        public List<LevelObjectData> objects = new List<LevelObjectData>();

        [Header("Camera")]
        [Tooltip("Camera.backgroundColor applied at level load.")]
        public Color skyColor         = new Color(0.15f, 0.2f, 0.15f, 1f);
        [Tooltip("OrthographicSize pushed to the SinglePlayerCam virtual camera.")]
        public float orthographicSize = 4f;
    }
}

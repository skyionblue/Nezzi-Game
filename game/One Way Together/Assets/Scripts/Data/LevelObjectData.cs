using UnityEngine;

namespace OneWayTogether.Data
{
    /// <summary>
    /// Identifies a type of runtime object that <see cref="OneWayTogether.Core.LevelBuilder"/>
    /// can instantiate from a <see cref="LevelPrefabRegistry"/>.
    /// </summary>
    public enum LevelObjectType
    {
        Scarlet        = 0,
        Dani           = 1,
        Gate           = 2,
        Lever          = 3,
        Coin           = 4,
        ReunionTrigger = 5,
        Checkpoint     = 6,
        RopeTrigger    = 7,
        Bridge         = 8,
        PushBoulder    = 9,
    }

    /// <summary>
    /// A single entry in the <see cref="LevelData.objects"/> list.
    /// Describes where an object spawns and how it should be configured.
    ///
    /// All positions are in 3D world space (XZ floor plane). Y is the floor level (0).
    /// For Gate/Lever pairs, <see cref="id"/> must match on both entries
    /// so the Gate subscribes to the correct plate event.
    /// </summary>
    [System.Serializable]
    public class LevelObjectData
    {
        [Tooltip("What kind of object to instantiate at runtime.")]
        public LevelObjectType type;

        [Tooltip("World-space position for this object (XZ floor plane, Y = floor height).")]
        public Vector3 position;

        [Tooltip("Y-axis rotation (degrees) applied at spawn. Lets one prefab face any direction — e.g. a Gate rotated 90 to span a Z-oriented wall gap.")]
        public float yRotation = 0f;

        [Tooltip("Shared ID linking a Gate to its controlling Lever/PressurePlate. Must match on both entries.")]
        public string id;

        [Tooltip("For Lever: when true the gate stays open after the lever is thrown once.")]
        public bool oneShot;

        [Tooltip("For ReunionTrigger: overrides the BoxCollider size. Leave at (0,0) to keep the prefab default.")]
        public Vector2 triggerSize;

        [Tooltip("For Gate: local-space offset the gate moves to when open. Defaults to (0, 2.5, 0) if zero.")]
        public Vector3 gateOpenOffset;
    }
}

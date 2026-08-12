namespace OneWayTogether.Data
{
    /// <summary>
    /// Encodes what a single ASCII character means in a <see cref="LevelData"/> tile row.
    /// The enum value itself is not stored per-tile at runtime — it is only used during
    /// the build pass in <see cref="OneWayTogether.Core.LevelBuilder"/>.
    /// </summary>
    public enum LevelTileType
    {
        Empty      = 0,  // ' ' — no tile placed
        Ground     = 1,  // '#' — solid foreground tile (Ground tilemap)
        Background = 2,  // '.' — decorative background tile (Background tilemap)
    }
}

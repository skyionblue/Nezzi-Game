# Unity Blueprint: World 1 — Puzzle 11 — The Low Road

**Scene:** Assets/Scenes/World1_Puzzle11.unity
**LevelData:** Assets/ScriptableObjects/Levels/World1_Puzzle11.asset

---

## Character Start Positions

- Scarlet: (−3, 0, −6)
- Dani: (3, 0, −6)

## Ground Tiles (via LevelData — RPGFloorDirt, type 3, y=−1.5, scale y=0.5)

South zone — x in {−6,−3,0,3,6} × z in {−9,−6,−3} = 15 tiles
North zone — x in {−6,−3,0,3,6} × z in {3,6,9} = 15 tiles
No floor tiles at z=0 (wall row — tunnel and gate prefabs cover that span)

## Wall Tiles (via LevelData — StoneWall, type 1, y=0, yRotation=90)

At z=0: x=−6, x=−3, x=3
(x=0 = gate; x=6 = tunnel — no wall tile at those positions)
Boundary west: x=−9, z in {−9..9}
Boundary east: x=9, z in {−9..9}
Boundary south: z=−12, x in {−9..9}
Boundary north: z=12, x in {−9..9}

## Interactive Objects (via LevelData)

| Object | Type | Position | Config |
|---|---|---|---|
| Checkpoint | 6 | (0, 0, −9) | — |
| Gate | 2 | (0, 0, 0) | id="Dani_Lever", gateOpenOffset=(0,3.7,0), yRotation=0 |
| Lever | 3 | (0, 0, 6) | id="Dani_Lever", oneShot=1, yRotation=0 |
| ReunionTrigger | 5 | (0, 0, 9) | triggerSize=(6,6) |
| Coin | 4 | (−3, 0, −3) | — |
| Coin | 4 | (3, 0, −3) | — |
| Coin | 4 | (3, 0, 6) | — |

## Manual Scene Objects (place via Unity MCP after LevelData build)

### CrawlTunnelEntrance

- Prefab: `Assets/Prefabs/Env/CrawlTunnelEntrance.prefab`
- Position: (6, 0, 0)
- Rotation: yRotation=180 (entrance faces south toward approaching characters)
- Parent: Objects container

### CrawlTrigger (child of CrawlTunnelEntrance)

- Add empty child GameObject named `CrawlZone`
- Add BoxCollider (isTrigger=true)
- Add CrawlTrigger component
- Size the BoxCollider to match the tunnel interior (tune in Editor)
- This fires `DaniController.SetCrawlingState(true/false)` as Dani enters/exits

## LevelSequenceController

- Add "World1_Puzzle11" to `_levelScenes` array in World1_Puzzle1.unity
- Add scene to Build Settings

## Reunion Trigger

- Position: (0, 0, 9)
- triggerSize: (6, 6) — wide enough to catch both chars approaching from different sides

## Camera Notes

Standard isometric follow. No overrides needed.
The trust beat (Scarlet watching the tunnel go dark) is delivered by the
isometric framing naturally — no special camera work required.

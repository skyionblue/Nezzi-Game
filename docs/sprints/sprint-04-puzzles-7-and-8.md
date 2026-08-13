# Sprint 04 — Puzzles 7 and 8 Playable

**Goal:** Make Puzzle 7 (First Combination) and Puzzle 8 (Hold and Go) fully playable end-to-end, with multi-height terrain as the dominant spatial feature of both rooms — not a token raised ledge but the structural reason each puzzle works the way it does.
**Theme:** Vertical space + mechanic combination (Puzzle 7) / simultaneous action across elevations (Puzzle 8)
**Status:** Planned

---

## Why this sprint

Sprints 01–03 delivered six puzzles. The developer's direct feedback: Puzzles 5 and 6 still feel like flat-plane puzzles even though a raised ledge exists in Puzzle 5. The spatial design of Puzzles 7 and 8 must treat height as the *reason* the puzzle works, not an accessory bolted onto an otherwise 2D layout.

The good news: multi-height terrain is now fully proven from Sprint 03. Floor tiles placed at `y = -0.5` (scale `y = 0.5`) produce a top surface at `y = 1.0 m`, exactly above Scarlet's `stepHeight = 0.6 m` ceiling. Characters can walk across raised surfaces. The lift mechanic is working. The data-authoring pattern is understood. This sprint builds on those foundations without repeating the terrain spike.

**Puzzle 7 (First Combination)** is the first time two previously-separate mechanics — boulder push and Dani-only tunnel — appear in the same room. The design doc presents this as a flat combination; the sprint redesigns the spatial layout so the two mechanics are *separated by height*. Scarlet operates at ground level (boulder, pressure plate, main gate); Dani must reach an elevated section (accessible only through the tunnel) to throw the lever that opens the gate. Neither character's path crosses the other's elevation. The combination of mechanics is what bridges the two levels.

**Puzzle 8 (Hold and Go)** introduces simultaneous action: Scarlet must hold a pressure plate while Dani crosses a bridge. The design doc puts this on a single plane; the sprint puts Scarlet's plate on a raised ledge she must walk up to reach, while the bridge and Dani's path exist at ground level below. Scarlet looking down from her platform at Dani crossing is a spatial read that communicates "I am holding this for you" without any tutorial text. After Dani crosses and throws a one-shot lever (opening Scarlet's gate permanently), Scarlet can step off the plate (bridge retracts — Dani is already on solid ground on the far side) and descend through her now-open gate. Reunion happens at ground level.

Both puzzles extend the sequence to eight levels. Completing Puzzle 8 continues to MainMenu.

---

## Key design decisions captured here

### Multi-height is structural, not decorative

Every room in this sprint must have a layout where removing the height difference would break the puzzle — not just make it look flatter. The test: can you solve the puzzle if you flatten everything to y=0? If yes, the height is decorative. If no, the height is structural. Both rooms must fail this test.

**Puzzle 7 check:** If you flatten the room, Dani can walk around the tunnel entrance (no elevation forces her through it), and the lever is reachable without tunneling. The puzzle loses its structure. Therefore the lever must be placed on the elevated section and reachable *only* through the tunnel.

**Puzzle 8 check:** If you flatten the room, Scarlet can reach the pressure plate and still see the bridge at the same elevation — the spatial "I'm up here holding this for you" read is lost, and the no-bypass enforcement is weaker (walls rather than fall hazards). Therefore Scarlet's plate must be on a raised ledge, with the bridge gap and Dani's path at the lower level.

### Puzzle 7 — spatial layout

```
SIDE VIEW (simplified):

 ┌──────────────────────────────────────┐
 │  UPPER LEVEL (y≈1.0)                 │
 │   [tunnel exit] → lever(^) → wall    │
 └──────────────────────────────────────┘
 ┌──────────────────────────────────────┐
 │  GROUND LEVEL (y=0)                  │
 │  S  D  ○  [pit/plate≡]  [tunnel...  │
 │                        [  ]gate → ★ │
 └──────────────────────────────────────┘
```

Sequence:
1. Scarlet and Dani start at ground level. A boulder sits on the path.
2. A pressure plate sits in a floor pit to one side. A closed tunnel entrance is on the far wall — currently blocked by a gate or barrier that will only lift when the pressure plate is activated.
3. Scarlet pushes the boulder into the pit. It lands on the plate. The tunnel entrance unlatches (its barrier lifts or dissolves — use a Gate component with `id = "Boulder_Plate"` set to open when the plate fires).
4. Dani crawls through the tunnel. The tunnel climbs slightly, exiting onto the **upper level**.
5. On the upper level, a lever sits at the far end — reachable only from the upper level, physically above any path Scarlet can reach. Dani throws the lever (`id = "Dani_Lever"`, `oneShot = true`).
6. The main gate at ground level swings open (gate linked to `id = "Dani_Lever"`).
7. Scarlet walks through the main gate. They reunite at ground level on the far side.

**Neither alone:** Scarlet cannot reach the lever (it is on the upper level; Scarlet's stepHeight cannot reach it from below, and the tunnel is too narrow for Scarlet). Dani cannot push the boulder (only `ScarletController.OnControllerColliderHit` applies force). If the boulder is not on the plate, the tunnel is blocked — Dani cannot even begin her path.

**The tunnel climb:** The tunnel in existing levels is a geometry-only width restriction on the XZ plane. To have it exit at a higher Y, the `CrawlTunnelEntrance` prefab's exit must be positioned at the upper-level floor height. This is the one new spatial authoring question — verify in Tasks 1 and 3 that a character exiting a tunnel can reliably land on an elevated surface without jitter or clipping. If the exit height causes issues, the tunnel can exit at ground level and a short ramp (narrow enough to block Scarlet's capsule) leads from the tunnel exit up to the lever platform.

### Puzzle 8 — spatial layout

```
SIDE VIEW (simplified):

  ┌────────────────────────────────────┐
  │  UPPER LEVEL (y≈1.0)               │
  │  [ramp/steps] → ≡(plate) → S      │
  └────────────────────────────────────┘
                                             
  ┌───────────────────────────────────┐
  │  GROUND LEVEL (y=0)               │
  │  D  [  ]gate ← lever(^)  [gap]====│
  │           (Dani's gate)            │
  └───────────────────────────────────┘

  ════  = bridge (deploys across gap when plate is active)
  ≡     = pressure plate on upper ledge (Scarlet stands here)
  [  ]  = Scarlet's gate (at the base of her ledge, ground level)
```

Sequence:
1. Dani starts at ground level. A gap in the floor blocks her path to the lever and the reunion point. The lever is on the far side of the gap.
2. Scarlet starts at ground level but must walk up a ramp to reach the upper ledge, where the pressure plate sits.
3. Scarlet walks up the ramp and stands on the plate. The bridge deploys across Dani's gap below.
4. Dani crosses the bridge. She reaches the lever on the far side and throws it (one-shot, `id = "Dani_Lever"`). This opens Scarlet's gate at the base of the ledge (ground level) and permanently latches.
5. Scarlet steps off the plate (bridge retracts — Dani is now on solid ground on the far side and is safe). Scarlet descends the ramp and walks through her now-open gate.
6. Both reach the reunion trigger at ground level on the far side.

**The no-retract problem is solved by layout, not new code.** When Dani throws the one-shot lever, the gate opens permanently (one-shot lever, game-tested pattern). The bridge retracts when Scarlet steps off, but Dani is already on solid ground — she crossed the bridge and is standing on the permanent far-side floor. Only the bridge is temporary. No changes to `Bridge.cs` are needed.

**The "hold and go" beat is preserved:** Scarlet physically cannot leave the upper ledge without descending the ramp (past the plate) while Dani is still crossing. The layout enforces the timing: if Scarlet leaves early, the bridge retracts and Dani cannot cross. The player must solve this in sequence.

**Neither alone:** Scarlet cannot open her own gate (it is linked to Dani's lever, which only Dani can reach by crossing the bridge). Dani cannot deploy the bridge (she has no pressure plate; the plate is on the upper ledge, which Dani could physically reach but the plate only matters if Scarlet stands on it — if needed, block Dani's access to the upper ledge with a narrow-enough ramp that Scarlet's capsule fits but not in a way that blocks Dani; or position the plate at the far end of the upper ledge away from the ramp so Dani would have to walk all the way there, which is a design issue — see Risks).

> **Note on Dani reaching the plate:** If Dani can walk up the ramp and stand on the plate herself, the "neither alone" rule is violated (Dani could deploy the bridge and throw the lever without Scarlet). Prevent this by making the ramp accessible only from Scarlet's starting side (a one-way spatial arrangement) or by placing a narrow-for-Scarlet passage that Dani must use on the ground level, separating their paths from the start. See Task 3 for the authoring decision. The cleanest solution: put the plate on a section of the upper ledge that can only be reached from Scarlet's starting position (e.g., the ledge overhangs the gap and is not connected to the ground path Dani walks).

### Puzzle 8 — bridge direction

`Bridge` deploys vertically from above (drops down). For it to span a horizontal gap at ground level, its authored position is at the center of the gap at ground level; it starts 20 units above (retractOffset) and drops into place. When viewed from the isometric camera this reads as a platform materializing across the gap — acceptable and consistent with how the bridge in Puzzle 2 was proven. If the visual read is unclear, prefix it with a visual cue (a glowing or highlighted floor tile at the landing zone).

### Puzzle 7 — tunnel-gate unlock mechanic

The tunnel entrance in existing puzzles is a size restriction only (narrow enough that Scarlet's capsule is blocked, Dani's passes). The design for Puzzle 7 needs the tunnel to be *locked* until Scarlet pushes the boulder onto the plate. The cleanest implementation: a `Gate` (type 2) placed at the tunnel entrance, linked to the boulder's pressure plate (`id = "Boulder_Plate"`). The gate swings open when the plate activates, revealing the tunnel mouth. No new code — `Gate` + `PressurePlate` + `PushBoulder` is the proven Puzzle 3 combo. The tunnel itself is unchanged.

---

## Success criteria (sprint definition of done)

- From Main Menu, the player can play Puzzles 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 in sequence without a manual scene change or editor intervention; completing Puzzle 8 returns to MainMenu.
- **Puzzle 7:** Scarlet pushes the boulder onto the pressure plate; the tunnel entrance gate opens; Dani crawls through the tunnel and exits onto the upper level; Dani throws the lever; the main gate at ground level opens; Scarlet walks through; both reunite. Scarlet cannot reach the lever without Dani tunneling to it. Dani cannot enter the tunnel without the boulder on the plate.
- **Puzzle 8:** Scarlet walks up the ramp to the upper ledge and stands on the pressure plate; the bridge deploys across Dani's gap; Dani crosses the bridge and throws the one-shot lever; Scarlet's gate opens permanently; Scarlet descends the ramp and walks through her gate; both reunite. Neither character can complete it alone. The bridge retracting after Scarlet leaves the plate does not strand Dani (she is on solid ground by then).
- **Height is structural in both rooms:** removing the height difference (flattening both rooms to y=0) would break the puzzle's core mechanic separation.
- Failure (fall into a hazard) in either new puzzle returns both characters to that puzzle's checkpoint; coin respawn still works.
- Hints load correctly per puzzle — Puzzle 7 hints are Puzzle 7 text, Puzzle 8 hints are Puzzle 8 text (not copied from a prior level).
- `LevelSequenceController._levelScenes` in `World1_Puzzle1` lists all eight puzzle scenes in order.

---

## Backlog

| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | **SPIKE — tunnel-to-elevated-exit authoring.** Test whether a character exiting a `CrawlTunnelEntrance` prefab can reliably land on a floor tile 1.0 m above the entry level. Do this in a throwaway LevelData. Record: does the character exit cleanly onto the raised surface, or does it jitter/clip? If it fails, test the fallback: tunnel exits at ground level, and a **narrow ramp** (width of a Dani capsule, not a Scarlet capsule) leads from the tunnel exit up to the lever platform. Record the chosen construction so Puzzle 7 authoring can reuse it. | M | In play mode, Dani exits the tunnel and stands stably on the upper-level floor (or on the narrow ramp that leads there). The exact exit position and any ramp dimensions are written down. | — |
| 2 | Create `World1_Puzzle7` scene by duplicating `World1_Puzzle6.unity`; strip all prior objects; wire in a new `World1_Puzzle7` LevelData asset. Confirm the scene opens with both characters spawning at origin and no leftover Puzzle 6 objects. | S | Scene opens in play mode; both characters spawn; no prior puzzle elements remain; `HintManager` points to the new `World1_Puzzle7` LevelData. | — |
| 3 | Author `World1_Puzzle7` LevelData: ground-level floor for Scarlet + Dani start; a `PushBoulder` (type 9) on the path; a floor pit with a `StonePressurePlate` (`id = "Boulder_Plate"`) that the boulder rolls into; a `Gate` (type 2) at the tunnel entrance linked to `id = "Boulder_Plate"` (swings open when plate activates); a `CrawlTunnelEntrance` whose exit is at (or ramps up to) the upper level per Task 1's proven construction; upper-level floor tiles at `y = -0.5, scale.y = 0.5`; a `Lever` on the upper level (`id = "Dani_Lever"`, `oneShot = true`); a `Gate` at the main exit at ground level linked to `id = "Dani_Lever"`; a `ReunionTrigger` beyond the main gate; a `Checkpoint` near the entry; 3 progressive hints. Enforce no-bypass with floor-gap choke (fall hazard on the divide). | M | In play mode: Scarlet pushes the boulder into the pit; the tunnel entrance gate opens; Dani crawls through and exits on the upper level; Dani throws the lever; the main gate opens; Scarlet walks through; both reunite. Scarlet alone cannot reach the lever. | Tasks 1, 2 |
| 4 | Verify Puzzle 7 "neither alone" and no-bypass. Confirm: (a) Scarlet cannot reach the upper-level lever by walking (stepHeight blocks it); (b) Dani cannot enter the tunnel before the boulder lands on the plate (tunnel gate is closed); (c) a character walking off the floor to skip the gate drops into the fall hazard. Tune boulder push distance, pit dimensions, and tunnel entrance gate position as needed. | S | Puzzle 7 solves correctly in single-player by switching. All three bypass attempts fail as described. | Task 3 |
| 5 | Create `World1_Puzzle8` scene by duplicating `World1_Puzzle7.unity`; strip all prior objects; wire in a new `World1_Puzzle8` LevelData asset. Confirm the scene opens with both characters spawning and no leftover Puzzle 7 objects. | S | Scene opens in play mode; both characters spawn; no prior puzzle elements remain; `HintManager` points to the new `World1_Puzzle8` LevelData. | — |
| 6 | Author `World1_Puzzle8` LevelData: Dani starts at ground level with a gap blocking her path; Scarlet starts at ground level on a separate side with a ramp leading to an upper ledge (upper tiles at `y = -0.5, scale.y = 0.5`); a `StonePressurePlate` at the far end of the upper ledge (`id = "Bridge_Plate"`, `oneShot = false` — must be held); a `Bridge` (type 8) spanning the gap at ground level, linked to `id = "Bridge_Plate"`; the upper ledge must be spatially arranged so Dani cannot reach the plate without crossing Scarlet's starting side (verify in Task 7); a `Lever` on the far side of the gap (`id = "Dani_Lever"`, `oneShot = true`); a `Gate` at the base of Scarlet's ledge (ground level) linked to `id = "Dani_Lever"` — this is the gate Scarlet walks through to reunite; a `ReunionTrigger` beyond Scarlet's gate; a `Checkpoint` at entry; 3 progressive hints. | M | In play mode: Scarlet walks up the ramp and stands on the plate; bridge deploys at ground level; Dani crosses; Dani throws the one-shot lever; Scarlet's gate opens permanently; Scarlet descends the ramp and walks through the gate; both reunite. Bridge retracting after Scarlet leaves plate does not strand Dani (she is on the far-side permanent floor). | Task 5 |
| 7 | Verify Puzzle 8 "neither alone," timing, and no-bypass. Confirm: (a) Dani cannot access the pressure plate from her starting side (upper ledge is not reachable from Dani's ground-level path); (b) if Scarlet leaves the plate before Dani crosses, the bridge retracts and Dani cannot cross — the puzzle must be restarted from checkpoint (or coin-respawned); (c) after Dani crosses and throws the lever, Scarlet's gate stays open even after Scarlet steps off the plate. Tune ramp width (wide enough for Scarlet's capsule, but confirm whether Dani can also use it — if yes, add a geometry blocker or adjust layout so the plate is only reachable from Scarlet's starting approach). | S | All three checks pass in play mode. Puzzle 8 solves by switching in the correct order: Scarlet to plate → switch to Dani → cross bridge → throw lever → switch to Scarlet → descend → walk through gate → reunite. | Task 6 |
| 8 | Extend `LevelSequenceController._levelScenes` (Inspector edit on the `DontDestroyOnLoad` GameObject in `World1_Puzzle1` scene) to include all eight puzzle scenes in order. Add `World1_Puzzle7` and `World1_Puzzle8` to Build Settings. Confirm Puzzle 8 completion returns to MainMenu. | S | Playing from MainMenu proceeds through all eight puzzles in order; completing Puzzle 8 returns to MainMenu without error. Both new scenes appear in Build Settings. | Tasks 3, 6 |
| 9 | Add fall hazards (boundary `Hazard` triggers) and coins (3–5 per puzzle) to both new levels. Confirm failure → checkpoint → respawn flow and coin spend/respawn in both new puzzles. | S | Triggering a fall hazard in Puzzle 7 or 8 returns both characters to that puzzle's `Checkpoint`. Spending coins respawns in place. Coin count increments correctly per puzzle. | Tasks 3, 6 |

---

## Out of scope

- **Vertical climb (rope/vine).** `DaniController.SetClimbingState` stays a stub. Height transitions in both puzzles are delivered by floor-tile ramps and tunnel exits, not by rope climbing.
- **New props or Meshy/Blender art.** Both puzzles use existing platform types and prefabs already in `LevelPrefabRegistry`. No new 3D assets are planned.
- **Bridge locking via a separate code path.** The no-retract problem in Puzzle 8 is solved by spatial layout (Dani is on permanent floor before the bridge retracts), not by adding a `_lockPlateId` to `Bridge.cs`. If this design assumption fails during Task 7 (e.g., the bridge retracts before Dani finishes crossing in a realistic timing window), the fix is to slow `_lowerSpeed` on that bridge instance — still no new code.
- **Audio / sound cues.** Sound design is deferred. Visual feedback (gate swinging, bridge descending, plate colour change) is the sole feedback channel.
- **Stacking mechanic as a puzzle element.** `DaniController.TryPickUpObject` exists and works. It is not used in either puzzle this sprint. It is available as a future mechanic introduction.
- **Puzzle 9 or 10.** Those are Sprint 05 and beyond.
- **A dedicated raised-platform prefab.** The proven floor-tile-stack pattern from Sprint 03 is reused. No new environment prefabs.

---

## Risks & assumptions

- **Tunnel-to-elevated-exit is the primary unknown.** Task 1 is a hard gate before Puzzle 7 authoring. The existing `CrawlTunnelEntrance` prefab was designed for a flat exit — whether the character cleanly exits at a higher Y (landing on a raised floor tile) is unproven. If it fails, the fallback (narrow ramp from tunnel exit to the lever platform) is pre-authorized. Record which path was taken; do not silently skip to the fallback without documenting the failure mode.

- **Dani reaching Scarlet's pressure plate in Puzzle 8.** If the ramp Scarlet uses to reach the upper ledge is also reachable by Dani, the "neither alone" rule is violated. The spatial layout must prevent this — either by making the ramp accessible only from Scarlet's starting side (the two characters start on opposite sides of a ground-level divide), or by placing the plate at the far end of a ledge section that geometrically overhangs the gap and cannot be reached from the ground path Dani walks. Task 7 specifically tests this. If the layout fails the test, a thin geometry blocker (a wall section at the ramp base on Dani's side) is the fix — this is data authoring, not new code.

- **Bridge timing window.** In Puzzle 8, after Dani throws the lever and Scarlet steps off the plate, there is a brief window while Scarlet descends the ramp. The bridge immediately starts retracting. If the bridge retracts fast enough to visually alarm the player (implying Dani is in danger), this reads poorly even if Dani is safe. Tune `_lowerSpeed` on this bridge instance to retract slowly — a data change, not code. Flag in Task 7 if the default speed reads badly.

- **`LevelSequenceController._levelScenes` only lists four scenes in its serialized default.** Sprint 03 extended it to six. This sprint extends it to eight. The array is serialized in the Inspector on the `DontDestroyOnLoad` GameObject in `World1_Puzzle1`. Task 8 is an Inspector edit, not a code change — but it must be done in the `World1_Puzzle1` scene, not in a new scene. Verify the singleton in `World1_Puzzle1` is the one being edited.

- **Assumption: both new scenes can be created by duplicating an existing puzzle scene.** This has been the proven pattern since Sprint 02. Duplicate `World1_Puzzle6` for Puzzle 7 (Task 2) and `World1_Puzzle7` for Puzzle 8 (Task 5). Always strip leftover objects after duplication and verify `HintManager` points to the correct new `LevelData`.

- **No new `LevelObjectType` enum values are needed.** Puzzle 7 uses: `PushBoulder (9)`, `StonePressurePlate (10)`, `Gate (2)`, `Lever (3)`, `ReunionTrigger (5)`, `Checkpoint (6)`, `Coin (4)`. Puzzle 8 uses: `StonePressurePlate (10)`, `Bridge (8)`, `Lever (3)`, `Gate (2)`, `ReunionTrigger (5)`, `Checkpoint (6)`, `Coin (4)`. All types are already in the enum and in `LevelPrefabRegistry`. No code additions to `LevelObjectData.cs` or `LevelBuilder.cs` are required.

---

## References

- Design: `README-Puzzle-Design.md` — Puzzle 7 "First Combination", Puzzle 8 "Hold and Go", Design Rules ("neither character solves it alone", "one new mechanic per puzzle"), Difficulty Curve (Puzzle 7: Medium, Puzzle 8: Medium)
- Design: `README-Design-Feedback.md` — Reunion win condition, Failure/Checkpoint, Coin/Hint systems, "neither solves it alone" rule, "very difficult by the end"
- Developer feedback (Sprint 04 brief): Puzzles 5 and 6 are too easy and still feel single-plane; next puzzles must use multi-height terrain as the *dominant* spatial feature, not an accessory. Difficulty must ramp noticeably from Puzzle 6.
- Standing preference (bridges over vertical climb): rope/vine climb stays a stub; height transitions use ramps, tunnel exits, and the proven floor-tile-stack construction from Sprint 03.
- Proven terrain construction (Sprint 03): floor tiles at `position.y = -0.5`, `scale.y = 0.5` give a top walkable surface at `y = 1.0 m`. Scarlet's `stepHeight = 0.6` cannot reach this unassisted. Characters walk stably across the top.
- Core systems: `Assets/Scripts/Core/LevelBuilder.cs`, `Assets/Scripts/Data/LevelData.cs` (`PlatformDef.position` / `scale`), `Assets/Scripts/Data/LevelObjectData.cs` (`LevelObjectType` enum — all needed types already present), `Assets/ScriptableObjects/LevelPrefabRegistry.asset`
- Puzzle objects: `Assets/Scripts/Puzzle/Bridge.cs` (one `_plateId`, deploys/retracts on `OnPressurePlateChanged`), `Assets/Scripts/Puzzle/Lever.cs` (`_oneShot`, fires `OnPressurePlateChanged`), `Assets/Scripts/Puzzle/Gate.cs` (swings on `OnPressurePlateChanged`, `_invertLogic`, `_blocker`), `Assets/Scripts/Puzzle/PressurePlate.cs` (`_oneShot` latch, `_lockable`, `Init()`), `Assets/Scripts/Puzzle/Hazard.cs`
- Character abilities: `Assets/Scripts/Characters/ScarletController.cs` (`OnControllerColliderHit` boulder push, `TryLiftDani` / `ReleaseDani`), `Assets/Scripts/Characters/DaniController.cs` (`TryActivateSwitch`, `BeginLiftedState` / `EndLiftedState`, `TryPickUpObject`)
- Sequence: `Assets/Scripts/Core/LevelSequenceController.cs` (`_levelScenes` to extend to 8 entries in Task 8 — this field lives on the `DontDestroyOnLoad` GameObject in `World1_Puzzle1.unity`)
- Existing levels to reference: `Assets/ScriptableObjects/Levels/World1_Puzzle5.asset` (multi-height construction), `Assets/ScriptableObjects/Levels/World1_Puzzle3.asset` (boulder + pressure plate pattern)
- Existing scenes to duplicate: `Assets/Scenes/World1_Puzzle6.unity` (for Task 2), `Assets/Scenes/World1_Puzzle7.unity` (for Task 5)

---

## Ready to hand to `unity-senior-developer`

- **Tasks 2 and 5** (scene creation by duplication) are mechanical — hand off immediately after authorizing.
- **Task 1** (tunnel-to-elevated-exit spike) is a focused data/play-mode spike with a clear binary outcome; good agent task. Its result (exit construction details) must be documented before Task 3 begins.
- **Tasks 3 and 6** (LevelData authoring) follow the proven pattern from Puzzles 3–6 and can be handed off once Tasks 1 and 2 (for Task 3) and Task 5 (for Task 6) are complete. The spatial layout decisions in this document are the authoritative spec.
- **Task 8** (`_levelScenes` + Build Settings) is a quick Inspector edit — hand off after Tasks 3 and 6 are stable.
- **Tasks 4, 7, and 9** (verification, no-bypass checks, hazards + coins) benefit from hands-on play-mode judgment (Louie) with the agent assisting on any data-side tuning (positions, speeds, gap widths).

# Sprint 02 — Puzzles 3 and 4 Playable

**Goal:** Advance the World 1 difficulty curve by making Puzzle 3 (The Boulder) and Puzzle 4 (The Tunnel) fully playable end-to-end, each requiring genuine cooperation — neither character can finish alone.
**Theme:** Scarlet's strength and Dani's size introduced as puzzle-solving superpowers
**Status:** Planned

---

## Why this sprint

Sprint 01 delivered the warmest two puzzles: a flat handshake (Puzzle 1) and the first separation beat (Puzzle 2). The player now understands switching and has seen Dani enable Scarlet. Sprint 02 introduces the two mechanics the rest of World 1 is built on:

- **Puzzle 3** is the first time `PressurePlate` + `Gate` + `PushBoulder` all interact in a real puzzle. All three objects are coded and prefab-ready; this sprint proves they work in a designed scenario, not just a throwaway test.
- **Puzzle 4** introduces the idea that Dani's small size is a *strength* — she can go places Scarlet physically cannot. The crawl animation does not exist yet, but the puzzle beat can be delivered *now* by geometry alone: a passage whose walls are too close together for Scarlet's `CharacterController` capsule to enter.

Both puzzles must also be wired into the existing `LevelSequenceController` so the four-puzzle sequence (1 → 2 → 3 → 4 → MainMenu) is playable in one sitting.

---

## Key design decisions captured here

### Puzzle 3 — the "pit" is flat

The design doc shows the boulder rolling into a side pit that contains a pressure plate. There is no multi-height terrain in the current build — no ramps, no ledges below floor level. The pit is therefore authored as a **dead-end side corridor off the main path**: the boulder rolls into it, lands on the `StonePressurePlate`, and the linked `Gate` opens on the main path. From the isometric camera, a walled alcove reads exactly like a pit. This avoids any new terrain work and matches the puzzle beat precisely.

### Puzzle 4 — geometry-only "Dani only" passage (no crawl animation)

`DaniController` has no crawl state. Two implementation paths exist:

**Option A — Minimal crawl state:** Add a crawl bool to `DaniController`, swap in a smaller `CapsuleCollider`, play a placeholder idle animation. Adds code and animation work; the placeholder animation may look wrong.

**Option B — Geometry-only narrow passage (chosen):** Make the tunnel corridor ~0.45 m wide in world space, bordered by `BoxCollider` walls. Dani's `CharacterController` radius is 0.2 m (0.4 m diameter — fits). Scarlet's `CharacterController` radius is 0.3 m (0.6 m diameter — does not fit). No new code, no new animation. The prefab `CrawlTunnelEntrance.prefab` already exists as a visual and can mark each end of the passage.

**Recommendation: Option B.** It delivers the Puzzle 4 beat without touching character code, keeps the sprint focused on puzzle authoring, and defers the crawl animation to a sprint where art work is also planned. If Scarlet visually clipping against the tunnel walls is noticeable, add a short `Debug.Log` message to confirm she is colliding, but do not fix it visually this sprint.

Option A (crawl state) is explicitly out of scope here — flag it for the Sprint 03 or art sprint if the visual gapping is unacceptable to Louie in playtest.

---

## Success criteria (sprint definition of done)

- From Main Menu, the player can play Puzzle 1 → Puzzle 2 → Puzzle 3 → Puzzle 4 in sequence without a manual scene change or editor intervention.
- **Puzzle 3:** Scarlet pushes the boulder into the alcove. The boulder lands on the pressure plate. The gate opens. Both characters walk through and trigger the reunion. Neither character can open the gate without the boulder on the plate.
- **Puzzle 4:** Dani walks through the narrow passage; Scarlet is blocked by the walls. Dani reaches the lever, throws it, the gate opens. Scarlet walks through. Both reunite. Neither character can complete the puzzle alone.
- Failure (character enters a hazard) returns both siblings to the puzzle's checkpoint in both new puzzles; coin respawn still works.
- Hints load correctly for each new puzzle — Puzzle 3 hints are Puzzle 3 text, Puzzle 4 hints are Puzzle 4 text.
- `LevelSequenceController._levelScenes` includes all four puzzle scene names in order; completing Puzzle 4 returns to MainMenu.

---

## Backlog

| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | Create `World1_Puzzle3` scene by duplicating `World1_Puzzle2`. Strip Puzzle 2's LevelData reference and objects; wire in a new `World1_Puzzle3` LevelData asset. | S | Scene opens in play mode with empty ground, both characters spawn, and no Puzzle 2 objects are present. | — |
| 2 | Author `World1_Puzzle3` LevelData: Scarlet + Dani start on the same side of a closed gate, boulder in front of them, a walled alcove to the side containing a `StonePressurePlate` (id "Boulder_Plate"), the gate linked to that same id, and a `ReunionTrigger` on the far side of the gate. Place a `Checkpoint` at puzzle entry. Add 3 progressive hints. | M | In play mode: Scarlet pushes the boulder into the alcove; it lands on the plate; the gate swings open; both characters walk through and the reunion fires. Standing Dani on the plate does not open the gate (plate only responds to the boulder mass — achieve this by sizing the plate trigger so only the boulder's collider overlaps it, not a standing character). | Task 1 |
| 3 | Verify Puzzle 3 completability and "neither alone" rule: confirm Dani cannot push the boulder; confirm Scarlet cannot pass the gate before the plate is active; confirm the puzzle is solvable by one person switching. Document any tuning needed on `_pushForce` or plate trigger sizing. | S | Puzzle 3 solves in one sitting by switching Scarlet → push boulder → plate activates → both walk through. Dani walking into the boulder does not move it. | Task 2 |
| 4 | Create `World1_Puzzle4` scene by duplicating `World1_Puzzle3`. Strip Puzzle 3 objects; wire a new `World1_Puzzle4` LevelData asset. | S | Scene opens in play mode with empty ground, both characters spawn, no Puzzle 3 objects present. | Task 1 (same pattern) |
| 5 | Author `World1_Puzzle4` LevelData: narrow passage alongside the main path, walled with geometry so the gap is ~0.45 m wide (fits Dani, blocks Scarlet). Place `CrawlTunnelEntrance.prefab` at each opening as visual markers. On the far side of the passage, place a `Lever` (id "Tunnel_Lever", oneShot true). On the main path, place a closed `Gate` linked to "Tunnel_Lever". Place `ReunionTrigger` beyond the gate. Place a `Checkpoint` at puzzle entry. Add 3 progressive hints. | M | In play mode: Dani walks through the passage; Scarlet is physically blocked. Dani throws the lever; gate opens; Scarlet walks through; both reunite. | Task 4 |
| 6 | Verify Puzzle 4 completability and "neither alone" rule: confirm Scarlet cannot enter the passage (CharacterController collides with passage walls); confirm Dani cannot open the gate from the Scarlet side (lever is only reachable after traversing the passage); confirm single-player solve by switching. | S | Puzzle 4 solves by switching to Dani → navigate passage → throw lever → switch to Scarlet → walk through gate → both reunite. Scarlet pressing against passage entrance does not enter. | Task 5 |
| 7 | Extend `LevelSequenceController._levelScenes` to include all four puzzle scenes in order: `World1_Puzzle1`, `World1_Puzzle2`, `World1_Puzzle3`, `World1_Puzzle4`. Ensure Puzzle 4 completion returns to MainMenu (already the fallback when `_currentIndex` exceeds the array length). | S | Playing from MainMenu proceeds through all four puzzles in order; completing Puzzle 4 returns to MainMenu without a dead end or error. | Tasks 2, 5 |
| 8 | Add a hazard (fall-off boundary or `Hazard` trigger) to each new puzzle and confirm failure → checkpoint → respawn flow works. Place coins (3–5 per puzzle) within the new levels. | S | Triggering a hazard in Puzzle 3 or 4 sends both characters back to that puzzle's `Checkpoint`. Spending coins respawns in place. Collecting all coins in each puzzle increments the coin counter correctly. | Tasks 2, 5 |

---

## Out of scope

- **Dani crawl animation and crawl state** — Puzzle 4 uses geometry-only width enforcement. The crawl animation, a smaller `CapsuleCollider` swap, and any animator bool are deferred. Revisit in a dedicated art + animation sprint if Louie decides the visual is important before launch.
- **Pressure plate "character weight" distinction** — keeping the plate active only when the boulder (not Dani) stands on it is solved by spatial sizing in Task 2, not by tag-filtering code. Tag-based weight filtering is a future enhancement.
- **Puzzle 5 (The Lift)** — that puzzle requires Scarlet to lift Dani to an elevated ledge, which needs the first multi-height terrain in any real puzzle. Out of scope here; planned for Sprint 03.
- **Stacking mechanic as a puzzle element** — `DaniController.TryPickUpObject` and `DropObject` are coded but no sprint-02 puzzle requires them.
- **New art from Meshy/Blender** — both puzzles use existing prefabs and platform types from the `LevelPrefabRegistry`. No new props are needed.
- **Scarlet "worried glance" animation** — the design doc notes Scarlet should visually react when Dani enters the tunnel. This is a polish beat, not a gameplay requirement. Deferred to an animation/art polish sprint.
- **Secret keys** — collectible design is unchanged; no key is hidden in Puzzles 3 or 4 this sprint.

---

## Risks & assumptions

- **Boulder + pressure plate sizing is the biggest unknown.** `StonePressurePlate` uses a 3D trigger collider — its size must be tuned so the boulder's `Rigidbody` collider overlaps it reliably after rolling, but a standing character does not accidentally activate it. If the plate is too large, Dani can stand on it and bypass the puzzle. If it is too small, the boulder may stop short and not register. Expect one round of in-play tuning (Task 3). If the plate really cannot be sized to exclude characters, the fallback is to add a layer check inside `PressurePlate.OnTriggerEnter` — but do not schedule that code change unless the sizing approach definitively fails.
- **Tunnel passage construction.** The "narrow corridor" in Puzzle 4 must be built from `LevelPrefabRegistry` platform types — specifically, closely-spaced wall segments with `StoneWall` or `AncientRuinsWall` prefabs. The exact spacing needs to be calibrated in-engine against the actual `CharacterController` radius values for both characters (confirm in the Inspector on the Scarlet and Dani GameObjects). Task 6 is the validation gate.
- **`CharacterController` radius values are assumed, not confirmed from code.** The prompt states Scarlet radius 0.3 m and Dani radius 0.2 m. Verify these in the Inspector before committing to the 0.45 m passage width. If the radii differ, adjust the passage width accordingly (target: Dani diameter + ~5 cm clearance, Scarlet diameter - ~5 cm short of fitting).
- **`LevelSequenceController` scene array is hardcoded.** The `_levelScenes` field is serialized in the Inspector — extending it to four entries (Task 7) requires editing the Inspector value on the `LevelSequenceController` GameObject in the `World1_Puzzle1` scene (where it lives as a `DontDestroyOnLoad` singleton). Do not add a fifth scene accidentally; ensure the Puzzle1 scene is the one that owns this component.
- **Assumption: `World1_Puzzle3` and `World1_Puzzle4` scenes can be built by duplicating the existing puzzle scenes.** Unity build settings must include all four scenes or the `SceneManager.LoadScene` call will fail at runtime. Task 7 should include a quick build-settings check.
- **Boulder physics in the isometric build.** Sprint 01 Task 5 was meant to verify that `ScarletController.OnControllerColliderHit` + `PushBoulder` + `StonePressurePlate` interact correctly. If that verification was completed and passing, Puzzle 3 is low-risk. If that task was skipped or failed, Puzzle 3 carries physics-debugging risk and Task 3 becomes larger. Confirm before starting Task 2.

---

## References

- Design: `README-Puzzle-Design.md` — Puzzle 3 "The Boulder", Puzzle 4 "The Tunnel", Design Rules, Difficulty Curve table
- Design: `README-Design-Feedback.md` — Reunion win condition, Failure/Checkpoint, Coin system, "neither solves it alone" rule
- Existing puzzle scenes to duplicate: `Assets/Scenes/World1_Puzzle1.unity`, `Assets/Scenes/World1_Puzzle2.unity`
- Existing level assets to copy structure from: `Assets/ScriptableObjects/Levels/World1_Puzzle1.asset`, `World1_Puzzle2.asset`
- Core systems: `Assets/Scripts/Core/LevelBuilder.cs`, `Assets/Scripts/Data/LevelData.cs`, `Assets/Scripts/Data/LevelObjectData.cs` (enum: `PressurePlate` is not in `LevelObjectType` — it is a platform/prop placed via `PlatformDef` or as a prefab prop, not a `LevelObjectType` entry; confirm the correct spawn path before authoring the LevelData)
- Puzzle objects: `Assets/Scripts/Puzzle/PressurePlate.cs`, `Assets/Scripts/Puzzle/Gate.cs`, `Assets/Scripts/Puzzle/Lever.cs`, `Assets/Scripts/Puzzle/Bridge.cs`
- Character abilities: `Assets/Scripts/Characters/ScarletController.cs` (`OnControllerColliderHit` for boulder push), `Assets/Scripts/Characters/DaniController.cs` (`TryActivateSwitch` for lever interaction)
- Props: `Assets/Prefabs/Props/StonePressurePlate.prefab`, `Assets/Prefabs/Props/PushBoulder.prefab`, `Assets/Prefabs/Env/CrawlTunnelEntrance.prefab`
- Puzzle prefabs: `Assets/Prefabs/Puzzle/Gate.prefab`, `Assets/Prefabs/Puzzle/Lever.prefab`
- Sequence: `Assets/Scripts/Core/LevelSequenceController.cs` (the `_levelScenes` array to extend in Task 7)

---

## Ready to hand to `unity-senior-developer`

- **Tasks 1 and 4** (scene creation by duplication) are mechanical and well-specified — hand off immediately.
- **Tasks 2 and 5** (LevelData authoring) follow the exact same pattern as `World1_Puzzle1.asset` and `World1_Puzzle2.asset`; hand off with the note that `StonePressurePlate` spawn path needs confirmation (see References note above).
- **Task 7** (extending `_levelScenes` + build settings) is a quick Inspector edit plus a build settings check — hand off after Tasks 2 and 5 are stable in play mode.
- **Tasks 3 and 6** (validation and tuning) are best done by Louie in play mode since they require hands-on input and visual judgment; the developer agent can assist if specific values need code-side changes.
- **Task 8** (hazards + coins) is self-contained and can be handed off in parallel with Tasks 3 and 6.

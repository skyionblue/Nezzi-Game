# Sprint 03 — Puzzles 5 and 6 Playable

**Goal:** Make Puzzle 5 (The Lift) and Puzzle 6 (The Trust) fully playable end-to-end and wire them into the sequence, proving the first multi-height terrain and the lift mechanic (Puzzle 5) and the "act without seeing your partner" beat via cross-linked lever/gate pairs (Puzzle 6).
**Theme:** Vertical cooperation (Scarlet enables Dani) and trust across a divide
**Status:** Planned

---

## Why this sprint

Sprints 01 and 02 delivered four flat-plane puzzles that established switching, separation, boulder+plate, hold-and-cross, and the geometry-only "only one sibling fits" idea. Every level so far lives on a single XZ plane at y=0. Sprint 03 pushes the difficulty curve into two beats the design leans on:

- **Puzzle 5 (The Lift)** is the first *vertical* gameplay in the game. A ledge blocks the path — too tall for Dani to reach alone, and the ledge beyond is too narrow for Scarlet. Scarlet lifts Dani onto the ledge; Dani runs across the top to a lever; the lever extends a bridge below for Scarlet; Scarlet crosses; they reunite. This proves two unproven things at once: **multi-height terrain** (a raised platform a character can stand and walk on) and the **lift mechanic** (`ScarletController.TryLiftDani` / `DaniController.BeginLiftedState`), which is fully coded but has never run in a real puzzle in the isometric build. This is the risk-bearing half of the sprint.

- **Puzzle 6 (The Trust)** is the low-code half. A wall divides the siblings; each has a gate that only opens from a lever on the *other's* side. Scarlet pulls her lever, which opens Dani's gate; Dani walks through and pulls her lever, which opens Scarlet's gate; they find each other. This is two `Lever` → `Gate` pairs, cross-wired by `id`, plus a floor-gap divide so they genuinely cannot reach each other except through the gates. **No new code** — it is built entirely from proven `Lever` + `Gate` + floor-gap mechanics.

Both puzzles must be wired into `LevelSequenceController` so the six-puzzle sequence (1 → 2 → 3 → 4 → 5 → 6 → MainMenu) is playable in one sitting.

---

## Key design decisions captured here

### Puzzle 5 — decide multi-height terrain BEFORE authoring the puzzle

There is no multi-height terrain in the build yet. Every level is a flat plane at y=0 (floor tiles at y=-1.5, scale 0.5, top surface at y=0). `StepHeightController` (stepHeight 0.6) lets characters walk up *small* ledges but not a full standable ledge. How to build a raised platform a character can stand ON and walk ACROSS is unproven.

`PlatformDef` already carries a full `Vector3 position` and `Vector3 scale`, so the raised ledge can be authored data-side by placing floor-tile platforms at a higher Y (e.g. a stack/slab whose top surface sits ~1.5–2 m above the ground plane) — **no new prefab or code should be needed if this works.** Task 1 is a spike to prove exactly that before any Puzzle 5 authoring begins. If a bare stack of floor tiles does not read or collide well, the fallback is a dedicated raised-platform prefab authored the same data-driven way; note it but prefer proving the stacked-tile approach first.

### Puzzle 5 — prove the lift mechanic; have a fallback ready but prefer the lift

`TryLiftDani` parents Dani to Scarlet at `_liftOffset` (0, 1.2, 0) and there is a `_liftLaunchForce` for a jump-off, but none of this has run in the isometric camera. The open question is whether, after Scarlet walks Dani up to the ledge and releases her, **Dani reliably ends up standing ON the ledge** (not clipping, not falling back down). Task 2 is a dedicated verification of the lift against the raised platform from Task 1.

The design hinges on the lift, so **prefer proving it.** Only if Task 2 shows the lift is unreliable/unshippable in the isometric build, fall back to Dani reaching the ledge another proven way — a **Dani-only ramp** (`RPGRampCovered` platform, narrow enough that Scarlet's capsule cannot follow, echoing the Puzzle 4 width technique) or **stacked crates** using the coded stacking mechanic. Record which path was taken and why; do not silently swap the design.

### Puzzle 6 — no new code, cross-wired trigger→gate pairs + a floor-gap divide

**DECIDED (confirmed by Louie): Scarlet's side uses a PRESSURE PLATE, not a lever.** Levers are Dani-only in the current code (see Risks), so rather than add lever support to `ScarletController`, Scarlet triggers her side by stepping on a `StonePressurePlate` (already proven, zero new code). Dani's side keeps a `Lever`. Both read the same to the player — each sibling activates something that opens the *other's* gate.

Puzzle 6 is built from existing mechanics only:

- **Scarlet's side:** a `StonePressurePlate` with `id = "Scarlet_Plate"`. A closed `Gate` on Dani's side linked to `id = "Scarlet_Plate"`. **Use `oneShot`/lock behavior so the gate stays open after Scarlet triggers it** — otherwise the gate closes when Scarlet steps off the plate to go find Dani. (If the plate can't latch, keep Scarlet on the plate until Dani is through her gate, then the sequence still resolves — but prefer a latched/one-shot open so Scarlet is free to move.)
- **Dani's side:** a `Lever` with `id = "Dani_Lever"`, `oneShot true`. A closed `Gate` on Scarlet's side linked to `id = "Dani_Lever"`.
- **The divide:** a dividing wall/void down the middle enforced by the **floor-gap choke technique** — the only floor tiles crossing the divide are the two gate tiles; everywhere else the divide is open air over a `FallHazard`, so nobody can walk around a gate.

Because audio is out of scope this sprint, the **visible gate swinging open is the feedback** that substitutes for the design's sound cue on the partner's side. The "can't see each other" beat comes from the physical wall and the isometric framing; no fog/vision code is added.

### Both puzzles — keep the proven no-bypass and safety patterns

- Enforce every gate with the **floor-gap choke point** (only the gate's tile crosses the divide; off-floor is a `FallHazard` BoxCollider at y=-5), and **verify with downward physics raycasts** along the divide (raycast at x offsets across the divide z; only the gate tile should hit floor). Do NOT rely on wall prefab colliders alone to seal a side.
- Reuse the working `CheckpointManager` / `FallHazard` / coins / hints pattern in both new levels, exactly as Puzzles 3 and 4 do.

---

## Success criteria (sprint definition of done)

- From Main Menu, the player can play Puzzle 1 → 2 → 3 → 4 → 5 → 6 in sequence without a manual scene change or editor intervention; completing Puzzle 6 returns to MainMenu.
- **Multi-height terrain proven:** a character can be placed on a raised platform and walk across it and up/onto it via the chosen method, verified in play mode (Task 1).
- **Puzzle 5:** Scarlet lifts Dani onto the high ledge; Dani walks across the ledge to the lever and throws it; a bridge extends below for Scarlet; Scarlet crosses; both reunite. Neither character can complete it alone — Scarlet cannot reach the ledge/lever, and Scarlet cannot cross until the bridge extends.
- **Puzzle 6:** Scarlet steps on her pressure plate, opening Dani's gate (gate visibly swings); Dani walks through and throws her lever, opening Scarlet's gate; both cross and reunite. Neither can open their own gate; walking off the floor to bypass a gate drops the character into the fall hazard.
- Failure (character enters a hazard / falls off) returns both siblings to that puzzle's checkpoint in both new puzzles; coin respawn still works.
- Hints load correctly per puzzle — Puzzle 5 hints are Puzzle 5 text, Puzzle 6 hints are Puzzle 6 text.
- `LevelSequenceController._levelScenes` lists all six puzzle scenes in order.

---

## Backlog

| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | **SPIKE — multi-height terrain.** Prototype a raised platform authored data-side (floor-tile `PlatformDef`s placed at higher Y, and/or a `RPGRampCovered` slope) and confirm a character can (a) stand on the raised surface and (b) walk across its top without falling through or jittering. Do this in a throwaway/test LevelData, not in a puzzle scene yet. Record the chosen construction (tile stack height, scale, whether a ramp is needed) so Puzzle 5 can reuse it. | M | In play mode, a character stands still on the raised platform (does not sink/fall) and walks the full length of its top surface. The exact Y/scale values that worked are written down for reuse. | — |
| 2 | **SPIKE — verify the lift mechanic against the raised platform.** Using the Task 1 platform, verify `Scarlet.TryLiftDani` → walk Dani up beside/onto the ledge → `ReleaseDani`, and confirm Dani reliably ends up STANDING ON the ledge (not clipping, not falling back). Test the `_liftOffset` / `_liftLaunchForce` behavior in the isometric camera. If unreliable, document the specific failure and pick the fallback (Dani-only ramp or stacked crates) per the Key Decisions. | M | Either: Scarlet lifts Dani and Dani ends up controllably standing on the raised ledge in play mode (preferred outcome), OR a documented decision to use the ramp/crate fallback with the reason. | Task 1 |
| 3 | Create `World1_Puzzle5` scene by duplicating an existing puzzle scene; strip prior objects; wire in a new `World1_Puzzle5` LevelData asset. | S | Scene opens in play mode with both characters spawning and no leftover objects from the duplicated scene. | — |
| 4 | Author `World1_Puzzle5` LevelData: a lower ground path for Scarlet ending at a floor gap; a raised ledge (from Task 1's proven construction) reachable only by the lift (or the Task 2 fallback); a `Lever` on top of the ledge (`id = "Bridge_Lever"`, `oneShot true`); a `Bridge` (type 8) below linked to `id = "Bridge_Lever"` that extends across Scarlet's gap; a `ReunionTrigger` on the far side; a `Checkpoint` at entry; 3 progressive hints. | M | In play mode: Scarlet lifts Dani onto the ledge; Dani crosses the ledge top and throws the lever; the bridge extends across Scarlet's gap; Scarlet crosses; both reunite. Scarlet cannot reach the lever, and cannot cross before the bridge extends. | Tasks 2, 3 |
| 5 | Verify Puzzle 5 completability and "neither alone" rule. Confirm Scarlet cannot reach the ledge/lever; confirm Scarlet cannot cross the gap before the bridge; confirm one player switching solves it. Tune `_liftOffset`, `_liftRange`, bridge span, and gap width as needed. | S | Puzzle 5 solves in one sitting: switch to Scarlet → lift Dani onto ledge → switch to Dani → cross ledge → throw lever → switch to Scarlet → cross bridge → both reunite. Dani cannot reach the ledge without being lifted (or via the intended fallback). | Task 4 |
| 6 | Create `World1_Puzzle6` scene by duplicating an existing puzzle scene; strip prior objects; wire in a new `World1_Puzzle6` LevelData asset. | S | Scene opens in play mode with both characters spawning and no leftover objects. | — |
| 7 | Author `World1_Puzzle6` LevelData: a central dividing wall/void with Scarlet on one side, Dani on the other. **Scarlet's `StonePressurePlate` (`id = "Scarlet_Plate"`) opens Dani's `Gate` (linked to "Scarlet_Plate")** — latch it open (oneShot/lock) so it stays open when Scarlet leaves the plate; Dani's `Lever` (`id = "Dani_Lever"`, oneShot) opens Scarlet's `Gate` (linked to "Dani_Lever"). Enforce the divide with the **floor-gap choke** (only the two gate tiles cross the divide; the rest is open air over a `FallHazard`). Place a `ReunionTrigger` where the two sides meet after both gates open; a `Checkpoint` at entry; 3 progressive hints. | M | In play mode: Scarlet steps on her plate → Dani's gate visibly swings open and stays open → Dani walks through and throws her lever → Scarlet's gate swings open → both cross and reunite. Neither gate opens from its own side's trigger. | Task 6 |
| 8 | Verify Puzzle 6 no-bypass with **downward raycasts along the divide**: raycast at x offsets across the divide z and confirm only the two gate tiles hit floor; everywhere else the raycast misses (fall into hazard). Confirm a character walking off the floor to bypass a gate drops into the `FallHazard` and respawns at the checkpoint. Confirm single-player solve by switching. | S | Raycast sweep shows floor only at the two gate tiles. Attempting to walk around a gate drops the character to the checkpoint. Puzzle 6 solves by switching Scarlet ↔ Dani in the intended order. | Task 7 |
| 9 | Extend `LevelSequenceController._levelScenes` to include all six puzzle scenes in order (`World1_Puzzle1` … `World1_Puzzle6`) and add both new scenes to Build Settings. Confirm Puzzle 6 completion returns to MainMenu. | S | Playing from MainMenu proceeds through all six puzzles in order; completing Puzzle 6 returns to MainMenu without a dead end or error. Both new scenes are in Build Settings. | Tasks 4, 7 |
| 10 | Add hazards (fall-off boundary / `Hazard` trigger) and coins (3–5 per puzzle) to both new levels and confirm failure → checkpoint → respawn and coin spend/respawn flows work. | S | Triggering a hazard in Puzzle 5 or 6 returns both characters to that puzzle's `Checkpoint`. Spending coins respawns in place. Collecting all coins increments the counter correctly per puzzle. | Tasks 4, 7 |

---

## Out of scope

- **Reviving the vertical-climb rope/vine mechanic.** `DaniController.SetClimbingState` stays a stub. Puzzle 5's height is delivered by the lift (or a ramp/crate fallback), never by a rope climb.
- **Audio / the Puzzle 6 sound cue.** Sound design is out of current scope; the visible gate swing is the feedback that substitutes for the audio cue on the partner's side. Do not add audio systems this sprint.
- **Vision / fog-of-war "can't see the partner" system.** The "act without seeing" beat is delivered by the physical dividing wall and isometric framing, not by any camera/vision code.
- **A general reusable raised-platform prefab** beyond what Task 1 needs. If stacked floor tiles work, do not build a new prefab; if a prefab is required, build the minimum for Puzzle 5, not a generic terrain toolkit.
- **The stacking mechanic as a first-class puzzle element** — only used as a *fallback* in Puzzle 5 if the lift fails (Task 2). No standalone stacking puzzle is planned here.
- **Lift-launch/jump-off tuning as a feature** — only tuned as far as Puzzle 5 needs (Task 5). A polished lift-and-throw feel is deferred.
- **New Meshy/Blender art** — both puzzles use existing platform types and prefabs from `LevelPrefabRegistry`.

---

## Risks & assumptions

- **Multi-height terrain is the single biggest unknown in this sprint.** No standable raised platform exists yet in any level. Task 1 is a hard gate before Puzzle 5 authoring: if a data-authored tile stack does not collide/read cleanly, the fallback is a dedicated raised-platform prefab, which adds asset work. Do not start Task 4 until Task 1 proves a construction.
- **The lift mechanic is unverified in the isometric build.** `TryLiftDani`/`BeginLiftedState` parent Dani at `_liftOffset` and disable her `CharacterController`, but whether she ends up standing ON a ledge after release — versus clipping into it or falling back — has never been observed. Task 2 gates this; the ramp/crate fallback is pre-authorized if it fails, but the lift is preferred because the design hinges on it.
- **Levers are Dani-only — RESOLVED.** `DaniController.TryActivateSwitch` is the only path that fires an `IInteractable`; `ScarletController` has no switch-activation. Louie confirmed the **pressure-plate reframe**: Scarlet's side uses a `StonePressurePlate` (no new code), Dani's side uses a `Lever`. The one thing to watch is latching — Scarlet's gate must stay open after she steps off the plate (use oneShot/lock behavior on the gate or plate), or the sequence stalls when Scarlet leaves the plate to reunite. No code addition to `ScarletController` is needed.
- **Bridge extension direction and span.** The `Bridge` (type 8) currently drops from above onto its `plateId` (as used in Puzzle 2). Confirm it can present as "extends across Scarlet's gap" from the isometric camera; if the drop-from-above read is wrong for a horizontal span, tune its open offset/orientation (`gateOpenOffset`, `yRotation`) or accept the drop-in read (it preserves the beat: a walkable platform appears across the gap).
- **Floor-gap raycast verification must be redone per level.** Wall prefab colliders are unreliable for sealing; the divide in both puzzles must be verified with the downward-raycast sweep (Task 8, and applied to Puzzle 5's gap too). Do not assume walls block passage.
- **`LevelSequenceController._levelScenes` is a serialized array on the `DontDestroyOnLoad` singleton in the `World1_Puzzle1` scene.** Extending it to six entries (Task 9) is an Inspector edit on that specific object; also add both new scenes to Build Settings or `SceneManager.LoadScene` fails at runtime.
- **Assumption:** new scenes can be created by duplicating an existing puzzle scene (Tasks 3, 6), matching how Sprint 02 built Puzzles 3 and 4.

---

## References

- Design: `README-Puzzle-Design.md` — Puzzle 5 "The Lift", Puzzle 6 "The Trust", Design Rules, Difficulty Curve
- Design: `README-Design-Feedback.md` — Reunion win condition, Failure/Checkpoint, Coin/Hint systems, "neither solves it alone" rule
- Standing preference (bridges over vertical climb): the rope/vine climb stays a stub; Puzzle 5 uses the lift, not a climb.
- Core systems: `Assets/Scripts/Core/LevelBuilder.cs`, `Assets/Scripts/Data/LevelData.cs` (`PlatformDef.position`/`scale` for raised tiles), `Assets/Scripts/Data/LevelObjectData.cs` (`LevelObjectType` enum — `Bridge = 8`, `Lever = 3`, `Gate = 2`, `StonePressurePlate = 10`), `Assets/ScriptableObjects/LevelPrefabRegistry.asset`
- Lift mechanic: `Assets/Scripts/Characters/ScarletController.cs` (`TryLiftDani`, `ReleaseDani`, `_liftOffset`, `_liftRange`, `_daniLayer`, `_liftLaunchForce`), `Assets/Scripts/Characters/DaniController.cs` (`BeginLiftedState`, `EndLiftedState`, stacking `TryPickUpObject`/`DropObject`, Dani-only `TryActivateSwitch`)
- Puzzle objects: `Assets/Scripts/Puzzle/Lever.cs`, `Assets/Scripts/Puzzle/Gate.cs`, `Assets/Scripts/Puzzle/PressurePlate.cs`, `Assets/Scripts/Puzzle/Bridge.cs`, `Assets/Scripts/Puzzle/Hazard.cs`
- Sequence: `Assets/Scripts/Core/LevelSequenceController.cs` (`_levelScenes` to extend in Task 9)
- Existing levels to copy structure from: `Assets/ScriptableObjects/Levels/World1_Puzzle3.asset`, `Assets/ScriptableObjects/Levels/World1_Puzzle4.asset`
- Existing scenes to duplicate: `Assets/Scenes/World1_Puzzle3.unity`, `Assets/Scenes/World1_Puzzle4.unity`

---

## Ready to hand to `unity-senior-developer`

- **Tasks 3 and 6** (scene creation by duplication) are mechanical — hand off immediately.
- **Task 1** (terrain spike) is a good developer-agent task: it is self-contained data authoring plus a play-mode observation, and its output (working Y/scale values) unblocks Puzzle 5.
- **Task 4** (Puzzle 5 LevelData) follows the proven Puzzle 3/4 pattern but must wait on Task 2's lift-vs-fallback outcome. **Task 7** (Puzzle 6 LevelData) is unblocked — the Scarlet-plate decision is made (pressure plate, no new code); hand off after Task 6.
- **Task 9** (`_levelScenes` + Build Settings) is a quick Inspector + build-settings edit — hand off after Tasks 4 and 7 are stable.
- **Tasks 2, 5, and 8** (lift verification, completability tuning, raycast no-bypass check) are best done by Louie in play mode with the agent assisting on any code-side value changes, since they require hands-on input and visual judgment.
- **Task 10** (hazards + coins) is self-contained and can run in parallel with Tasks 5 and 8.

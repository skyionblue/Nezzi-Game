# Sprint 05 — Puzzles 9 and 10 Playable (World 1 Finale)

**Goal:** Make Puzzle 9 (The Long Way Around) and Puzzle 10 (World 1 Finale) fully playable end-to-end, using multi-height terrain as the structural reason each puzzle works and combining three or more proven mechanics in every room — the hardest puzzles in World 1.
**Theme:** Multi-step problem solving across elevations (Puzzle 9) / greatest-hits combination under spatial complexity (Puzzle 10)
**Status:** Planned

---

## Why this sprint

Sprints 01–04 delivered eight puzzles and proved every mechanic in the World 1 set: boulder push, pressure plate, lever+gate, bridge (drops from above), Scarlet-lifts-Dani onto a 1.0 m ledge, cross-wired lever/gate pairs, simultaneous hold-and-cross with a held pressure plate, and multi-height terrain as a structural divide. The lift mechanic is polished (`liftOffset (0, 1.6, 0.3)`, 0.5 m forward nudge, upward velocity on release).

Sprints 03's developer feedback was direct: Puzzles 5 and 6 felt too flat. Sprint 04 corrected this and multi-height terrain is now the proven baseline. Puzzles 9 and 10 must go further — not just one elevated section per room, but layouts where multiple elevations interact with each other and where the solution requires tracking which character is where across all heights simultaneously.

**Puzzle 9 (The Long Way Around)** is the first puzzle designed to make the player genuinely pause. The design doc describes "the answer isn't immediately obvious" — the "obvious" solution fails because a gate is on the wrong side, and the player must first reroute the problem by sending one character high before the other can act at ground level. This sprint redesigns the original "Dani climbs to the high lever" beat (which relied on the rope/vine climb stub) as a **Scarlet-lifts-Dani-onto-the-upper-ledge** beat — the same spatial and emotional moment, delivered through the proven lift mechanic.

**Puzzle 10 (World 1 Finale)** is a "greatest-hits" room that the design doc explicitly calls a combination of all World 1 skills. The original design spec depended on two unavailable systems: rope/vine vertical climb (`DaniController.SetClimbingState` is a stub with no implementation) and `CrawlTunnelEntrance` (has no `LevelObjectType`, no `LevelPrefabRegistry` entry, and no `LevelBuilder` case — it cannot be authored data-side at all). This sprint redesigns Puzzle 10 from the ground up using only the proven data-driven mechanic set, while preserving every emotional beat from the spec: Scarlet enables Dani to reach a high path, Dani acts first on the far side to enable Scarlet, Scarlet crosses and joins Dani, boulder pushes onto a plate, a gate opens, and both walk through together in the finale. The story beat — both characters stand at the edge of the forest and see the direction home — is preserved.

Both puzzles must complete the ten-puzzle World 1 sequence. Completing Puzzle 10 returns to MainMenu.

---

## Key design decisions captured here

### What is NOT available — hard constraints

- **Rope/vine vertical climb:** `DaniController.SetClimbingState` is a stub. No climb behavior executes. Do not plan any puzzle beat that requires a character to ascend a rope or vine. Not in Puzzle 9, not in Puzzle 10.
- **CrawlTunnelEntrance:** This prefab has no `LevelObjectType` value, no entry in `LevelPrefabRegistry`, and no `case` in `LevelBuilder.PlaceObjects()`. It cannot be spawned by the data-driven pipeline. Do not plan any puzzle beat that requires a crawl tunnel. If Dani needs to access a small passage, use a **geometry-only width restriction** (a gap in the floor platform layout whose XZ span is narrower than Scarlet's capsule but wide enough for Dani's capsule) — this is how Puzzle 4's passage was handled. The narrow-gap technique requires no new prefab or code.
- **Stacking mechanic as a puzzle element:** `DaniController.TryPickUpObject` is coded and works, but no puzzle in the sequence has used it yet. Introducing it for the first time in the hardest two puzzles violates design rule 4 ("one new mechanic per puzzle — later puzzles combine previous ideas"). The stacking mechanic belongs in a mid-sequence puzzle where it can be introduced simply. It is out of scope for this sprint.

### Puzzle 9 — spatial redesign: "The Long Way Around"

The design doc's core beat: the "obvious" approach (boulder onto a plate to open a gate) fails because the gate is on the wrong side. The player must first send one character to a high lever that **reroutes** where the boulder needs to go — only after the lever is thrown does the correct pressure plate become the live target for the boulder.

The original spec had Dani climb a rope to reach the high lever. The proven substitute is the lift mechanic: Scarlet lifts Dani onto the upper ledge, Dani walks to the lever, throws it, then Scarlet pushes the boulder.

**Why order matters here (the "long way around" read):** If the player tries boulder → plate first, it either sends the boulder to the wrong plate (one that opens the wrong gate — a decoy gate that leads nowhere) or the correct plate is not yet wired to anything until the lever is thrown. The layout must physically enforce that Step 1 is "get Dani to the high lever" and Step 2 is "push the boulder to the now-correct plate." The player must discover this order through observation, not by brute force.

**Spatial layout:**

```
SIDE VIEW (simplified):

  ┌───────────────────────────────────────┐
  │  UPPER LEVEL (y ≈ 1.0)                │
  │  [lift point ↑]  lever(^)             │
  └───────────────────────────────────────┘

  ┌───────────────────────────────────────┐
  │  GROUND LEVEL (y = 0)                 │
  │  S  D   [decoy plate ≡]  ○  [correct plate ≡]  [  ] gate → ★ │
  └───────────────────────────────────────┘
```

Sequence:
1. Scarlet and Dani start at ground level. A boulder sits on the path. Two pressure plates are visible — one near the start (decoy), one deeper in the room.
2. An upper ledge runs along one wall, reachable only by the lift. A lever sits on the upper ledge.
3. **Wrong path (the trap):** Scarlet pushes the boulder onto the decoy plate. A gate opens — but it leads only to a dead end or a wall, not to the reunion trigger. The player observes this, backtracks, and reconsiders.
4. **Correct sequence:** Scarlet lifts Dani onto the upper ledge. Dani walks to the lever and throws it (one-shot, `id = "Route_Lever"`). This permanently opens the path between the correct plate and the main gate (in code: the main gate's `_triggerPlateId` is already "Correct_Plate"; the decoy gate is wired to "Decoy_Plate"). The lever throw itself does NOT open the main gate — it signals the player that something changed.
5. Scarlet pushes the boulder to the correct (deeper) pressure plate. The main gate opens.
6. Scarlet walks through the main gate. Both reunite at the far side.

**Implementation note on the "decoy" gate:** The decoy gate opens when its plate fires, confirming the boulder CAN land on it — the visual feedback is correct. But the space it opens leads to a small dead-end pocket with no path to the reunion trigger. No new code — it is just a second `Gate`/`PressurePlate` pair whose opening leads nowhere useful. The player reads the environment: "this gate opened but there's no path through it." That observation is the puzzle's core insight.

**Implementation note on Step 4:** The lever throw does not directly interact with the main gate (that gate is wired to `"Correct_Plate"`, not the lever). The lever throw is a visual/audio signal that something in the room changed. The actual gate open happens when the boulder lands on the correct plate in Step 5. This is a pure data-authoring pattern — no new mechanic or code.

**Neither alone:** Scarlet cannot reach the upper-level lever (stepHeight = 0.6 blocks the 1.0 m ledge). Dani cannot push the boulder (only `ScarletController.OnControllerColliderHit` applies force). Without the lever throw, the player may attempt decoy-plate first and learn the room through failure — that is intentional design.

**Multi-height is structural:** Remove the height difference and the lift is no longer needed — Dani can walk to the lever without Scarlet's help. The "neither alone" rule breaks. Therefore the height is structural.

### Puzzle 10 — full redesign: "World 1 Finale"

The original design spec is not executable: it requires rope climb (stub) and crawl tunnel (not in the data-driven pipeline). This sprint delivers the same emotional arc — lift enables Dani to reach a high path; Dani acts first to enable Scarlet; Scarlet crosses and joins Dani; boulder + plate + gate; both walk through together — using only proven mechanics.

**Spatial layout — two levels, two sides, one reunion:**

```
SIDE VIEW (simplified):

  ┌───────────────────────────────────────────────────────┐
  │  UPPER LEVEL (y ≈ 1.0)                                │
  │  [lift point ↑]  D walks across  lever(^)  [  ]gate  │
  └───────────────────────────────────────────────────────┘
                                    ↓ gate opens for Scarlet
  ┌───────────────────────────────────────────────────────┐
  │  GROUND LEVEL (y = 0)                                 │
  │  S  [gap]=====  ○   ≡   [  ]gate  →  ★               │
  └───────────────────────────────────────────────────────┘

  ═════ = bridge (drops from above when Scarlet stands on her plate)
  ≡     = pressure plate (Scarlet, held — keeps bridge up for herself)
  ○     = boulder (Scarlet pushes onto second plate after crossing)
  ★     = reunion trigger (ground level, far side)
```

The room has **two acts separated by a choke point:**

**Act 1 — Dani goes first:**
1. Scarlet and Dani start together at ground level. A high ledge runs along one wall; the lift point is here.
2. Scarlet lifts Dani onto the upper ledge. Dani walks across the upper path.
3. Dani reaches a lever on the upper level (`id = "Dani_Upper_Lever"`, `oneShot = true`). She throws it. This opens a gate on the ground level — a gate that was blocking Scarlet's access to the pressure plate and boulder in the second half of the room. (The gate swings open permanently.)
4. Scarlet is now unblocked. She walks through the now-open gate into the second half.

**Act 2 — Scarlet and then together:**
5. In the second half, at ground level, there is a gap blocking Scarlet's path to the boulder and the reunion trigger. A `StonePressurePlate` (`id = "Bridge_Plate"`, held, not one-shot) sits here; Scarlet stands on it. The bridge deploys across the gap.
6. Scarlet crosses the bridge. She reaches a `PushBoulder` and a second `StonePressurePlate` (`id = "Boulder_Plate"`, `oneShot = true`).
7. Scarlet pushes the boulder onto the second plate. A final gate opens (`id = "Boulder_Plate"`).
8. Scarlet walks through the final gate toward the `ReunionTrigger`.
9. Dani — who has been walking the upper path — descends via a narrow-ledge ramp at the far end (upper ledge terminates in a ramp back to ground level; ramp is wide enough for both characters since both need to reunite at ground level). Both enter the `ReunionTrigger` together.

**Wait — bridge timing in step 5:** Scarlet must step off the plate to cross the bridge, but the plate is hold-style (not one-shot), so stepping off retracts the bridge before she finishes crossing. Fix: **make the second plate one-shot** (`oneShot = true`). The first activation latches the bridge down permanently. Scarlet steps on it once, bridge deploys and stays, Scarlet crosses freely. This is a pure data authoring change — `PressurePlate._oneShot = true` already works for this in the codebase.

**Neither alone — verified:**
- Dani cannot push the boulder (only Scarlet can).
- Scarlet cannot reach the upper lever (stepHeight blocks the 1.0 m ledge, lift requires Dani).
- Before Dani throws her lever, the gate blocking Act 2 is closed — Scarlet cannot reach the boulder or the reunion trigger.
- Without the bridge latching, Scarlet cannot cross the gap and reach the boulder.
- The reunion trigger requires both characters inside simultaneously (`ReunionTrigger` fires only when both tags are present).

**Multi-height is structural:** Remove the height difference and Dani can walk to the lever on the ground plane without needing the lift. The "neither alone" rule breaks immediately. The height is structural.

**Story beat (Puzzle 10 completion):** The `PuzzleCompleteUI` delay (2.5 s via `LevelSequenceController`) gives a moment of stillness before the MainMenu loads. The isometric camera holds on both characters standing together at the far side of the room. No code addition is needed — the existing 2.5 s `_completionDelay` in `LevelSequenceController` provides this beat.

### Multi-height terrain — no new spike needed

The terrain construction is proven from Sprint 03 and used in every puzzle since: floor tiles at `position.y = -0.5`, `scale.y = 0.5` give a top walkable surface at `y = 1.0 m`. `stepHeight = 0.6` blocks unassisted climbing of 1.0 m ledges. The lift mechanic is proven at `liftOffset (0, 1.6, 0.3)` with the 0.5 m forward nudge and upward velocity on release. No terrain spike is needed this sprint — apply the known construction directly.

### Narrow-gap width restriction (Puzzle 10, upper-to-lower ramp for Dani)

In Act 2, Dani needs to descend from the upper level to the reunion trigger at ground level. A ramp (RPGRampCovered platform type) works if it is wide enough for both characters — since both need to reach the reunion trigger at ground level, there is no width restriction needed on this ramp. Scarlet reaches ground level by crossing the bridge; Dani reaches ground level by descending the far ramp. The ramp can be full-width.

### No new LevelObjectType enum values needed

Puzzle 9 uses: `PushBoulder (9)`, `StonePressurePlate (10)`, `Gate (2)`, `Lever (3)`, `ReunionTrigger (5)`, `Checkpoint (6)`, `Coin (4)`. All already in the enum.

Puzzle 10 uses: `PushBoulder (9)`, `StonePressurePlate (10)` (×2), `Bridge (8)`, `Gate (2)` (×2), `Lever (3)`, `ReunionTrigger (5)`, `Checkpoint (6)`, `Coin (4)`. All already in the enum.

No additions to `LevelObjectData.cs`, `LevelBuilder.cs`, or `LevelPrefabRegistry` are required. This is a pure data-authoring sprint.

---

## Success criteria (sprint definition of done)

- From Main Menu, the player can play Puzzles 1 through 10 in sequence without a manual scene change or editor intervention; completing Puzzle 10 returns to MainMenu.
- **Puzzle 9:** Scarlet and Dani start together. Attempting boulder → decoy plate → decoy gate first leads to a visible dead end (no path to the reunion trigger through that gate). Scarlet lifts Dani onto the upper ledge; Dani throws the lever; Scarlet pushes the boulder to the correct plate; the main gate opens; Scarlet walks through; both reunite. Scarlet cannot reach the lever without being given a lift. Dani cannot push the boulder at all.
- **Puzzle 10:** Scarlet lifts Dani onto the upper ledge; Dani traverses the upper path and throws her lever; the Act 2 gate opens for Scarlet at ground level; Scarlet stands on the bridge plate (one-shot); the bridge latches down; Scarlet crosses; Scarlet pushes the boulder onto the second plate; the final gate opens; both characters enter the reunion trigger together. No character can complete it without the other. The bridge does not retract while Scarlet is crossing.
- **Height is structural in both rooms:** flattening either room to y=0 would break the core mechanic separation (the "neither alone" constraint would be violated without the height).
- Failure (fall hazard) in either new puzzle returns both characters to that puzzle's checkpoint; coin respawn still works.
- Hints load correctly per puzzle — Puzzle 9 hints are Puzzle 9 text, Puzzle 10 hints are Puzzle 10 text (not copied from a prior level; verify `HintManager` points to the correct `LevelData` in both scenes).
- `LevelSequenceController._levelScenes` in `World1_Puzzle1` lists all ten puzzle scenes in order.
- All ten scenes appear in Build Settings and the Active Build Profile.

---

## Backlog

| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | Create `World1_Puzzle9` scene by duplicating `World1_Puzzle8.unity`; strip all prior puzzle objects; wire in a new `World1_Puzzle9` LevelData asset. Confirm scene opens with both characters spawning and no leftover Puzzle 8 elements. Verify `HintManager` points to the new asset. | S | Scene opens in play mode; both characters spawn at origin with no prior puzzle elements; `HintManager.levelData` references `World1_Puzzle9`. | — |
| 2 | Author `World1_Puzzle9` LevelData: ground-level floor for Scarlet and Dani start; a `PushBoulder` on the path; a **decoy** `StonePressurePlate` (`id = "Decoy_Plate"`, `oneShot = false`) with a `Gate` (`id = "Decoy_Gate"`, linked to `"Decoy_Plate"`) that opens into a visible dead end; upper-level floor tiles at `y = -0.5, scale.y = 0.5` reachable only by the lift; a `Lever` on the upper level (`id = "Route_Lever"`, `oneShot = true`, visual-only — wired to no gate, serves as the player signal that the room state changed); a second **correct** `StonePressurePlate` deeper in the room (`id = "Correct_Plate"`, `oneShot = true`); a `Gate` at the main exit linked to `"Correct_Plate"`; the lift point positioned such that `stepHeight = 0.6` cannot reach the upper ledge without the lift; a `ReunionTrigger` beyond the main gate; a `Checkpoint` near entry; fall hazard boundary triggers; 3 progressive hints. | M | In play mode, the full Puzzle 9 solve sequence works: lift Dani → lever throw → boulder to correct plate → main gate opens → both reunite. Pushing boulder to decoy plate opens a gate to a dead end only. Scarlet cannot reach the lever unaided (stepHeight blocked). | Task 1 |
| 3 | Verify Puzzle 9 "neither alone," the decoy trap, and no-bypass. Confirm: (a) Scarlet cannot reach the upper lever without the lift; (b) pushing the boulder to the decoy plate produces a dead-end gate, not the reunion path; (c) the correct-plate gate opens only after the boulder lands there; (d) characters cannot bypass any gate by walking around it (fall hazard choke). Tune boulder push distance, plate positions, and decoy gate dead-end geometry as needed. | S | All four checks pass in play mode. Full solve by switching takes the correct order: lift Dani → lever → boulder to correct plate → gate → reunite. The decoy trap is observable and instructive, not frustrating (dead end is clearly visible as a dead end). | Task 2 |
| 4 | Create `World1_Puzzle10` scene by duplicating `World1_Puzzle9.unity`; strip all prior puzzle objects; wire in a new `World1_Puzzle10` LevelData asset. Confirm scene opens with both characters spawning and no leftover Puzzle 9 elements. Verify `HintManager` points to the new asset. | S | Scene opens in play mode; both characters spawn at origin; no prior puzzle elements remain; `HintManager.levelData` references `World1_Puzzle10`. | Task 1 (pattern established) |
| 5 | Author `World1_Puzzle10` LevelData — Act 1: Scarlet and Dani start together at ground level; upper-level floor tiles (proven `y = -0.5, scale.y = 0.5`) along one wall for Dani's path; lift point at start of upper ledge; a `Lever` on the upper level (`id = "Dani_Upper_Lever"`, `oneShot = true`); a `Gate` at ground level between Act 1 and Act 2, linked to `"Dani_Upper_Lever"` (this gate is closed until Dani throws her lever; it permanently opens when she does); a `Checkpoint` at the Act 1 / Act 2 boundary (so a late failure in Act 2 doesn't send them back to the very start). | M | In play mode, Act 1 runs correctly: Scarlet lifts Dani onto the upper ledge; Dani walks to the lever and throws it; the Act 2 ground-level gate opens permanently; Scarlet walks through. Dani cannot throw the lever without being lifted. Scarlet cannot enter Act 2 before the gate opens. | Task 4 |
| 6 | Author `World1_Puzzle10` LevelData — Act 2: a gap in the ground-level floor in Act 2; a `StonePressurePlate` (`id = "Bridge_Plate"`, `oneShot = true` so the bridge latches on first contact and does not retract when Scarlet steps off); a `Bridge` linked to `"Bridge_Plate"` spanning the gap; a `PushBoulder` on the far side of the gap; a second `StonePressurePlate` (`id = "Boulder_Plate"`, `oneShot = true`) that the boulder rolls onto; a `Gate` at the final exit linked to `"Boulder_Plate"`; a far-end ramp from the upper level back to ground level (wide, no width restriction — both characters can descend it; Dani descends from the upper path, Scarlet arrives from the bridge path); a `ReunionTrigger` at the far end of ground level (both characters must be inside simultaneously); fall hazard boundary triggers; coins (4–6 total across both acts, more in Act 2 per the late-game coin distribution guidance in `README-Design-Feedback.md`); 3 progressive hints per level. | M | In play mode, Act 2 runs correctly: Scarlet stands on the bridge plate (bridge latches down permanently); Scarlet crosses; Scarlet pushes the boulder onto the second plate; the final gate opens; both characters enter the reunion trigger together. The bridge does not retract while Scarlet is crossing. Neither character can complete Act 2 alone. | Task 5 |
| 7 | Verify Puzzle 10 full end-to-end and "neither alone." Confirm: (a) Scarlet cannot reach the upper lever without the lift (stepHeight blocked); (b) Scarlet cannot enter Act 2 before Dani throws her lever; (c) the bridge latches on one-shot and does not retract while Scarlet is mid-crossing; (d) the boulder can only be pushed by Scarlet; (e) the reunion trigger fires only when both characters are inside (not just one); (f) characters cannot bypass any gate (fall hazard choke). Tune boulder push distance, gap width, bridge position, and ramp placement as needed. | S | All six checks pass in play mode. Full solve by switching: Scarlet lifts Dani → switch to Dani → walk upper path → throw lever → switch to Scarlet → walk through Act 2 gate → step on bridge plate → cross bridge → push boulder → final gate opens → walk to reunion → Dani descends far ramp → both inside trigger → level complete. | Tasks 5, 6 |
| 8 | Extend `LevelSequenceController._levelScenes` (Inspector edit on the `DontDestroyOnLoad` GameObject in `World1_Puzzle1.unity`) to include all ten puzzle scenes in order. Add `World1_Puzzle9` and `World1_Puzzle10` to Build Settings and the Active Build Profile. Confirm completing Puzzle 10 returns to MainMenu without error or dead scene. | S | Playing from MainMenu proceeds through all ten puzzles in sequence; completing Puzzle 10 returns to MainMenu. Both new scenes appear in Build Settings. No `SceneManager.LoadScene` error in the console. | Tasks 2, 5 |
| 9 | Verify full ten-puzzle sequence from MainMenu. Confirm scene-to-scene transitions are clean (no leftover objects, no wrong HintManager reference, no wrong character start position) across the full run. Note: this does not require solving all ten — fast-solve each puzzle in sequence up to Puzzle 10, then solve it fully and confirm MainMenu return. | S | All ten puzzles load in order from MainMenu. Each new puzzle loads with characters at the correct start position and hints pointing to the correct `LevelData`. Puzzle 10 completion returns to MainMenu. | Task 8 |

---

## Out of scope

- **Rope/vine vertical climb.** `DaniController.SetClimbingState` remains a stub. Every height transition in both puzzles uses the lift mechanic or floor-tile ramps. Rope climb is not scheduled unless the developer explicitly chooses to build it.
- **CrawlTunnelEntrance as a data-driven object.** This prefab has no `LevelObjectType`, no `LevelPrefabRegistry` entry, and no `LevelBuilder` case. Planning any puzzle beat around it would require code additions to three files. That work is deferred. If narrow passages are needed, the geometry-only width restriction technique (proven in Puzzle 4) is the substitute.
- **Stacking mechanic as a puzzle element.** `DaniController.TryPickUpObject` works, but Puzzle 9 and 10 are the wrong place to introduce it for the first time. It belongs in a mid-sequence puzzle where it can be taught simply. Out of scope for this sprint.
- **Audio / sound design.** All feedback is visual (gate swinging, bridge descending, plate color change). No audio system work is planned.
- **New Meshy/Blender art.** Both puzzles use platform types and prefabs already in `LevelPrefabRegistry`. No 3D asset pipeline work.
- **World 2 or any content beyond the ten-puzzle World 1 sequence.** This sprint closes World 1.
- **New `LevelObjectType` enum values or new `LevelBuilder` cases.** All needed types exist.

---

## Risks & assumptions

- **The "Route_Lever" in Puzzle 9 is a visual-only signal.** The lever throw fires `GameEvents.RaisePressurePlateChanged("Route_Lever", true)`. If no Gate is wired to `"Route_Lever"`, this event goes nowhere — which is the intended behavior (the gate that matters is wired to `"Correct_Plate"`, not to the lever). However, verify in play mode that Dani can actually reach and throw the lever, and that the throw produces a visible/audible reaction (lever animation flip) so the player knows something happened. If the "no gate opens" read is confusing — the player cannot tell whether the lever did anything — one option is to wire the lever to a second gate that opens a previously-invisible shortcut or visual cue. This is a play-mode judgment call for Task 3.

- **Decoy gate placement in Puzzle 9.** The decoy plate and gate must be placed such that (a) the player naturally tries it first (it is closer or more obvious than the correct plate) and (b) the dead end it opens is immediately readable as a dead end (a wall, a short pocket, clearly no path forward). If the dead end is too subtle, the player pushes the boulder back (which may be physically difficult with the current boulder drag=6 setup) and gets frustrated. Test this in Task 3 and tune the dead-end geometry to be unambiguously closed-off. The dead end should read as "wrong path" in one second of observation, not as "maybe there's more."

- **Boulder cannot be pushed back.** `PushBoulder` (drag=6, Y-frozen Rigidbody) can only be pushed in the direction Scarlet walks into it. Once the boulder is on the decoy plate in Puzzle 9, Scarlet cannot easily push it back off. This means the player may be "stuck" with the boulder on the wrong plate and have to use the checkpoint reset (or coin respawn) to restart. This is acceptable — the puzzle is rated Hard and the player is expected to reset from checkpoint. However, the decoy plate should be positioned such that the boulder can plausibly roll off the plate under its own momentum if Scarlet nudges it from the correct side, reducing restart frequency. Alternatively, place the decoy plate in a position where the boulder cannot reach it without deliberate effort (so casual pushes go to the correct plate, and the decoy is only triggered by an intentional wrong move). Task 3 must resolve this through play-mode tuning.

- **Bridge plate one-shot behavior in Puzzle 10.** The plan uses `oneShot = true` on `"Bridge_Plate"` so the bridge latches permanently after Scarlet first stands on it. This is the same pattern proven in Sprint 04 (Puzzle 8's one-shot lever latching the bridge). Verify in Task 7 that the `PressurePlate._oneShot` path works correctly when the activating object is Scarlet (a `CharacterController`) rather than a Lever — it should, since `PressurePlate.OnTriggerEnter` fires for any collider and the `_oneShot` latch is independent of the trigger source.

- **Dani's upper-path return in Puzzle 10.** Dani traverses the upper level in Act 1, then must descend a far ramp to reach the reunion trigger at ground level in Act 2. The ramp must be positioned so Dani can reach it after Scarlet has crossed the bridge and pushed the boulder — meaning the ramp exits at a ground-level position near (or past) the final gate. Verify in Task 7 that Dani's path from upper level to reunion trigger does not require passing through any gate that is closed during Act 2. If the gate layout forces Dani to backtrack through Act 1, add a second ramp or adjust the upper-level floor layout so Dani's path is a forward-only traverse (start → upper ledge → traverse → far ramp → reunion trigger).

- **`LevelSequenceController._levelScenes` is a serialized array on the `DontDestroyOnLoad` singleton in `World1_Puzzle1.unity`.** Sprint 04 extended it to eight entries. Task 8 extends it to ten. This is an Inspector edit on that specific object in `World1_Puzzle1.unity` — not in the new scenes. Verify the singleton is on the correct object and that the `DontDestroyOnLoad` pattern means only one `LevelSequenceController` exists at runtime when running from MainMenu.

- **Assumption: scene duplication is the proven creation pattern.** Tasks 1 and 4 duplicate an existing scene. Always strip all prior puzzle elements after duplication and verify that `HintManager` (a scene-level component) has its `levelData` reference updated to the new asset — this has silently broken in prior sprints when the old reference was left in place.

- **Coin distribution in Puzzle 10.** `README-Design-Feedback.md` states: "Late-game puzzles should have more coins nearby — the difficulty warrants more safety valve resources." Puzzle 10 should have 4–6 coins (versus the 3–5 in earlier puzzles), placed in the natural exploration path (Act 1 upper ledge, Act 2 ground path). Task 6 includes coins in the LevelData authoring — confirm count and placement favor the late-game guidance.

---

## References

- Design: `README-Puzzle-Design.md` — Puzzle 9 "The Long Way Around" (multi-step, non-obvious, first real "aha"), Puzzle 10 "World 1 Finale — The Old Bridge" (greatest-hits combination), Design Rules (neither alone, one enables the other, no text instructions, separation resolved), Difficulty Curve (Puzzle 9: Hard, Puzzle 10: Hard)
- Design: `README-Design-Feedback.md` — "very difficult by the end," coin distribution for late-game puzzles, failure state, "neither solves it alone" rule, reunion as win condition
- Standing preference (bridges over vertical climb): rope/vine climb stays a stub; Puzzle 9's "Dani reaches high lever" beat is delivered by the lift, not a climb; Puzzle 10's original rope beats are replaced by lift + bridge + ramp
- CrawlTunnelEntrance exclusion: confirmed not in the data-driven pipeline (no `LevelObjectType`, no `LevelPrefabRegistry` entry, no `LevelBuilder` case) — no narrow-tunnel beats planned for either puzzle
- Proven terrain construction (Sprint 03 + Sprint 04): floor tiles at `position.y = -0.5`, `scale.y = 0.5` → top surface at `y = 1.0 m`; `stepHeight = 0.6` blocks unassisted climbing; lift `liftOffset (0, 1.6, 0.3)`, 0.5 m forward nudge, upward velocity on release
- Core systems: `Assets/Scripts/Core/LevelBuilder.cs`, `Assets/Scripts/Data/LevelData.cs`, `Assets/Scripts/Data/LevelObjectData.cs` (all needed types present: `Gate=2`, `Lever=3`, `Coin=4`, `ReunionTrigger=5`, `Checkpoint=6`, `Bridge=8`, `PushBoulder=9`, `StonePressurePlate=10`), `Assets/ScriptableObjects/LevelPrefabRegistry.asset`
- Puzzle objects: `Assets/Scripts/Puzzle/Bridge.cs` (one `_plateId`, deploys on `OnPressurePlateChanged`), `Assets/Scripts/Puzzle/Lever.cs` (`_oneShot`, fires `OnPressurePlateChanged`), `Assets/Scripts/Puzzle/Gate.cs` (swings on `OnPressurePlateChanged`), `Assets/Scripts/Puzzle/PressurePlate.cs` (`_oneShot` latch proven), `Assets/Scripts/Puzzle/Hazard.cs`
- Character abilities: `Assets/Scripts/Characters/ScarletController.cs` (`OnControllerColliderHit` boulder push, `TryLiftDani` / `ReleaseDani`), `Assets/Scripts/Characters/DaniController.cs` (`TryActivateSwitch`, `BeginLiftedState` / `EndLiftedState`, `SetClimbingState` is a stub — do not use)
- Sequence: `Assets/Scripts/Core/LevelSequenceController.cs` (`_levelScenes` to extend to 10 entries in Task 8 — this field lives on the `DontDestroyOnLoad` GameObject in `World1_Puzzle1.unity`)
- Existing levels to reference for construction patterns: `Assets/ScriptableObjects/Levels/World1_Puzzle5.asset` (lift mechanic + multi-height), `Assets/ScriptableObjects/Levels/World1_Puzzle8.asset` (one-shot lever latching bridge, hold-and-cross), `Assets/ScriptableObjects/Levels/World1_Puzzle3.asset` (boulder + pressure plate)
- Existing scenes to duplicate: `Assets/Scenes/World1_Puzzle8.unity` (for Task 1), `Assets/Scenes/World1_Puzzle9.unity` (for Task 4, once Task 1 is done)

---

## Ready to hand to `unity-senior-developer`

- **Tasks 1 and 4** (scene creation by duplication) are mechanical — hand off immediately after authorizing. Both follow the same proven pattern used every sprint since Sprint 02.
- **Tasks 2 and 5** (LevelData authoring for Act 1) can be handed off once the corresponding scene exists. The spatial layouts in this document are the authoritative spec. Puzzle 9's authoring (Task 2) is self-contained. Puzzle 10 Act 1 (Task 5) must be stable before Act 2 authoring (Task 6) begins, since the Act 2 gate is linked to Dani's upper-level lever from Act 1.
- **Task 6** (Puzzle 10 Act 2 LevelData) depends on Task 5 being complete and verified at a basic level (both characters spawn, Act 1 gate is functional). Hand off once Task 5 is stable.
- **Task 8** (`_levelScenes` + Build Settings) is a quick Inspector + build-settings edit — hand off after Tasks 2 and 5 are stable enough to be in sequence.
- **Tasks 3, 7, and 9** (verification, no-bypass checks, full sequence run) benefit from Louie's hands-on play-mode judgment. The agent can assist with data-side tuning (plate positions, gap widths, bridge speeds) but the "does this read correctly" and "is the decoy instructive vs. frustrating" calls are human judgment.

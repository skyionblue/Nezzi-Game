# Sprint 01 — First Two Puzzles Playable End-to-End

**Goal:** Turn the isometric tech demo into an actual game: Puzzle 1 (Side by Side) and Puzzle 2 (The Gap) are fully playable, and completing Puzzle 1 loads Puzzle 2.
**Theme:** Core loop — from "systems exist" to "you can play the opening of World 1"
**Status:** Planned

## Why this sprint

The project has strong systems (data-driven level building, character switching, coins,
hints, failure/respawn) but is **not yet a game you can play through**. There is exactly one
level asset, it does not match any of the 10 designed puzzles, and finishing it goes nowhere —
`GameManager.HandleReunionAchieved` only sets `PuzzleComplete` state with no "next level" load.

Two of the mechanics the puzzle docs depend on are also unproven in the new HD-2D isometric
build: **rope/vine climb is an explicit stub** (`DaniController.SetClimbingState` does nothing)
and **boulder-push / lift / stacking are coded but never exercised by a real puzzle**.

This sprint delivers the smallest honest vertical slice of the actual designed game: the two
gentlest puzzles, the mechanics they need (movement + separation + one enabling action), and
the level-to-level flow that makes it a sequence instead of a single room. It deliberately
avoids climb (deferred in code) by choosing Puzzle 2's lever-drops-rope beat as a **lever/gate
interaction** rather than a physical climb, and flags the climb redesign as its own future item.

## Success criteria (sprint definition of done)
- From the Main Menu, the player enters Puzzle 1, walks both siblings onto the reunion spot, and the game **advances to Puzzle 2** without a manual scene change.
- Puzzle 1 plays as the "warm handshake": flat ground, no obstacles, both characters reach the reunion trigger and it completes.
- Puzzle 2 requires the switch-and-enable beat: the player must switch to Dani, throw a lever, open Scarlet's gate, and reunite — Puzzle 2 cannot be completed by one character alone.
- Failure (walking into a hazard / falling off the world) returns to the puzzle's checkpoint, and respawn-in-place still works, in both new puzzles.
- Hints load per-puzzle: the hint text shown in Puzzle 2 is Puzzle 2's, not Puzzle 1's.

## Backlog
| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | Author `World1_Puzzle1` LevelData (flat forest floor, Scarlet + Dani start side-by-side, single reunion trigger, no obstacles). Add 3 hint strings even though it's trivial. | S | In play mode, LevelBuilder builds a flat walkable room; both characters spawn side by side; walking both onto the reunion trigger fires "puzzle complete." | — |
| 2 | Author `World1_Puzzle2` LevelData (The Gap): Scarlet penned behind a closed gate on the left, Dani free on the right with a lever; lever `id` matches gate `id`, lever is `oneShot`. Include a floor gap / hazard boundary so the separation reads. | M | In play mode, Scarlet cannot pass until the player switches to Dani and throws the lever; after the lever, Scarlet's gate opens and both can reunite. One character alone cannot finish. | Task 1 (pattern to copy) |
| 3 | Add a level-sequence flow so reunion advances to the next puzzle. Introduce a lightweight ordered list of LevelData (or scene refs) and load the next on `OnReunionAchieved`, with a short "complete" beat before the load. | M | Completing Puzzle 1 automatically brings up Puzzle 2 (fresh coins state per design TBD — see risks); no manual scene/inspector change needed. | Tasks 1, 2 |
| 4 | Wire per-puzzle HintManager + LevelBuilder data. Ensure loading Puzzle 2 repoints HintManager `_levelData` (and coin/hint state) to Puzzle 2 so hints and the hint button reflect the current puzzle. | S | Requesting a hint in Puzzle 2 shows Puzzle 2's authored hint text and cost; the hint counter resets appropriately for the new puzzle. | Task 3 |
| 5 | Verify boulder-push in the isometric build using a throwaway test setup (Scarlet walks into a Rigidbody boulder and moves it onto a pressure plate). Confirm `PushBoulder` prefab + `StonePressurePlate` + `Gate` react. Document the working config. | S | In play mode, Scarlet pushes the boulder onto the plate and the linked gate opens; Dani cannot budge the boulder. Findings noted for the Puzzle 3 sprint. | — |
| 6 | Confirm checkpoint + failure + respawn works in both new puzzles: place a `Checkpoint` at each puzzle's entry, a `Hazard` (or fall-off boundary) mid-puzzle, and validate reset + coin respawn. | S | Triggering failure in each puzzle returns both siblings to that puzzle's checkpoint; spending coins respawns in place; input is frozen during the failure panel. | Tasks 1, 2 |
| 7 | Main Menu → Puzzle 1 entry: ensure "Play" loads the first puzzle in the sequence via the new flow, not a hardcoded scene. | S | Pressing Play from `MainMenu` starts Puzzle 1; on completion it continues into Puzzle 2 with no dead end. | Task 3 |
| 8 | Reconcile `CLAUDE.md` "Key Scene / 2D" notes with the shipped HD-2D isometric reality (movement, no jump, 3D colliders). Planning-doc only — no code. *(Optional if time-boxed; flag as tech-debt otherwise.)* | S | `CLAUDE.md` no longer describes 2D Rigidbody2D/Tilemap/jump as the current model; the isometric CharacterController model is documented. | — |

## Out of scope
- Rope / vine **physical climb** mechanic — it is an explicit stub in `DaniController.SetClimbingState` and needs a design pass for the isometric space. Puzzle 2 here uses a lever→gate beat, not a climb. (Own future sprint.)
- Puzzles 3–10 (boulder, tunnel, lift, trust, combinations, finale) — this sprint only proves the loop with the two gentlest puzzles. Task 5 pre-verifies the boulder mechanic so the Puzzle 3 sprint is unblocked.
- Dani stacking as a *puzzle* — the code exists but no puzzle in scope requires it.
- Secret keys, lore doors, co-op second-player onboarding polish, audio design for the trust puzzle.
- New art from Meshy/Blender — this sprint uses existing prefabs and platform types only.

## Risks & assumptions
- **Level-transition state ownership is the biggest risk.** GameManager, CoinManager, HintManager, and CheckpointManager are singletons/scene objects assuming one level per session. Advancing to the next puzzle must decide: do coins carry over between puzzles (design says coins are a persistent resource — likely yes), and do checkpoints/hints reset (yes). Task 3 must define this explicitly; getting it wrong causes stale hints or lost coins. **This is the one thing most likely to blow the sprint.**
- **Assumption:** "advance to next puzzle" can be done by reloading the same scene with a different LevelData rather than one Unity scene per puzzle. If separate scenes are preferred, Task 3 grows toward L and should be split (flow controller vs. scene wiring).
- Boulder-push (Task 5) may not behave in the isometric CharacterController setup — CharacterController does not push Rigidbodies without `OnControllerColliderHit` push code. If it doesn't work out of the box, that's a finding that adds a task to the Puzzle 3 sprint, not this one.
- Puzzle 2's "gap" needs a way to make Scarlet unable to cross — with no jump, a literal gap in the floor plus a fall-off boundary (Hazard) is the readable option; confirm the floor plates leave an actual walkable hole.
- CLAUDE.md is stale vs. the shipped iso build; treat the code (not CLAUDE.md) as source of truth for movement/physics this sprint.

## References
- Design: `README-Puzzle-Design.md` — Puzzle 1 "Side by Side", Puzzle 2 "The Gap", Design Rules (neither solves alone; one enables the other).
- Design: `README-Design-Feedback.md` — Reunion win condition, Failure state (checkpoint + coin respawn), Coin/Hint system.
- Level pipeline: `Assets/Scripts/Core/LevelBuilder.cs`, `Assets/Scripts/Data/LevelData.cs`, `Assets/Scripts/Data/LevelObjectData.cs`, `Assets/ScriptableObjects/Levels/World1_Level1.asset` (existing example to copy).
- Loop: `Assets/Scripts/Core/GameManager.cs` (`HandleReunionAchieved` — the dead end to fix), `Assets/Scripts/Puzzle/ReunionTrigger.cs`, `Assets/Scripts/Core/HintManager.cs`, `Assets/Scripts/Core/CheckpointManager.cs`, `Assets/Scripts/Collectibles/CoinManager.cs`.
- Mechanics: `Assets/Scripts/Characters/ScarletController.cs` (lift), `Assets/Scripts/Characters/DaniController.cs` (stack + climb stub), `Assets/Scripts/Puzzle/{Lever,Gate,PressurePlate,Hazard}.cs`.

## Ready to hand to `unity-senior-developer`
- Tasks 1, 2, 6 (LevelData authoring + checkpoint/hazard placement) are well-specified against the existing `World1_Level1.asset` pattern.
- Task 5 (boulder verification) is a self-contained investigation with a clear pass/fail.
- Task 3 (level-sequence flow) should be discussed/design-confirmed first (see risks) before handing off.

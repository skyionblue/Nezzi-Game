# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Commit Policy

**HARD RULE — NO EXCEPTIONS:** Do not run `git commit` or `git push` under any circumstances until the user explicitly says "commit", "commit and push", or "push". This applies to all work, all sessions, all agents, and all automated scripts.

- Do NOT commit "to save progress"
- Do NOT commit after fixing a bug
- Do NOT commit after an agent completes work
- Do NOT commit as part of a multi-step task unless that step was explicitly authorized

When changes are ready: stage them, describe what will be committed, and **stop**. Wait for explicit user authorization before running `git commit`.

---

## Project Overview

**One Way Together** — an **HD-2D isometric** cooperative puzzle game for iOS and Android. Two siblings, Scarlet (big) and Dani (small), are lost and trying to find their way home. Every puzzle requires both characters. Win condition: both characters must be inside a `ReunionTrigger` simultaneously — not just reach an exit.

**Engine:** Unity 6 LTS, URP, targeting iOS + Android. **3D characters + 3D environment** (Polyworks asset pack) viewed through a perspective isometric camera. Movement is on the **XZ ground plane via `CharacterController`** — there is **no jump** and no 2D physics.

---

## Repository Layout

```
Nezzy's Game/
├── CLAUDE.md                        ← this file
├── README.md                        ← design overview
├── README-Design-Feedback.md        ← confirmed game decisions
├── README-Puzzle-Design.md          ← puzzle design doc (10 World 1 puzzles)
├── README-Meshy-Asset-Prompts.md    ← Meshy prompts for all props
├── models/                          ← raw Meshy ZIP downloads
├── art/
│   ├── characters/Scarlet/          ← Blender pipeline files + processed FBX
│   └── characters/Dani/             ← Blender pipeline files + processed FBX
├── docs/
│   ├── sprints/                     ← SPRINTS.md index + sprint-NN-*.md files
│   └── meshy-icon-loading-screen-prompts.md
├── game/One Way Together/           ← Unity project root
│   └── Assets/
│       ├── Scripts/                 ← all C# (namespace: OneWayTogether.*)
│       ├── Art/Characters/          ← FBX + textures + materials
│       ├── Art/Props/               ← prop FBX + textures + materials
│       ├── Prefabs/Props/           ← ready-to-drag prefabs (PushBoulder, etc.)
│       ├── Prefabs/Puzzle/          ← Gate, Lever, Bridge, Coin, etc.
│       ├── Prefabs/Env/             ← environment prefabs (e.g. CrawlTunnelEntrance)
│       ├── Animation/               ← ScarletAnimator.controller, DaniAnimator.controller
│       ├── Input/                   ← OneWayTogetherControls.inputactions
│       ├── Scenes/                  ← MainMenu, World1_Puzzle1, World1_Puzzle2
│       ├── StepHeightSystem/        ← Step Height Controller package (CharacterController adapters)
│       └── ScriptableObjects/
│           ├── Characters/          ← ScarletData.asset, DaniData.asset
│           ├── Coins/               ← CoinSystemData.asset
│           └── Levels/              ← World1_Puzzle1.asset, World1_Puzzle2.asset, LevelPrefabRegistry
└── .claude/
    ├── agents/                      ← unity-senior-developer, sprint-planner, etc.
    ├── foundational/                ← project-context.md, tech-standards.md
    └── skills/                      ← character-importer, asset-pipeline, level-design, profiling-workflow
```

**Levels are data-driven, not hand-placed.** `LevelBuilder` (in `Scripts/Core/`) reads a `LevelData` ScriptableObject at `Awake` and instantiates the floor, walls, props, and puzzle objects from a `LevelPrefabRegistry`. Do not build levels by hand in the scene — author/edit the `LevelData` asset.

**`LevelBuilder.BuildLevel()` order:** `BuildPlatforms()` → `PlaceObjects()` → `Physics.SyncTransforms()` → `PlaceCharacters()`. The `Physics.SyncTransforms()` call is critical — it forces floor colliders to register before characters spawn, preventing them from falling through on the first frame.

---

## Unity Development

**Unity version:** 6000.5.6f1 (Unity 6 LTS)
**MCP:** Unity MCP is connected when working in this project — use the `unity-mcp-skill` before Unity Editor tasks. Key MCP tools: `manage_gameobject`, `manage_components`, `manage_asset`, `manage_scene`, `execute_menu_item`, `batch_execute`, `manage_camera`.

**Blender MCP** is also connected for asset pipeline work (mcp__blender__execute_tool).

There is no CLI build command — builds are triggered through Unity Editor (File → Build Settings).

---

## Code Architecture

### Namespace
All runtime C#: `OneWayTogether.<SubNamespace>` (Core, Characters, Input, Camera, Collectibles, Puzzle, UI, Events, Data).

### Event Bus
`GameEvents` (static class in `Events/`) is the only cross-system communication channel. Nothing polls or calls across systems directly — everything goes through events. Key raises:
```csharp
GameEvents.RaiseActiveCharacterChanged(CharacterType)  // fires on Tab/SWAP switch
GameEvents.RaiseReunionAchieved()                      // puzzle complete
GameEvents.RaisePressurePlateChanged(string id, bool)  // puzzle mechanics
GameEvents.RaiseGateStateChanged(string id, bool)
GameEvents.RaiseCheckpointReset()                      // after teleport to checkpoint
```

### Character System
`CharacterBase` (abstract) owns: `CharacterController` movement on the XZ plane with **manually applied gravity** (no jump), facing rotation toward the move direction, walk/run speed blending from joystick magnitude, animator driving, and `IsControllable` state. Characters receive input through `ReceiveMove(Vector2)`, `ReceiveInteract()`, and `ReceiveStopMove()`.

**Walk/Run:** Joystick magnitude < 0.5 → `MoveSpeed` (walk). Magnitude ≥ 0.5 → `RunSpeed` (run). Both values live in `CharacterData`. The animator `Speed` parameter is passed raw so the Walk→Run transition at 5.5 u/s fires the Kevin Iglesias run animation.

**Step Height:** Both characters have `StepHeightController` (from the StepHeightSystem package) with CC adapter components (`CCRigidbodyWrapper`, `CCMovementInputManager`, `CCColliderManager`, `StepHeightBootstrapper`). The bootstrapper uses reflection to inject these into the package's private fields. `stepHeight = 0.6`.

Subclasses add character-specific abilities:
- `ScarletController`: push boulders via `OnControllerColliderHit` (ForceMode.Force, `_pushForce = 15`), lift Dani
- `DaniController`: activate switches/levers, stack objects. **Climb (ropes/vines) is a stub** (`SetClimbingState` does nothing).

**Input flow:** `InputRouter` owns the `InputActionAsset` and forwards to the active character. On-screen mobile controls (`MobileInputBridge` + joystick and SWAP/USE buttons) call the same `InputRouter` public methods.

**Input is gated on game state:** `InputRouter` only forwards input while `GameState.Playing`.

### Checkpoint System
`CheckpointManager` tracks per-character spawn positions. **`LevelBuilder.PlaceCharacters()` calls `cm.RegisterCharacter()` after moving characters** — this is the authoritative registration. `CheckpointTrigger.Activate()` also calls `RegisterCheckpoint()` per character with ±0.5 X offsets. `HandleCheckpointActivated` is intentionally a no-op (was previously overwriting both positions to the same point).

`CheckpointManager.Teleport()` disables the CharacterController, sets position, re-enables, and calls `CharacterBase.ResetVelocity()` — all three steps are required to prevent falling through the floor after teleport.

### Puzzle System
Puzzle objects use **3D trigger colliders**:
- `Gate` — swings open (rotation-based) on `OnPressurePlateChanged`
- `Lever` (implements `IInteractable`) — Dani activates via overlap sphere on the Switch layer
- `Bridge` — starts `_retractOffset` (default 20) units **above** its authored position; lowers smoothly at `_lowerSpeed` u/s when its plate ID activates. Uses a coroutine (`MoveTo`) not a snap.
- `ReunionTrigger` — fires `RaiseReunionAchieved` when both Scarlet (tag) and Dani (tag) are inside simultaneously. Creates a gold floor disc marker in `Awake`. Trigger size set by `LevelData.triggerSize`.
- `CheckpointTrigger` — one-shot; registers per-character spawn offsets with `CheckpointManager`
- `Hazard` — `OnTriggerEnter` on Character layer → `RaiseCharacterFailed` → `GameState.Failure`
- `PushBoulder` — a Rigidbody-based prop. Scarlet pushes it via `OnControllerColliderHit`. Has drag=6, constraints freeze Y position and all rotations.

### Level Sequence
`LevelSequenceController` (DontDestroyOnLoad, in `World1_Puzzle1` scene) listens for `OnReunionAchieved` and loads the next scene from its ordered `_levelScenes` array after a `_completionDelay` (2.5 s). After all puzzles, returns to MainMenu.

### Economy & UI Systems
- **Coins** — `CoinPickup` → `CoinManager` (DontDestroyOnLoad — coins persist across puzzle scenes). Two sinks: respawn-in-place and progressive hints.
- **Hints** — authored per-level in `LevelData.hints` (3 tiers). `HintManager` must point to the correct scene's `LevelData` — verify after any scene duplication.
- **UI uses legacy `UnityEngine.UI.Text`, not TextMeshPro** — see Project Gotchas.
- **MainMenuController** builds its own Canvas entirely in code (no TMP dependency). Disables all pre-existing Canvas objects in `Awake` and creates its own EventSystem if none exists.
- **PuzzleCompleteUI** also builds itself in code; attached to a plain GameObject in each puzzle scene (not DontDestroyOnLoad).

### Data Layer (ScriptableObjects)
- `CharacterData`: CharacterType, DisplayName, MoveSpeed, RunSpeed, AnimatorController
- `CoinSystemData`: RespawnCost, Hint1/2/3Cost
- `LevelData`: scarletStart/daniStart (Vector3), `platforms` (`PlatformDef` list), `objects` (`LevelObjectData` list), `hints`, skyColor
- `LevelObjectType` enum (in `LevelObjectData.cs`): Scarlet=0, Dani=1, Gate=2, Lever=3, Coin=4, ReunionTrigger=5, Checkpoint=6, RopeTrigger=7, Bridge=8, PushBoulder=9
- `LevelPrefabRegistry`: maps platform/object types → prefabs for `LevelBuilder`

### Camera
`Main Camera` uses `IsometricCameraFollow` to follow the active character at a fixed isometric offset. Input mapping: screen-right = world +X, screen-up = world +Z.

---

## Character & Asset Pipeline

### Character Import (`/character-importer`)
Raw Meshy ZIP → Blender (decimate to 20k tris, build 22-bone Humanoid rig, weight paint) → FBX export → Unity (Humanoid avatar, URP Lit material, Animator controller).
- Characters face their move direction via `transform.rotation` in `CharacterBase` — no sprite flip.
- Kevin Iglesias **Human Basic Motions FREE** pack provides Idle, Walk, and Run animations. Run clips: `HumanF@Run01_Forward.fbx` (female), `HumanM@Run01_Forward.fbx` (male). Both Scarlet and Dani use the female run.

### Prop Import (`/asset-pipeline`)
Raw Meshy ZIP → Blender → Unity (BoxCollider, URP Lit material, prefab).

---

## Field & Style Conventions

- `[SerializeField] private` for all inspector fields — never `public` fields on MonoBehaviours
- Underscore prefix for private members: `_moveSpeed`
- `public Type PropertyName => _field;` for externally-readable values
- Cache all `GetComponent` in `Awake()` — never in `Update`/`FixedUpdate`
- No LINQ in gameplay loops
- No `FindObjectOfType` at runtime — use events or serialised references
- `FindAnyObjectByType` (not `FindFirstObjectByType`) for Unity 6 compatibility

---

## Active Scenes

| Scene | Purpose |
|-------|---------|
| `MainMenu` | Title screen — built entirely in code by `MainMenuController` |
| `World1_Puzzle1` | Side by Side: push boulder off reunion spot, both characters step on it |
| `World1_Puzzle2` | The Gap: Dani pulls lever → bridge falls from above → Scarlet crosses |

All three scenes must be in **Build Settings** and the **Active Build Profile** for `SceneManager.LoadScene()` by name to work. Use `Tools/Add Scenes To Build` editor script if they disappear.

---

## Project Gotchas

- **UI text: use legacy `UnityEngine.UI.Text`, not TextMeshPro.** The TMP font atlas renders as tofu (black boxes) in game view. Use `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` in code. All UI classes (`HUDController`, `FailureUI`, `HintUI`, `PuzzleCompleteUI`, `MainMenuController`) follow this pattern.
- **URP pipeline gets cleared on asset-pack import.** Run `Tools/Fix URP Pipeline` (editor script) to reassign `Assets/Settings/UniversalRP.asset` to all Graphics + Quality tiers. Characters go pink when this happens.
- **TagManager gets wiped on package import.** The StepHeight package import wiped all custom tags and layers. Required tags: `Dani`, `Scarlet`. Required layers: `Character` (8), `Ground` (9), `Stackable` (10), `Switch` (11). Verify after any new package import — if `LayerMask.NameToLayer("Character")` returns -1, coins, the ReunionTrigger, and Hazards silently stop working.
- **`?.` on Unity Object fields is unsafe.** An unassigned serialized `Object` field is Unity "fake null" — `?.` does not short-circuit and throws. Use explicit `if (x != null)` checks.
- **Character baked scene positions matter.** Characters have a scene-level Transform position that applies on the very first frame before `LevelBuilder.Awake()` runs. If that position is inside a Hazard trigger (e.g. the gap in Puzzle 2), the fail screen fires immediately. Always set the baked Transform to a safe position that matches `LevelData.scarletStart`/`daniStart`.
- **`OnControllerColliderHit` is required for boulder push.** `CharacterController` does NOT automatically push Rigidbodies. `ScarletController.OnControllerColliderHit` applies `ForceMode.Force` per contact frame. Dani cannot push boulders — only `ScarletController` has this method.
- **`Physics.SyncTransforms()` is required after `Instantiate`.** Floor tile colliders don't register with the physics engine until the next FixedUpdate unless `Physics.SyncTransforms()` is called. `LevelBuilder` calls it between `PlaceObjects()` and `PlaceCharacters()`.

---

## Skills Available

| Skill | When to use |
|---|---|
| `/character-importer <Name> <zipPath>` | Import a Meshy character through Blender to Unity |
| `/asset-pipeline <Name> <type> <zipPath>` | Import a prop/env through Blender to Unity |
| `/level-design <LevelName>` | Design a puzzle level (produces GDD, Meshy prompts, Unity Blueprint) |
| `/profiling-workflow` | Establish frame budget and profile on-device |

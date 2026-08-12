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
├── game/One Way Together/           ← Unity project root
│   └── Assets/
│       ├── Scripts/                 ← all C# (namespace: OneWayTogether.*)
│       ├── Art/Characters/          ← FBX + textures + materials
│       ├── Art/Props/               ← prop FBX + textures + materials
│       ├── Prefabs/Props/           ← ready-to-drag prefabs
│       ├── Prefabs/Env/             ← environment prefabs (e.g. CrawlTunnelEntrance)
│       ├── Animation/               ← ScarletAnimator.controller, DaniAnimator.controller
│       ├── Input/                   ← OneWayTogetherControls.inputactions
│       ├── Prefabs/Puzzle/          ← Gate, Lever, Coin, etc.
│       ├── Scenes/                  ← MainMenu, World1_Level1
│       └── ScriptableObjects/
│           ├── Characters/          ← ScarletData.asset, DaniData.asset
│           ├── Coins/               ← CoinSystemData.asset
│           └── Levels/              ← World1_Level1.asset (LevelData) + LevelPrefabRegistry
├── sprints/                         ← sprint backlog markdown (SPRINTS.md index + sprint-NN-*.md)
└── .claude/
    ├── agents/                      ← unity-senior-developer, unity-code-assistant, unity-code-reviewer, sprint-planner
    ├── foundational/                ← project-context.md, tech-standards.md
    └── skills/                      ← character-importer, asset-pipeline, level-design, profiling-workflow
```

**Levels are data-driven, not hand-placed.** `LevelBuilder` (in `Scripts/Core/`) reads a `LevelData` ScriptableObject at `Awake` and instantiates the floor, walls, props, and puzzle objects from a `LevelPrefabRegistry`. Do not build levels by hand in the scene — author/edit the `LevelData` asset.

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
GameEvents.RaiseActiveCharacterChanged(CharacterType)  // fires on Tab switch
GameEvents.RaiseReunionAchieved()                      // puzzle complete
GameEvents.RaisePressurePlateChanged(string id, bool)  // puzzle mechanics
GameEvents.RaiseGateStateChanged(string id, bool)
```

### Character System
`CharacterBase` (abstract) owns: `CharacterController` movement on the XZ plane with **manually applied gravity** (no jump), facing rotation toward the move direction, animator driving, and `IsControllable` state. Characters receive input through `ReceiveMove(Vector2)`, `ReceiveInteract()`, and `ReceiveStopMove()` — they do **not** own `PlayerInput` components. Subclasses add character-specific abilities:
- `ScarletController`: push boulders, hold pressure plates, lift Dani
- `DaniController`: activate switches/levers, stack objects. **Climb (ropes/vines) is currently a stub** (`SetClimbingState` does nothing) pending a redesign for the isometric space.

**Input flow:** `InputRouter` owns the `InputActionAsset` and subscribes to the `Move` / `Interact` / `SwitchCharacter` actions directly, then forwards to the active character via `ReceiveMove/ReceiveInteract`. On-screen mobile controls (`MobileInputBridge` + the joystick and SWAP/USE buttons on the `MobileControls` canvas) call the same `InputRouter` public methods (`SetMoveInput`, `TriggerInteract`, `TrySwitchCharacter`).

**Input is gated on game state:** `InputRouter` only forwards input while `GameState.Playing`, and stops both characters on any transition out of Playing (failure panel, pause, puzzle complete) so nobody keeps sliding.

### Input Router
`InputRouter` holds direct serialised refs to the `_scarlet` and `_dani` `CharacterBase` objects plus the `_actionAsset`. It tracks `ActiveCharacter`, raises `GameEvents.RaiseActiveCharacterChanged` on switch, and debounces `TrySwitchCharacter()` per frame. There is one shared action asset — no per-character `PlayerInput` / device-pairing dance.

### Puzzle System
Puzzle objects use **3D trigger colliders** (characters are 3D `CharacterController` capsules — no 2D physics):
- `PressurePlate` → broadcasts by `plateId` string → listened to by `Gate`
- `Lever` (implements `IInteractable`) → toggles plate events
- `ReunionTrigger` → completes the puzzle when Scarlet and Dani (by tag) are both inside simultaneously → raises `GameEvents.RaiseReunionAchieved`
- `CheckpointTrigger` → registers spawn positions with `CheckpointManager`
- `Hazard` → the "trap" failure cause: on character entry raises `GameEvents.RaiseCharacterFailed` → `GameManager` enters `GameState.Failure`
- `RopeTrigger` → intended to enable Dani's climb state (climb itself is a stub — see Character System)

### Economy & UI Systems
- **Coins** — `CoinPickup` → `CoinManager` (count, spend). Two sinks: **respawn-in-place** (`FailureUI`, costs `RespawnCost`) and **progressive hints** (`HintManager` + `HintUI`, tiers cost `Hint1/2/3Cost`).
- **Hints** are authored per-level in `LevelData.hints` and revealed in order (vague → specific → full).
- **UI uses legacy `UnityEngine.UI.Text`, not TextMeshPro** — see Project Gotchas.

### Data Layer (ScriptableObjects)
- `CharacterData`: CharacterType, DisplayName, MoveSpeed, AnimatorController (no jump/ground fields — removed for the isometric model)
- `CoinSystemData`: RespawnCost, Hint1/2/3Cost
- `LevelData`: scarletStart/daniStart (Vector3), `platforms` (`PlatformDef` list), `objects` (`LevelObjectData` list), `hints` (list of strings), skyColor
- `LevelPrefabRegistry`: maps platform/object types → prefabs for `LevelBuilder`

### Camera
`Main Camera` uses `IsometricCameraFollow` to follow the active character at a fixed isometric offset, switching target on `GameEvents.OnActiveCharacterChanged`. `CameraController` + Cinemachine `SinglePlayerCam` / `CoopCam` also exist for co-op framing.

---

## Character & Asset Pipeline

### Character Import (`/character-importer`)
Raw Meshy ZIP → Blender (decimate to 20k tris, build 22-bone Humanoid rig, weight paint) → FBX export → Unity (Humanoid avatar, URP Lit material, Animator controller).
- Output FBX: `art/characters/<Name>/<Name>_processed.fbx`
- Unity: `Assets/Art/Characters/<Name>/`
- Animator controllers: `Assets/Animation/<Name>/<Name>Animator.controller`
- Characters face their move direction via `transform.rotation` in `CharacterBase` (isometric 3D) — no sprite flip.
- Input is routed centrally by `InputRouter` (which uses `InputAction.CallbackContext`); characters have no `PlayerInput` component and no `InputValue` handlers.

### Prop Import (`/asset-pipeline`)
Raw Meshy ZIP → Blender (validate, decimate to ~1000 tris, fix Z-origin, export) → Unity (BoxCollider, URP Lit material, prefab).
- Output: `art/props/<Name>/`, Unity prefab at `Assets/Prefabs/Props/<Name>.prefab`
- FBX export settings (non-negotiable): `axis_forward='-Z'`, `axis_up='Y'`, `FBX_SCALE_ALL`, `bake_space_transform=True`

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

## Key Scene: World1_Level1

The scene is a **data-driven shell** — `LevelBuilder` constructs the level at runtime from `Assets/ScriptableObjects/Levels/World1_Level1.asset`. To change layout, edit that `LevelData` asset, not the scene.

- **Scarlet / Dani**: 3D `CharacterController` capsules on the `Character` layer, positioned at `LevelData.scarletStart` / `daniStart` (Vector3 on the XZ plane) by `LevelBuilder`.
- **Ground / walls / props**: instantiated by `LevelBuilder` from `platforms` (floor tiles, stone walls, vegetation, rocks) — 3D meshes with 3D colliders, no tilemap.
- **Puzzle objects**: instantiated from `LevelData.objects` (Gate, Lever, Coin, ReunionTrigger, Checkpoint, Hazard) by type + Vector3 position + `yRotation`.
- **Managers present in-scene**: `GameManager`, `CheckpointManager`, `CoinManager`, `HintManager`, `InputRouter` (holds `_scarlet`/`_dani`/`_actionAsset`), `LevelBuilder`, and the `MobileControls` canvas (joystick, SWAP/USE buttons, coin counter, `FailureUI`, `HintUI`).
- **Named layers** (registered in TagManager): `Character`, `Ground`, `Stackable`, `Switch`.

---

## Project Gotchas

- **UI text: use legacy `UnityEngine.UI.Text`, not TextMeshPro.** The project's TMP font atlas renders as tofu (empty boxes) in the game view on this setup. The built-in `LegacyRuntime.ttf` always renders — assign it in code via `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` when building UI through tooling (the serialized `font` property can't be set via MCP). `HUDController`, `FailureUI`, and `HintUI` all follow this pattern.
- **URP pipeline gets cleared on asset-pack import.** Importing packs (Joystick Pack, UISystem, etc.) has repeatedly reset the URP Render Pipeline Asset in Project Settings, dropping rendering to Built-in (the board turns pink). Fix: reassign `Assets/Settings/UniversalRP.asset` in **Graphics** and all **Quality** tiers.
- **`?.` on Unity Object fields is unsafe.** An unassigned serialized `Object` field is Unity "fake null" — `?.` does not short-circuit and throws. Use explicit `if (x != null)` checks.
- **`CLAUDE.md` vs. code:** treat the **code as source of truth** for movement/physics. Earlier docs described a 2D Rigidbody2D/Tilemap/jump model that no longer exists.

---

## Skills Available

| Skill | When to use |
|---|---|
| `/character-importer <Name> <zipPath>` | Import a Meshy character through Blender to Unity |
| `/asset-pipeline <Name> <type> <zipPath>` | Import a prop/env through Blender to Unity |
| `/level-design <LevelName>` | Design a puzzle level (produces GDD, Meshy prompts, Unity Blueprint) |
| `/profiling-workflow` | Establish frame budget and profile on-device |

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Commit Policy

**Never create a git commit without explicit user authorization.** Stage changes, summarize what would be committed, and wait for the user to say "commit" or "commit and push" before running `git commit`. The same applies to `git push`.

---

## Project Overview

**One Way Together** — a 2D cooperative puzzle platformer for iOS and Android. Two siblings, Scarlet (big) and Dani (small), are lost and trying to find their way home. Every puzzle requires both characters. Win condition: both characters must enter a `ReunionTrigger` simultaneously — not just reach an exit.

**Engine:** Unity 6 LTS, URP, targeting iOS + Android.

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
│       ├── Scenes/                  ← MainMenu, World1_Level1
│       └── ScriptableObjects/
│           ├── Characters/          ← ScarletData.asset, DaniData.asset
│           └── Coins/               ← CoinSystemData.asset
└── .claude/
    ├── agents/                      ← unity-senior-developer, unity-code-assistant, unity-code-reviewer
    ├── foundational/                ← project-context.md, tech-standards.md
    └── skills/                      ← character-importer, asset-pipeline, level-design, profiling-workflow
```

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
`CharacterBase` (abstract) owns: Rigidbody2D physics, jump queue, ground detection, animator driving, and `IsControllable` state. Subclasses add character-specific abilities:
- `ScarletController`: push boulders, hold pressure plates, lift Dani
- `DaniController`: crawl tunnels, climb ropes, activate switches, stack objects

**Input flow:** `PlayerInput` (SendMessages, `InputValue` signature) → `CharacterBase.OnMove/OnJump/OnInteract/OnSwitchCharacter` → physics or relay to `InputRouter`.

**Character switching:** Both `PlayerInput` components receive Tab simultaneously. `OnSwitchCharacter` is guarded by `IsControllable` (only active character forwards the call). `InputRouter.TrySwitchCharacter()` has a per-frame debounce. On switch, `InputRouter` calls `PlayerInput.DeactivateInput/ActivateInput` — Unity's Input System only pairs devices to the active `PlayerInput`.

### Input Router
`InputRouter` holds direct serialised refs to `_scarletInput` and `_daniInput`. It tracks `ActiveCharacter` and raises `GameEvents.RaiseActiveCharacterChanged` on switch. The inactive character's `PlayerInput` is deactivated so the keyboard device follows the active character.

### Puzzle System
Puzzles are built from prefabs in `Assets/Prefabs/Props/`:
- `PressurePlate` → broadcasts by `plateId` string → listened to by `Gate`
- `Lever` (implements `IInteractable`) → toggles plate events
- `ReunionTrigger` → completes the level when both characters are inside
- `CheckpointTrigger` → registers spawn positions with `CheckpointManager`
- `RopeTrigger` → enables Dani's climb state on enter/exit

### Data Layer (ScriptableObjects)
- `CharacterData`: MoveSpeed, JumpForce, GroundCheckRadius, GroundLayer, AnimatorController
- `CoinSystemData`: RespawnCost, Hint1/2/3Cost

### Camera
`CameraController` manages two Cinemachine cameras: `SinglePlayerCam` (follows active character, priority 20) and `CoopCam` (frames both via TargetGroup, priority 10).

---

## Character & Asset Pipeline

### Character Import (`/character-importer`)
Raw Meshy ZIP → Blender (decimate to 20k tris, build 22-bone Humanoid rig, weight paint) → FBX export → Unity (Humanoid avatar, URP Lit material, Animator controller).
- Output FBX: `art/characters/<Name>/<Name>_processed.fbx`
- Unity: `Assets/Art/Characters/<Name>/`
- Animator controllers: `Assets/Animation/<Name>/<Name>Animator.controller`
- Model rotation: `localRotation = (0, 90, 0)` on the child model GO for correct sideways walk
- Flip mechanic: `transform.Rotate(0, 180, 0)` on root (not scale.x flip — avoids normal inversion)
- **Input method signatures must use `InputValue`, not `InputAction.CallbackContext`** (SendMessages mode requirement)

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

- **Scarlet**: root at Y=1.9, CapsuleCollider2D (0.6×1.8), GroundCheck child at local Y=-0.9
- **Dani**: root at Y=1.432, localScale=(0.72,0.72,0.72), CapsuleCollider2D (0.45×1.2), GroundCheck at local Y=-0.6
- **Ground**: Tilemap on layer `Ground`, TilemapCollider2D + CompositeCollider2D, Rigidbody2D Static
- **ReunionTrigger**: at X=8 — puzzle complete when both characters are inside simultaneously
- **InputRouter**: holds `_scarletInput` and `_daniInput` serialised refs — must be wired in Inspector

---

## Skills Available

| Skill | When to use |
|---|---|
| `/character-importer <Name> <zipPath>` | Import a Meshy character through Blender to Unity |
| `/asset-pipeline <Name> <type> <zipPath>` | Import a prop/env through Blender to Unity |
| `/level-design <LevelName>` | Design a puzzle level (produces GDD, Meshy prompts, Unity Blueprint) |
| `/profiling-workflow` | Establish frame budget and profile on-device |

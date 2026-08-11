# Project Context — One Way Together

## Game Summary
Two siblings (Scarlet and Dani) are lost and trying to find their way home.
Every puzzle requires both characters. Win condition: REUNITE at a designated trigger — not just reach an exit.

## Repository Layout
```
Nezzy's Game/
├── .claude/
│   └── foundational/
│       ├── project-context.md    ← this file
│       └── tech-standards.md
├── game/                         ← Unity project root
│   └── Assets/
│       ├── Scripts/
│       │   ├── OneWayTogether.asmdef
│       │   ├── Core/             GameManager, CheckpointManager
│       │   ├── Characters/       CharacterBase, ScarletController, DaniController, IInteractable
│       │   ├── Input/            InputRouter
│       │   ├── Camera/           CameraController
│       │   ├── Collectibles/     CoinPickup, CoinManager, SecretKeyPickup
│       │   ├── Puzzle/           CheckpointTrigger, ReunionTrigger, PressurePlate, Gate, Lever, RopeTrigger
│       │   ├── UI/               HUDController, FailureUI
│       │   ├── Events/           GameEvents (static event bus), GameState enum, CharacterType enum
│       │   └── Data/             CharacterData (SO), CoinSystemData (SO)
│       ├── Input/
│       │   └── OneWayTogetherControls.inputactions
│       ├── Scenes/               MainMenu, World1_Level1
│       ├── Prefabs/
│       ├── Sprites/
│       ├── Tilemaps/
│       ├── Audio/
│       └── ScriptableObjects/
├── README-Design-Feedback.md
├── README-Puzzle-Design.md
└── README.md
```

## Platform
iOS + Android. Unity 6 LTS, URP 2D, Pixel Perfect Camera, Cinemachine.

## Sprint State
Phase 1 — Foundation scaffold complete. All core scripts written and on disk.
Next: open Unity, create the project, import packages, set up scenes via MCP.

## Unity MCP Workflow
MCP tools are available for Unity Editor automation. Use the unity-mcp-skill before
any Unity Editor task. Key tools: create_script, manage_gameobject, manage_packages,
manage_scene, batch_execute, manage_camera.

## Characters
- **Scarlet**: push/roll boulders, hold pressure plates, lift Dani
- **Dani**: crawl through tunnels, climb ropes/vines, activate switches, stack objects

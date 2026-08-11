# Technical Standards — One Way Together

## Namespace
All runtime code: `OneWayTogether.<SubNamespace>`
Sub-namespaces: Core, Characters, Input, Camera, Collectibles, Puzzle, UI, Events, Data

## Field Conventions
- `[SerializeField] private` for inspector-exposed fields — never `public` fields
- Private members prefixed with underscore: `_moveSpeed`, `_stats`
- Properties for computed / externally-readable values: `public float MoveSpeed => _moveSpeed;`

## Architecture Patterns
- **Event bus**: `GameEvents` (static) — all cross-system communication
- **Data layer**: ScriptableObjects in `Assets/ScriptableObjects/`
  - CharacterData — movement, jump, ground detection per character
  - CoinSystemData — respawn cost, hint tier costs
- **MonoBehaviours**: runtime state, physics, animation driving
- **Singletons**: GameManager only. CoinManager uses a scene-scoped Instance pattern.
- Cache all `GetComponent` in `Awake()` — never call in Update/FixedUpdate

## Performance Rules
- No LINQ in gameplay loops
- No `FindObjectOfType` in runtime code — use events or scene-assigned references
- No string concatenation in Update — use `SetText(format, value)` with TMP
- No `GetComponent` in Update/FixedUpdate
- Coroutines: always `StopCoroutine` on `OnDestroy` if running

## Input
New Input System exclusively. No `Input.GetKey` / legacy input anywhere.
Input Action Asset: `Assets/Input/OneWayTogetherControls.inputactions`
Actions: Move (Vector2), Jump, Interact, SwitchCharacter, Pause
Control schemes: Keyboard&Mouse, Gamepad, Touch

## Layers (set up in Unity Editor)
- `Character` — both Scarlet and Dani's colliders
- `Ground` — tilemap and static geometry
- `Stackable` — crates, barrels Dani can carry
- `Switch` — levers and buttons Dani can activate

## Tags (set up in Unity Editor)
- `Scarlet` — on Scarlet's root GameObject
- `Dani` — on Dani's root GameObject

## Key Script APIs

### GameEvents (static event bus)
```csharp
GameEvents.RaiseGameStateChanged(GameState state)
GameEvents.RaiseActiveCharacterChanged(CharacterType type)
GameEvents.RaiseCharacterFailed(CharacterType type)
GameEvents.RaiseReunionAchieved()
GameEvents.RaiseCheckpointActivated(Vector3 pos)
GameEvents.RaiseCoinCollected(int total)
GameEvents.RaiseCoinsSpent(int total)
GameEvents.RaiseSecretKeyCollected(string keyId)
GameEvents.RaisePressurePlateChanged(string plateId, bool isActive)
GameEvents.RaiseGateStateChanged(string gateId, bool isOpen)
```

### CheckpointManager
```csharp
RegisterCharacter(CharacterType type, Transform t, Vector3 startPos)
RegisterCheckpoint(CharacterType type, Vector3 pos)
ResetToCheckpoint()
RespawnInPlace()
```

### CoinManager (scene-scoped singleton)
```csharp
CoinManager.Instance.CoinCount
CoinManager.Instance.CollectCoin(AudioClip clip = null)
CoinManager.Instance.TryRespawnInPlace() → bool
CoinManager.Instance.TryPurchaseHint() → int (tier 1/2/3, or 0 if insufficient)
```

### CharacterBase
```csharp
CharacterType CharacterType { get; }   // abstract
bool IsGrounded { get; }
bool IsControllable { get; }
Vector2 Velocity { get; }
```

### DaniController
```csharp
BeginLiftedState(Transform scarlet, Vector3 offset, float launchForce)
EndLiftedState()
SetClimbingState(bool canClimb)
bool IsCrawling { get; }
bool IsClimbing { get; }
```

### ScarletController
```csharp
ReleaseDani()
```

### IInteractable
```csharp
void Interact(CharacterBase source)
```

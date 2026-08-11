---
name: level-design
description: Full level design pipeline for One Way Together. Produces three documents per level — a Puzzle Design Document, a Meshy asset prompt file, and a Unity Blueprint — with approval gates before any files are written. Invoke with the level name and any context; the skill guides the rest.
---

# Level Design Skill

Runs the complete level design pipeline for a new puzzle level or world. Produces three documents that cover everything needed to build the level.

## Usage

```
/level-design [level name] [optional: world / theme / puzzle mechanic notes / difficulty]
```

**Examples:**
- `/level-design World1_Level3`
- `/level-design World1_Level3 — introduce boulder + tunnel combination, medium difficulty`
- `/level-design World2_Level1 — first water mechanic level, gentle introduction`

The more context provided, the fewer clarifying questions during execution.

---

## Context (Read First)

Before any design work, read these files to understand the current game state:

- `README-Design-Feedback.md` — confirmed decisions, character abilities, art direction
- `README-Puzzle-Design.md` — design rules, existing World 1 puzzles, mechanic legend
- `README-Game-Discovery.md` — original discovery document and USP

**Key design rules (always enforce):**
1. Neither character (Scarlet or Dani) can solve the puzzle alone
2. One character must enable the other — every puzzle has an interlocking solution
3. No text instructions — obstacles must be visually obvious
4. One new mechanic per puzzle (early game); combinations come after both parts are learned separately
5. Win condition: both characters REUNITE at a designated point (not just reach an exit)

---

## Pipeline Overview

| Gate | What happens | Who approves |
|---|---|---|
| **Gate 1** | Puzzle concept: mechanic, emotional beat, difficulty | User |
| **Gate 2** | Detailed puzzle design + ASCII layout | User |
| **Gate 3** | Meshy asset gap analysis (new vs. existing) | User |
| **Write** | All three files written | User (final confirm) |

---

## Step 1: Context Gathering

Ask the user for any missing context:

1. **World and level number** — which world, which slot in the world
2. **Mechanic being introduced or combined** — what's new in this level vs. the previous one
3. **Difficulty target** — gentle introduction, medium, hard, or finale-level
4. **Emotional beat** — what should the player feel at the end (relief, triumph, "aha", trust)
5. **Any specific assets needed** — anything that doesn't exist yet

If the user invoked with enough context, skip questions and proceed to Gate 1.

---

## Step 2: Puzzle Concept Brief (→ Gate 1)

Present a short concept before any detailed design:

```markdown
## Puzzle Concept: [Level Name]

**World:** [World N — theme]
**Slot:** [Level N of M in this world]
**Difficulty:** [Gentle / Medium / Hard / Finale]

### Mechanic Focus
[Which mechanic(s) this puzzle is built around — one sentence each]

### The "Aha" Moment
[The exact moment the player will feel the puzzle click — what they realize and when]

### Emotional Target
[Heartwarming / Concern / Triumph / Trust / Relief — and why this level earns it]

### Cooperation Structure
**Scarlet's role:** [What she does in this puzzle]
**Dani's role:** [What she does in this puzzle]
**The enable moment:** [The specific moment one character makes the other's path possible]

### Separation
[Yes / No — does the puzzle involve the characters being out of sight of each other?]
[If yes: how does the player know their other character is doing something useful?]
```

**Gate 1:** Present the concept. Wait for user approval or corrections. Do not proceed to detailed design until approved.

---

## Step 3: Detailed Puzzle Design (→ Gate 2)

Write the complete puzzle design using the established format from `README-Puzzle-Design.md`.

```markdown
## [Level Name]

**Teaches:** [Mechanic or combination being introduced]
**Emotion:** [Target emotional beat]

### ASCII Layout

Use the mechanic legend from README-Puzzle-Design.md:

```
S      Scarlet start
D      Dani start
█      Wall / ground
▓      Platform
○      Boulder
□      Stackable object
≡      Pressure plate
[  ]   Gate / door
🗝      Secret key
~      Rope or vine
...    Narrow tunnel (Dani only)
^      Lever or switch
=      Bridge or extended platform
★      Reunion point
↑      Lift point
```

[Draw the level layout]

### Step-by-Step Solution

1. [First action — who does what]
2. [Second action — result and what it enables]
3. [Continue until both reach ★]

### Why Neither Can Solo

[Explicit statement of why Scarlet alone fails and why Dani alone fails]

### Design Notes

[Any specific notes about timing, camera, animation moments, or trust beats]

### Checkpoint Placement

[Where checkpoints should sit relative to the puzzle structure]

### Coin Placement (Hints)

[Where coins are hidden — off the main path, behind exploration moments. Never blocking the solution.]

### Secret Key Location (if any)

[Where the secret key is hidden and what it unlocks]
```

**Gate 2:** Present the full puzzle design. Wait for user approval. Incorporate any corrections before proceeding.

---

## Step 4: Meshy Asset Gap Analysis (→ Gate 3)

Before writing any Meshy prompts, cross-reference what already exists.

**Check existing assets in:**
- `art/` — any already-imported or referenced art
- `Assets/Art/` — anything already in Unity
- `game/Assets/Art/Placeholder/` — placeholder tiles and assets

For each prop or environment piece the puzzle needs:

| Asset | Status | Notes |
|---|---|---|
| [Asset Name] | ✅ Exists / ⚠️ Check / ❌ Missing | [Unity path or "needs Meshy"] |

**Only write Meshy prompts for ❌ Missing assets.**

Present the gap table first and wait for Gate 3 approval before writing prompts.

### Meshy Prompt Format

```markdown
### Asset: [Asset Name]

**Unity asset name:** `prop_<name>.fbx` or `env_<name>.fbx`
**Type:** prop / env
**Poly budget:** ~[N] tris
**Priority:** HIGH / MEDIUM / LOW — [one-line reason]

#### Meshy Text Prompt

[Prompt text under 800 characters. Be specific: material, shape, proportions, style keywords.
For One Way Together: "cartoon stylized 3D game asset, pixel art + storybook aesthetic, warm saturated colors"]

#### Art Direction Notes

- [Key visual feature]
- [Gameplay-critical size or collision note]
- [What to ask for if Meshy misses the key detail]

#### Post-Processing Notes

- No rig needed. Base at Y=0.
- Target [N] tris.
- Process through /asset-pipeline before Unity import.
```

---

## Step 5: Unity Blueprint

A room-by-room layout document for the unity-senior-developer agent to build the scene.

```markdown
# Unity Blueprint: [Level Name]

**Scene file:** `Assets/Scenes/[WorldN]/[LevelName].unity`
**Asset path:** `Assets/Art/`

---

## Scene Layout

### Tilemap / Ground

[Which tilemap layers are needed. What tiles to use. Approximate dimensions.]

### Character Start Positions

- Scarlet: [x, y, z]
- Dani: [x, y, z]

### Interactive Objects

| Object | Component | Position | Config |
|---|---|---|---|
| [Name] | [Script] | [x, y, z] | [Key settings] |

### Checkpoint Positions

| Checkpoint | Position | Notes |
|---|---|---|
| Start | [x, y, z] | [Placed before puzzle begins] |
| Mid (if needed) | [x, y, z] | [Placed after major first step] |

### Coin Positions

| Coin | Position | Notes |
|---|---|---|
| [1] | [x, y, z] | [Hidden location] |

### Reunion Trigger

- Position: [x, y, z]
- Size: [w × h]

### Props and ENV

| Asset | Prefab | Position | Rotation | Notes |
|---|---|---|---|---|
| [Name] | [path] | [x, y, z] | [y°] | [Scale, collider notes] |

### Camera Notes

[Any special camera behavior for this level — tight spaces, trust moments, separation framing]

### Unity Notes

- [Any script configuration beyond defaults]
- [Physics layer assignments]
- [Lighting or material overrides]
```

---

## Step 6: Final Review and File Write

Before writing files, confirm:

```
## Ready to write — [Level Name]

**Output folder:** docs/levels/[level-slug]/

**Files to create:**
- puzzle-design.md    (solution walkthrough, ASCII layout, design notes)
- meshy-prompts.md    ([N] Meshy orders, [N] existing assets confirmed)
- unity-blueprint.md  (scene setup, object positions, Unity notes)

Proceed?
```

Write all three files after confirmation.

---

## Quality Rules

**Puzzle rules:**
- If either character can solve the puzzle alone, it fails the core design rule — redesign
- The enable moment must be explicit and readable without a tutorial
- Every puzzle must have an "aha" moment — the realization of why the solution works
- Early puzzles teach one thing; later puzzles combine previously learned things
- Puzzles solvable in ~30 seconds once understood (tight layout, 1–2 screens wide)

**Coin rules:**
- Never put coins between the player and the solution — coins reward exploration, not luck
- Hard puzzles deserve more coins nearby (more safety valve resources)
- Coins should feel like rewards for curiosity, not consolation prizes

**Meshy rules:**
- Every prompt must be under 800 characters (hard limit)
- Poly budget must be stated per asset
- Gameplay-critical props (platforms, boulders, pressure plates) are HIGH priority

**Blueprint rules:**
- Every scene gets exact position coordinates for interactive objects
- All collider settings that differ from defaults must be explicitly stated
- The Reunion Trigger position and size must always be specified

---

## Output File Structure

```
docs/levels/[level-slug]/
  puzzle-design.md      ← Full puzzle design with ASCII layout and solution
  meshy-prompts.md      ← Asset gap analysis + Meshy prompts for missing assets
  unity-blueprint.md    ← Scene construction instructions for the developer
```

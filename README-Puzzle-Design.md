# Puzzle Design — One Way Together

> Scarlet and Dani. Siblings. Lost. Finding their way home.

------------------------------------------------------------------------

# Design Rules

Every puzzle must follow these rules. No exceptions.

1. **Neither character solves it alone.** If Scarlet or Dani can reach
   the exit without the other, the puzzle needs another pass.

2. **One character enables the other.** Every puzzle has at least one
   moment where one sibling creates the opportunity for the other.

3. **No text instructions.** The player learns by doing. Obstacles
   should be visually obvious — a boulder Scarlet can push, a tunnel
   only Dani fits through.

4. **One new mechanic per puzzle.** Early puzzles introduce exactly one
   idea. Later puzzles combine previous ideas.

5. **Separation must be resolved.** If the characters split up, the
   puzzle ends with them together again. Every reunion should feel
   earned.

------------------------------------------------------------------------

# Mechanic Legend (ASCII Diagrams)

```
S      Scarlet starting position
D      Dani starting position
█      Solid wall or ground
▓      Platform (can stand on)
○      Boulder (Scarlet can push)
≡      Pressure plate (holds when stood on)
[  ]   Gate / door (open or closed)
~      Rope or vine (Dani can climb)
...    Narrow tunnel (Dani only)
^      Lever or switch
=      Bridge or extended platform
★      Reunion / level exit
↑      Lift point (Scarlet lifts Dani here)
```

------------------------------------------------------------------------

# World 1 — Forest

## Puzzle 1: Side by Side

**Teaches:** Basic movement. Characters travel together.
**Emotion:** Warm. Comfortable. Establishing the bond.

```
█████████████████████████████████████████
█                                       █
█  S  D                              ★  █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
█████████████████████████████████████████
```

**What happens:**
- Scarlet and Dani start side by side.
- The path to the reunion point is clear and flat.
- No obstacles. No challenge.
- This is the game saying hello.

**Design note:** The first puzzle should feel like a warm handshake.
Players learn the controls, get comfortable with the characters, and
feel good. Save challenge for later.

------------------------------------------------------------------------

## Puzzle 2: The Gap

**Teaches:** Separation. Dani enables Scarlet.
**Emotion:** First moment of worry — then relief.

```
█████████████████████████████████████████
█                 █         █           █
█  S              █    D    █        ★  █
█▓▓▓▓▓▓▓▓▓▓       █▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓█
█████████  █       █████████████████████
           █   ~   █
           █▓▓▓▓▓▓▓█
           ^
```

**What happens:**
1. A gap in the floor separates the siblings.
2. Dani is already on the other side (or can jump the gap — Scarlet
   cannot).
3. A rope hangs from the ledge above, but it's tied up — held by a
   lever Dani can reach.
4. Dani pulls the lever. The rope drops.
5. Scarlet climbs the rope across.
6. They continue together.

**Design note:** This is the first time the player feels the separation
anxiety. Keep it brief. Dani's side should feel safe so the player
isn't worried about the wrong character.

------------------------------------------------------------------------

## Puzzle 3: The Boulder

**Teaches:** Scarlet's strength creates opportunities for Dani.
**Emotion:** Satisfying. "Oh — she does that."

```
█████████████████████████████████████████
█                                       █
█  S  D      ○    [  ]            ★     █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
              █   █
              ^   █
              █████
```

**What happens:**
1. A boulder blocks the path. Neither sibling can pass.
2. Scarlet pushes the boulder. It rolls into a pit to the side.
3. The pit contains a pressure plate — the boulder lands on it.
4. The pressure plate opens the gate ahead.
5. Both siblings walk through.

**Design note:** The player doesn't need to discover this — the boulder
is obviously pushable and the gate is obviously closed. The "aha" is
realizing the boulder lands exactly where it needs to.

------------------------------------------------------------------------

## Puzzle 4: The Tunnel

**Teaches:** Dani's size is a strength, not a limitation.
**Emotion:** Clever. Dani leads the way.

```
█████████████████████████████████████████
█                   █████████           █
█  S  D         [  ]█.......█       ★   █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓█
                          ^
```

**What happens:**
1. A locked gate blocks the path. A tunnel runs alongside — too narrow
   for Scarlet.
2. Dani crawls through the tunnel.
3. On the other side, Dani reaches a lever.
4. Dani pulls it. The gate opens.
5. Scarlet walks through. They reunite.

**Design note:** Scarlet should visually react when Dani goes through
the tunnel without her — a worried glance. Small animation that costs
nothing and pays off emotionally.

------------------------------------------------------------------------

## Puzzle 5: The Lift

**Teaches:** Scarlet physically enables Dani.
**Emotion:** Teamwork. They are stronger together than apart.

```
█████████████████████████████████████████
█            █████████████              █
█            █         ^  █             █
█            █▓▓▓▓▓▓▓▓▓▓▓▓█    ★        █
█            █             ============█
█  S  D      █                         █
█▓▓▓▓▓↑▓▓▓▓▓▓███████████████████████████
```

**What happens:**
1. A high ledge blocks the path — too tall for Dani to reach, and the
   ledge beyond is too narrow for Scarlet.
2. Scarlet lifts Dani to the ledge.
3. Dani runs along the top, reaches a lever.
4. Dani pulls it — a bridge extends below for Scarlet.
5. Scarlet crosses. They reunite beyond the ledge.

**Design note:** The lift should feel like a natural gesture — Scarlet
cups her hands, Dani steps in. Make this animation warm and sibling-like.

------------------------------------------------------------------------

## Puzzle 6: The Trust

**Teaches:** The trust mechanic — acting without seeing the other.
**Emotion:** Tension. Then relief.

```
█████████████████████████████████████████
█              [  ]█[  ]                █
█  S               █         D      ★   █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
                   █
        ^          ^
```

**What happens:**
1. A wall divides the siblings. Each has a gate that requires a lever
   from the other side to open.
2. Scarlet cannot see Dani. Dani cannot see Scarlet.
3. Scarlet pulls her lever — a sound plays on Dani's side (machinery
   activating, a gate creaking).
4. Dani sees her gate open. She walks through.
5. Dani pulls her lever — Scarlet's gate opens.
6. Scarlet walks through. They find each other.

**Design note:** Sound design carries this puzzle. The player on
Scarlet's side must trust that pulling the lever is doing something.
The reunion here should feel warmer than usual — they were apart
without being able to see each other.

------------------------------------------------------------------------

## Puzzle 7: First Combination

**Teaches:** Two mechanics together for the first time.
**Emotion:** Growing confidence. "We know how to do this."

```
████████████████████████████████████████
█               ████    █              █
█               █..█  ^ █              █
█         ○     █..█▓▓▓▓█    [  ]   ★  █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓████    █▓▓▓▓▓▓▓▓▓▓▓▓▓█
         ≡               
█████████████████████████████████████████
```

**What happens:**
1. A boulder sits on the path. A tunnel runs through a wall nearby.
   A gate blocks the exit.
2. Scarlet pushes the boulder onto a pressure plate — this unlocks the
   tunnel entrance.
3. Dani crawls through the tunnel.
4. On the other side, Dani reaches a lever that opens the gate.
5. Scarlet walks through the gate. They reunite.

**Design note:** Don't introduce both mechanics simultaneously — let
the player identify each step. The boulder first (familiar), the
tunnel second (familiar). The combination is the new idea.

------------------------------------------------------------------------

## Puzzle 8: Hold and Go

**Teaches:** Simultaneous action. Timing matters.
**Emotion:** Cooperative precision. This is where it starts to feel
like a real puzzle game.

```
████████████████████████████████████████
█                           █          █
█  S                 D      █    ★     █
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
             ≡          ^
           (holds     (opens
           bridge)    other door)
```

**What happens:**
1. A pressure plate in the middle of the room controls a bridge on
   Dani's side.
2. Scarlet must stand on the plate to keep the bridge extended.
3. While Scarlet holds the plate, Dani crosses the bridge.
4. Dani reaches a lever on the far side — pulling it locks the bridge
   in place and opens a second door near Scarlet.
5. Scarlet steps off the plate (bridge stays locked) and walks through
   her door.
6. They reunite.

**Design note:** The player needs to understand the sequence: Scarlet
first, then Dani, then Dani frees Scarlet. Let the environment
communicate this visually — arrows etched in stone, glowing lines,
or simply the obvious spatial arrangement.

------------------------------------------------------------------------

## Puzzle 9: The Long Way Around

**Teaches:** Multi-step solutions. The answer isn't immediately obvious.
**Emotion:** Stuck → thinking → breakthrough. The first real "aha."

```
████████████████████████████████████████
█  ^    ████████████████    █          █
█▓▓▓▓▓▓▓█              █▓▓▓█     ★    █
█       █   ○    ≡     █   █▓▓▓▓▓▓▓▓▓▓█
█  S D  █▓▓▓▓▓▓▓▓▓▓▓▓▓▓█   [  ]       █
████████                █              █
                        ^^^^^^^^^^^^^^^^
                        (series of levers,
                         one correct order)
```

**What happens:**
1. Room has multiple elements — a boulder, a pressure plate, a lever
   up high, a gate.
2. The "obvious" solution (boulder → plate → gate) doesn't work
   directly because the gate is on the wrong side.
3. Dani climbs to the high lever first — this reroutes where the
   boulder needs to go.
4. Scarlet pushes the boulder to the now-correct pressure plate.
5. Gate opens. They proceed.

**Design note:** This is the first puzzle designed to make the player
pause. That's fine. Frustration managed correctly becomes satisfaction.
The solution should feel logical in hindsight, not arbitrary.

------------------------------------------------------------------------

## Puzzle 10: World 1 Finale — The Old Bridge

**Teaches:** Everything combined. Full cooperation under mild time pressure.
**Emotion:** Triumph. Brief story beat. They can see the path forward.

```
████████████████████████████████████████
█        ~          ████   ○           █
█   ↑    ~   D  ...████  ≡    ^  [  ] ★█
█   S    ~      ...████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██                █
                       ████████████████
```

**What happens:**
1. Scarlet and Dani start at a crumbling bridge. The way forward
   requires all their skills.
2. Scarlet lifts Dani to a vine. Dani climbs up and across.
3. Dani crawls through a tunnel on the far side.
4. Dani finds a boulder — too heavy for her.
5. Dani pulls a lever that lowers a rope for Scarlet.
6. Scarlet climbs up, joins Dani on the high path.
7. Scarlet pushes the boulder onto a pressure plate.
8. The gate ahead opens.
9. Both walk through together and see — in the distance — the direction
   home.

**Story beat:** A brief moment. The siblings stand at the edge of the
forest. Somewhere ahead is the way back. They look at each other.
Then they keep going.

**Design note:** This puzzle should feel like a greatest-hits of
everything the player learned. Nothing new. Just fluent execution of
the mechanics that now feel natural. End it with the story beat — earn
the emotion with the gameplay first.

------------------------------------------------------------------------

# Difficulty Curve — World 1

| Puzzle | Mechanic Introduced        | Difficulty |
| ------ | -------------------------- | ---------- |
| 1      | Movement / together        | Tutorial   |
| 2      | Separation / rope          | Very easy  |
| 3      | Push boulder               | Easy       |
| 4      | Crawl tunnel               | Easy       |
| 5      | Lift                       | Easy       |
| 6      | Trust (can't see each other) | Medium   |
| 7      | Boulder + tunnel combined  | Medium     |
| 8      | Simultaneous action        | Medium     |
| 9      | Multi-step / non-obvious   | Hard       |
| 10     | All combined               | Hard       |

------------------------------------------------------------------------

# What Comes Next

World 2 introduces one new mechanic (water / buoyancy) and immediately
starts combining it with mechanics from World 1.

The difficulty baseline resets slightly at the start of each world —
the first puzzle teaches the new mechanic simply, then combinations
begin again.

By World 6 (Home), the player is fluent in all mechanics and the
puzzles combine everything without mercy.

------------------------------------------------------------------------

# Questions to Resolve Before Building

1. **Single player or co-op?** Who controls whom? One player switches
   between Scarlet and Dani, or two players?
2. **Failure state?** What happens when a character falls or gets
   stuck? Instant reset, rewind, or checkpoint?
3. **How long is each puzzle?** 30 seconds at a glance or 5-10 minutes
   of thinking?
4. **Are there collectibles beyond coins?** Hidden items, secret paths,
   lore pieces?
5. **Does Dani's yellow hoodie have any in-game significance?** Could
   be a visual guide mechanic — bright against dark environments.

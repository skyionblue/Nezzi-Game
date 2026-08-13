# Puzzle Design: World 1 — Puzzle 11 — "The Low Road"

**Teaches:** Crawl tunnel — a passage only Dani can fit through
**Emotion:** Trust. Scarlet waits and watches the tunnel entrance go dark.
**Difficulty:** Gentle — clean mechanic introduction

## ASCII Layout

```
█████████████████████████████████████
█   ▓    ▓    ★    ▓    ▓          █
█   ▓    ▓    ^    ▓    ▓          █
█   ▓    ▓    ▓    ▓    ▓          █
█   █    █   [ ]   █   ...         █
█   ▓    ▓    ▓    ▓    ▓          █
█   ▓    S    ▓    D    ▓          █
█   ▓    ▓    ▓    ▓    ▓          █
█████████████████████████████████████
```

A wall runs the full width of the room with two openings: a locked gate `[ ]`
in the centre and a crawl tunnel `...` at the east end. The lever `^` sits
north of centre. Reunion `★` is at the far north.

## Step-by-Step Solution

1. Both chars explore the south zone and reach the wall. Scarlet cannot pass —
   she's too tall for the tunnel and the gate is locked.
2. Switch to Dani. Walk her east to the tunnel entrance at the wall's right end.
3. Dani crawls through the tunnel (crawl animation plays).
4. Dani exits into the north zone. Walk north to the lever. Press USE.
5. The gate swings open. Switch to Scarlet; walk her through the centre gate.
6. Both walk north to the reunion trigger. Level complete.

## Why Neither Can Solo

**Scarlet alone:** Cannot fit through the tunnel. Gate is locked with no lever
in the south zone. She has no path north.

**Dani alone:** Can crawl through, pull the lever, and reach the reunion zone —
but Scarlet is still stuck south of the wall. The ReunionTrigger requires both
characters inside simultaneously. It never fires.

## Design Notes

- The tunnel entrance is at the east end of the wall, visible from both start
  positions, clearly reading as "a gap Dani can use."
- CrawlTunnelEntrance prefab placed as a manual scene object at (6, 0, 0).
  A CrawlTrigger child (BoxCollider trigger) drives the crawl animation.
- Scarlet has no active role beyond waiting and walking through. That stillness
  IS her beat — trust.
- Gate yRotation=0, gateOpenOffset=(0, 3.7, 0).

## Checkpoint

- (0, 0, −9) — south zone, before the wall.

## Coins

| # | Position | Note |
|---|---|---|
| 1 | (−3, 0, −3) | West side of south zone — rewards exploring the wall |
| 2 | (3, 0, −3) | East side — leads the eye toward the tunnel |
| 3 | (3, 0, 6) | North zone beside the lever — reward for crawling through |

## Hints

1. "Something on the far side of the wall controls the gate. Only one of you is small enough to find out what."
2. "Dani can fit through the low tunnel on the right side of the wall. Scarlet has to wait for the gate to open."
3. "Switch to Dani and crawl through the tunnel. Pull the lever on the far side to open the gate for Scarlet. Then both walk to the glowing circle."

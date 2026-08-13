# Sprint V2-01 — Eleven City Puzzles (Design Spec)

**Goal:** Define all 11 puzzles for the Clay City world — their zones, mechanics, solve sequences, camera anchors, and acceptance criteria — so the unity-senior-developer agent has a complete, build-ready spec for every puzzle zone.
**Theme:** City puzzle world design (V2 — Cartoon City Environment Pack)
**Status:** Planned

---

## Context

V2 replaces the forest world with the **Clay City** — a cartoon city where Scarlet and Dani start lost downtown and work their way out to the suburbs toward home. The demo scene (`Assets/Scenes/CityWorld.unity`) is the single persistent scene. All 11 puzzle zones live inside it, separated by real city geography. Between puzzles, a cinematic camera pan carries the player from one zone to the next.

The V1 mechanic set is fully implemented. V2 remaps each mechanic to a city-appropriate prop. No new core systems are required for the first sprint — this is a design-to-build spec, not a systems sprint.

---

## V2 Mechanic Prop Map (standing reference)

| V1 Mechanic | V2 City Prop | Implementation note |
|---|---|---|
| Boulder push | Dumpster / trash bin push | Scarlet pushes `SM_Bin` Rigidbody prefabs via `OnControllerColliderHit` — identical to boulder push |
| Pressure plate (stand-on hold) | Crosswalk button pad | Same `PressurePlate` component; visual is a painted crosswalk pad flush with the road surface |
| Lever (Dani only) | Wall buzzer / button panel | Same `Lever` + `IInteractable` — Dani reaches it, Scarlet cannot (panel mounted low on a building wall) |
| Gate / door | Wrought-iron gate | Keep existing gate prefab — fits city fencing |
| Narrow passage | Gap in fence / turnstile slot | Geometry-only width restriction (same technique as V1 Puzzle 4); no new prefab needed |
| Bridge (drops from above) | Construction ramp / scaffolding | Same `Bridge` component; visual is an `SM_Bars` scaffold plank that drops into place |
| Scarlet lifts Dani | Scarlet boosts Dani over fence | Same `TryLiftDani` / `BeginLiftedState` |
| Cross-wired lever/gate | Traffic light control box | Same logic — two `Lever` + `Gate` pairs, IDs cross-wired |
| Hold-and-cross plate | Crosswalk hold pad | Scarlet stands on pad, Dani crosses — same `PressurePlate` (hold, not one-shot) |
| Decoy / multi-step | Dual crosswalk pads | Same decoy-plate technique from V1 Puzzle 9 |

**New V2 mechanic — manhole crawl:** Dani drops into a manhole and resurfaces elsewhere. Replaces the forest crawl tunnel (`CrawlTrigger`). Uses the same `CrawlTrigger` component with two trigger volumes (entry manhole → exit manhole). This is the one mechanic that needs a new prefab and `LevelBuilder` case if `CrawlTrigger` is not already in the data-driven pipeline — see Risks.

---

## Design Rules (unchanged from V1)

1. Neither character solves it alone.
2. One enables the other — every puzzle has an interlocking moment.
3. No text instructions — obstacles must be visually obvious.
4. One new mechanic per puzzle (early); combinations later.
5. Win condition: both characters reunite at the designated `ReunionTrigger`.

---

## Camera Transition System

Between puzzles, a Cinemachine **dolly-track or free-look pan** carries from the current zone's **Camera Exit Point** to the next zone's **Camera Entry Point**. The transition plays after `OnReunionAchieved` fires and before `LevelSequenceController` (or its V2 equivalent) loads or reveals the next zone.

Each puzzle spec below defines:
- **Camera Entry:** where the camera settles when the zone becomes active (angle + target position)
- **Camera Exit:** where the camera starts its fly-out after reunion fires

For V2, "loading the next scene" is replaced by "panning within the single `CityWorld` scene." The `LevelSequenceController` will need to be adapted (or replaced) to support within-scene zone activation rather than `SceneManager.LoadScene`. That is a systems task for a future sprint — the specs below define the spatial anchor points the camera system will use.

---

## Puzzle Specifications

---

### Puzzle 1 — "The Intersection" (Downtown / Easiest)

**Zone:** Skyscraper-district street corner. Open plaza between two tall buildings. Fenced by low decorative iron railings.
**City location:** Northwest corner of the downtown skyscraper block. Widest, most readable space in the city.
**Mechanic introduced:** Basic movement. Characters travel together. Reunion as win condition.
**Emotion:** Warm, disorienting — the city is huge and unfamiliar. Getting your bearings.

**Solve sequence:**
1. Scarlet and Dani start side by side on a wide sidewalk.
2. A low iron gate across the street is the only obstacle between them and the reunion marker on the far curb.
3. The gate has a wall buzzer panel beside it at Dani's height.
4. Dani presses the buzzer. The gate swings open.
5. Both walk through and step onto the glowing reunion marker together.

**Neither alone proof:** The gate is closed and Dani's buzzer is required to open it. Scarlet cannot press the buzzer (mounted low — only Dani reaches it). Dani alone cannot complete the puzzle because the `ReunionTrigger` requires both characters simultaneously.

**Enable moment:** Dani opens the gate so both can pass. (The "Dani enables Scarlet" half. The return is: Scarlet's presence is required for reunion — without Scarlet the gate opening accomplishes nothing.)

**Design note:** This is the handshake puzzle. One obstacle, one mechanic, minimal cognitive load. The city scale (tall skyscrapers, wide roads) should feel enormous and slightly intimidating — lean into that with camera FOV and building density. The reunion marker glows warmly against the grey city asphalt.

**Camera Entry:** High isometric above the two characters on the starting sidewalk. Tilt down slightly — buildings visible on both sides to sell the city scale.
**Camera Exit:** Slow pull-back from the reunion marker looking up the street toward the mid-city area. The camera begins drifting toward Puzzle 2's zone.

---

### Puzzle 2 — "The Parking Lot" (Downtown)

**Zone:** Open parking lot / plaza adjacent to the skyscraper block. Fenced perimeter with a single narrow gap in the fence that only Dani fits through.
**City location:** East side of the skyscraper block, behind the main building.
**Mechanic introduced:** Separation. Dani's size is a strength (gap in fence).
**Emotion:** First moment of worry — Scarlet is left behind. Then relief.

**Solve sequence:**
1. Scarlet and Dani enter the parking lot together. A wrought-iron fence runs across the middle of the lot.
2. The fence has a single narrow gap (a missing fence panel, or a turnstile slot) that Dani can squeeze through but Scarlet cannot.
3. On Scarlet's side: a crosswalk button pad is visible but the pedestrian gate it controls is on Dani's side.
4. Dani slips through the gap to the far side.
5. On Dani's side, a wall buzzer panel opens the large vehicle gate on Scarlet's side.
6. Dani presses the buzzer. The vehicle gate swings open for Scarlet.
7. Scarlet walks through. Both reach the reunion marker on the far side.

**Neither alone proof:** Scarlet cannot fit through the gap. Dani cannot open the vehicle gate from Scarlet's side (buzzer is on Dani's side). Without Scarlet walking through the vehicle gate, reunion is impossible.

**Enable moment:** Dani fits through the gap and opens the gate for Scarlet from the other side.

**Design note:** The gap should read instantly — a missing fence panel with a clear "Dani-sized" opening, not a hidden passage. Scarlet's character controller collides with the fence geometry; Dani's narrower capsule passes through. No special mechanic needed beyond the existing CharacterController capsule radii difference.

**Camera Entry:** Low isometric looking across the parking lot — fence line visible in the foreground. Both characters enter from the left.
**Camera Exit:** Pan right along the fence line, continuing into the alley between buildings.

---

### Puzzle 3 — "The Alley" (Downtown / Mid-transition)

**Zone:** Narrow alley between two buildings. Linear east-west channel, barely wide enough for Scarlet. A large dumpster blocks the center.
**City location:** Alley running between the skyscraper block and the first mid-city residential building.
**Mechanic introduced:** Scarlet pushes the dumpster (city equivalent of boulder push).
**Emotion:** Satisfying. "Oh — she does that."

**Solve sequence:**
1. Scarlet and Dani enter the alley from the west. A heavy dumpster sits in the middle, blocking both characters.
2. To the south is a recessed loading dock — a pressure-plate crosswalk pad is flush with the dock floor.
3. Scarlet pushes the dumpster south. It rolls into the loading dock and lands on the crosswalk pad.
4. The pad activates. A rolling security door (gate) at the east end of the alley slides open.
5. Both characters walk through the now-open door and reach the reunion marker beyond.

**Neither alone proof:** Dani cannot push the dumpster (only `ScarletController.OnControllerColliderHit` applies force). Scarlet alone cannot trigger reunion. The dumpster must land precisely on the pad — requires understanding the alley layout.

**Enable moment:** Scarlet's strength clears the path for both, but the dumpster lands on the plate — revealing that the push was the key, not just the obstacle removal.

**Design note:** The dumpster should be visually distinct (large, green, city-standard). The loading dock should be visible from the start — the player sees the pad before they see why it matters. The "aha" is recognizing where to push the dumpster.

**Camera Entry:** Street-level isometric looking east down the alley. Both characters visible in the left third. Dumpster blocking the center.
**Camera Exit:** Pull back to reveal the open door, then crane up and drift north toward the mid-city residential area.

---

### Puzzle 4 — "The Yard" (Mid-City Residential)

**Zone:** A fenced house yard — one of the oval fence-ring islands with a house at the center. The first residential puzzle.
**City location:** First house on the mid-city residential loop, south side.
**Mechanic introduced:** Manhole crawl — Dani drops into a manhole on one side and resurfaces on the other (city equivalent of the forest crawl tunnel).
**Emotion:** Clever. Dani leads the way.

**Solve sequence:**
1. Scarlet and Dani approach the yard from the street. A tall section of fence (too tall for either to climb) blocks the yard entry gate from being opened from outside.
2. A manhole on the sidewalk outside the fence is open and glowing with Dani's color indicator.
3. Dani enters the manhole. She travels underground and resurfaces inside the yard via a second manhole.
4. Inside the yard, a wall buzzer panel is on the house wall beside the yard gate.
5. Dani presses the buzzer. The yard gate swings open for Scarlet.
6. Scarlet enters. Both reach the reunion marker near the house's front door.

**Neither alone proof:** The fence is too tall for Dani to climb unaided (and there is no gate she can open from the outside). Scarlet cannot fit in the manhole. Without Dani opening the gate from inside, Scarlet cannot enter. Reunion requires both.

**Enable moment:** Dani uses the manhole to bypass the fence and open the gate for Scarlet from the inside.

**New system note:** The manhole `CrawlTrigger` (entry + exit pair) must be added to the `LevelBuilder` pipeline if not already there. `CrawlTrigger.cs` exists in the codebase — check whether it has a `LevelObjectType` and `LevelPrefabRegistry` entry. If not, this is the one code task in an otherwise data-authoring sprint. See Risks.

**Camera Entry:** Elevated isometric looking down at the yard island from the street. Fence ring visible, house in center.
**Camera Exit:** Pull up and drift north-east toward the basketball court area.

---

### Puzzle 5 — "The Court" (Mid-City / Basketball Court)

**Zone:** The fenced basketball/football court. Rectangular, fully enclosed. High fence on all sides. Chain-link look.
**City location:** Center of the mid-city block, near the park.
**Mechanic introduced:** Scarlet lifts Dani over the fence (city equivalent of the boost mechanic).
**Emotion:** Teamwork. They are stronger together than apart.

**Solve sequence:**
1. Scarlet and Dani approach the court. The chain-link fence is too tall for either to climb or vault.
2. The entry gate has an electronic lock panel mounted high — too high for either character to reach from outside.
3. A lift point marker (glowing floor indicator) is at the base of the fence.
4. Scarlet lifts Dani to the top of the fence. Dani drops inside the court.
5. Inside the court, a wall panel at Dani's height opens the entry gate.
6. Dani presses it. The gate opens for Scarlet.
7. Scarlet enters. A second crosswalk pad (held) is inside the court — Scarlet stands on it.
8. The standing pad keeps a construction ramp / scaffold extended across a gap on the court's far side.
9. While Scarlet holds the pad, Dani crosses the ramp.
10. Dani reaches a buzzer on the far side that latches the ramp permanently (one-shot).
11. Scarlet steps off the pad. Both cross to the reunion marker at the center of the court.

**Neither alone proof:** Scarlet cannot get Dani inside without the lift. Dani cannot open the entry gate without being inside. Scarlet cannot cross the gap without the ramp latched. Dani cannot latch the ramp without Scarlet holding the pad first. Neither can reach reunion without the other.

**Enable moment:** Scarlet enables Dani (lift), then Dani enables Scarlet (opens gate), then Scarlet enables Dani (holds pad), then Dani enables Scarlet (latches ramp). First multi-enable puzzle.

**Design note:** This is Puzzle 5 — the player is now comfortable with the mechanics. Stack two enables without explanation and let the player feel the escalation. The court context (sport, teamwork) mirrors the cooperative theme.

**Camera Entry:** Looking down into the court from the north. Fence visible on all sides.
**Camera Exit:** Crane up and drift west toward the park section.

---

### Puzzle 6 — "The Park" (Mid-City / Park)

**Zone:** Open park section bounded by a low fence and the road. Benches, trees, a central path.
**City location:** West side of the mid-city block, between two residential loops.
**Mechanic introduced:** Trust mechanic — cross-wired lever/gate pairs (city equivalent: traffic light control boxes on opposite sides of the park).
**Emotion:** Tension. Neither sibling can see what the other is doing.

**Solve sequence:**
1. Scarlet and Dani enter the park from the south. A low wall divides the park into two halves — Scarlet's side (west) and Dani's side (east). The wall has no gap wide enough to pass through.
2. Each side has a locked gate to the far north — Scarlet's gate leads to the north path, Dani's gate leads to the north path.
3. Each gate has a traffic-light control box (lever) — but each box is wired to the OTHER character's gate (cross-wired: Scarlet's box opens Dani's gate, Dani's box opens Scarlet's gate).
4. Scarlet presses her control box. A sound and visual flash on Dani's side signals that Dani's gate opened.
5. Dani walks through her now-open gate.
6. Dani presses her control box (on the far side of the wall). Scarlet's gate opens.
7. Scarlet walks through. Both reach the reunion marker at the park's north end.

**Neither alone proof:** Each gate requires the OTHER character's control box. Neither can open their own gate. Both gates must be opened to reach the reunion marker. Sound/visual feedback across the dividing wall makes the cross-wire relationship readable without text.

**Enable moment:** Each character opens the path for the other — pure mutual dependency. Neither leads; both enable simultaneously.

**Design note:** Sound design carries this puzzle. A traffic light "click" and visual flash on the opposite side of the wall tells the player their action did something without showing it. The brief moment of not being able to see the other character is the emotional beat the design doc calls the "trust mechanic."

**Camera Entry:** Split isometric looking south across the park. Dividing wall visible. One character on each side.
**Camera Exit:** Pan north, following both characters as they converge on the reunion marker.

---

### Puzzle 7 — "The Second Yard" (Mid-City Residential / Second Loop)

**Zone:** Second house yard — another oval fence-ring island, but with an elevated deck/porch on the house.
**City location:** Mid-city residential loop, north side. Second house encountered.
**Mechanic introduced:** First combination puzzle (dumpster push + manhole crawl).
**Emotion:** Growing confidence. "We know how to do this."

**Solve sequence:**
1. Scarlet and Dani arrive at the yard. The yard gate is blocked by a heavy planter / dumpster pushed against it — the gate physically cannot swing open with the object in the way.
2. A sidewalk manhole is outside the fence, slightly west.
3. Dani enters the manhole, resurfaces inside the yard.
4. Inside the yard, Dani cannot move the planter (too heavy). But a buzzer panel on the house porch is at Dani's height — it opens a small side-gate in the fence.
5. Dani presses the buzzer. The side-gate opens, but it is too narrow for Scarlet (geometry-only width restriction).
6. Dani exits through the side-gate and rejoins Scarlet outside the fence.
7. Scarlet pushes the main planter/dumpster off the main gate (pushes it east, away from the gate).
8. The main gate swings open. Both enter.
9. The reunion marker is at the house front door.

**Neither alone proof:** Dani must enter via manhole to trigger the side-gate buzzer. Scarlet cannot enter the yard through the manhole or the side-gate. Without Scarlet pushing the planter off the main gate, neither can enter through the main gate. Reunion requires both inside.

**Enable moment:** Dani goes in the back way to open the side exit so she can get back out, then Scarlet clears the main entrance. Each creates the other's path.

**Design note:** The combination of manhole-then-dumpster is the puzzle. Seeing the dumpster blocking the gate first tempts the player to push it immediately — but Scarlet can't enter the yard yet (no open gate). The "aha" is: Dani goes first via manhole to open the side gate, then exits, then Scarlet clears the main gate.

**Camera Entry:** Elevated isometric above the second yard, slightly north of the first yard camera angle.
**Camera Exit:** Drift north-east toward the residential street bridge / road overpass zone.

---

### Puzzle 8 — "The Overpass" (Mid-City / Road Overpass)

**Zone:** Road overpass zone — a raised road bridge crosses over a lower service road. The service road is below, the overpass above. Chain barriers on the overpass edge.
**City location:** North-east of the mid-city residential loop, where the overpass connects to the suburban outskirts road.
**Mechanic introduced:** Hold-and-cross (city version: Scarlet holds a crosswalk button pad so a construction scaffold ramp stays extended while Dani crosses).
**Emotion:** Cooperative precision. Timing feels real.

**Solve sequence:**
1. Scarlet is on the overpass (upper road, y ≈ 1.0 m). Dani is on the lower service road (y = 0). They start separated by the height difference — this is the first puzzle where they begin apart.
2. A scaffold ramp on Dani's side would connect lower road to the upper road, but it is retracted (raised) and only deploys when a crosswalk button pad on the lower road is held.
3. Scarlet cannot reach the lower road (no ramp or stairs available from her starting position).
4. Dani stands on the crosswalk button pad. The scaffold ramp deploys, bridging lower to upper.
5. Scarlet crosses down the ramp from the overpass to the lower road. (Ramp connects upper overpass to lower service road — Scarlet descends.)
6. Scarlet is now on the lower road. A buzzer panel on the overpass support pillar is at Scarlet's height — she presses it. This opens a gate at the far end of the lower road (the gate Dani needs to walk through to reach the overpass ramp on the far side).
7. Wait — Dani is still holding the pad. Dani steps off the pad when Scarlet reaches the lower road. The ramp can retract now (Scarlet is already down).
8. Dani walks through the now-open gate (opened by Scarlet's buzzer press) to the far side.
9. On the far side, a second scaffold ramp leads back up to the overpass level. Dani walks up.
10. Scarlet is now on the lower road walking east. Dani is now on the upper overpass road walking east. A reunion marker sits at the far east end of the overpass.
11. Both arrive at the reunion marker — Scarlet from below via a final ramp, Dani from the overpass deck.

**Neither alone proof:** Scarlet cannot get down from the overpass without Dani holding the pad. Dani cannot open the far gate without Scarlet pressing the buzzer. Neither can reach the reunion point without the other completing their role.

**Enable moment:** Dani holds the pad so Scarlet can descend. Scarlet presses the buzzer so Dani can advance. Sequential mutual enabling under spatial separation.

**Design note:** This is the first puzzle where they start separated. The camera entry angle should show both simultaneously — Scarlet above, Dani below — to immediately communicate the spatial relationship. The hold-and-release timing is the new challenge: the player must recognize that Dani can step off the pad once Scarlet is down.

**Camera Entry:** Wide isometric angle showing both road levels. Scarlet visible on upper road (left/top), Dani visible on lower road (right/bottom).
**Camera Exit:** Pull east and crane up to the suburban outskirts — wider, greener, less dense cityscape ahead.

---

### Puzzle 9 — "The Suburb Gate" (Suburban Outskirts)

**Zone:** Entry to the suburban outskirts — a wide road with a neighborhood entry gate (ornamental stone pillars, tall iron gate). First puzzle in the suburbs.
**City location:** Where the city road transitions to the suburban street. The gate marks the boundary.
**Mechanic introduced:** Multi-step / non-obvious solution (city version: two crosswalk pads, one is a decoy; correct sequence must be discovered).
**Emotion:** Stuck → thinking → breakthrough. First real "aha."

**Solve sequence:**
1. Scarlet and Dani face the tall neighborhood entry gate. It is flanked by stone pillars. Two crosswalk button pads are on the ground — one near the gate (obvious), one further back on the road.
2. A traffic-light control box is on top of one pillar — too high for either character to reach from the ground. Only reachable from the raised platform on the west side of the gate.
3. Wrong path (the decoy): Scarlet stands on the obvious near crosswalk pad. A pedestrian gate on the side opens — but it leads only to a fenced alcove with no exit. Dead end is immediately readable.
4. Correct sequence: Scarlet lifts Dani onto the raised platform beside the pillar. Dani reaches the traffic-light control box and presses it. This rewires the main gate's control circuit (visual: indicator light changes from red to green on the far crosswalk pad).
5. Scarlet steps onto the far crosswalk pad. The main gate opens.
6. Both walk through. Reunion marker is on the suburban street just beyond the gate.

**Neither alone proof:** Scarlet cannot reach the control box on the pillar (stepHeight blocks it). Dani cannot be lifted without Scarlet. After the lift, Dani cannot trigger reunion alone. After the control box, Scarlet must stand on the correct pad — Dani cannot trigger a pressure plate alone (she's still on the platform or descending). Reunion requires both through the gate.

**Enable moment:** Scarlet lifts Dani to the unreachable control box. Dani rewires the circuit. Scarlet activates the main gate by standing on the correct pad.

**Design note:** The decoy near-pad should be tried first — it is closer and more obvious. The dead-end alcove it opens must read as a dead end in one second of observation (low fence, no path out). The player backtracks, looks higher, spots the control box on the pillar. The lift is the key.

**Camera Entry:** Street-level isometric looking at the ornamental gate from the city side. Gate fills the frame. Both characters on the near side.
**Camera Exit:** Slow reverse pull through the now-open gate, looking back at the city skyline behind the characters as they walk forward into the suburbs.

---

### Puzzle 10 — "The Cul-de-Sac" (Suburban Outskirts)

**Zone:** A suburban cul-de-sac — circular road terminus with a house at the end and a large yard. Multiple fence sections, a parked car blocking a path, and a yard gate requiring coordinated access.
**City location:** Deep suburban loop, second-to-last house before the final zone.
**Mechanic introduced:** Full combination — lift + manhole + dumpster/car push + hold-and-cross. Hardest combination yet.
**Emotion:** Every mechanic clicks together. The city expertise feels earned.

**Solve sequence:**

**Act 1 — Getting in:**
1. Scarlet and Dani approach the cul-de-sac. A parked delivery van (large dumpster stand-in, pushable Rigidbody) blocks the side-gate to the yard. The main gate is electronically locked.
2. A manhole on the sidewalk glows — Dani's indicator.
3. Dani enters the manhole, resurfaces inside the yard near the house.
4. Inside, a buzzer panel on the house wall is at Dani's height. Dani presses it. This opens the side-gate for Scarlet.
5. Scarlet cannot enter through the now-open side-gate — the parked van is still blocking it.
6. Dani walks back to the side-gate from inside. A wall buzzer on the interior side of the fence opens the main gate.
7. Dani presses the interior buzzer. The main gate opens.
8. Scarlet enters through the main gate (van was blocking the side-gate, not the main gate).

**Act 2 — Reaching reunion:**
9. Inside the yard: a gap in the inner yard path separates the house's front area from the garage/back area where the reunion marker sits.
10. A scaffold ramp (held bridge) is available — it spans the gap when a crosswalk pad is held.
11. Dani stands on the crosswalk pad. The ramp extends.
12. Scarlet crosses the ramp to the back area.
13. A final van/dumpster sits in front of the reunion marker. Scarlet pushes it aside.
14. Dani steps off the pad (ramp can retract — Scarlet is already across).
15. Dani crosses the gap via a narrow ledge path (width-restriction passage, Dani only) on the other side of the yard.
16. Both reach the reunion marker at the garage.

**Neither alone proof:** Dani must enter via manhole. Scarlet cannot enter until two separate gates are opened (requires Dani). Scarlet must push the final van and cross the held ramp. Dani must hold the ramp. Dani must cross the narrow ledge independently. All five abilities are required.

**Enable moment:** Multiple — every act has one character enabling the other. Most complex enable chain so far.

**Camera Entry:** High isometric over the cul-de-sac. Circular road visible. House in the center.
**Camera Exit:** Low, slow pull south back down the suburban street, lingering on the characters — almost home. Then crane up to reveal the final zone ahead.

---

### Puzzle 11 — "Coming Home" (Deep Suburbs / Finale)

**Zone:** The last house. A quiet suburban home with a large yard, a garden, and a warm glow through the windows. The house they've been trying to reach.
**City location:** End of the suburban street. The final destination.
**Mechanic introduced:** Everything combined. Full mastery under the hardest spatial challenge.
**Emotion:** Triumph. Tears. Home.

**Solve sequence:**

**Act 1 — The Garden Wall:**
1. Scarlet and Dani arrive at the house's outer garden wall — a tall stone fence with a decorative iron gate. The gate is padlocked.
2. A manhole in the garden (just inside the wall) is the only way in without the key.
3. Dani enters the manhole from outside via a storm drain (entry point on the street side). She surfaces inside the garden.
4. Inside the garden, a buzzer panel at Dani's height is on the garden wall. Pressing it opens a side-gate in the wall — but the side-gate has a narrow gap only Dani could fit through (not useful for Scarlet).
5. On the interior wall, a control box is mounted high on the garden wall's stone pillar — too high for Dani to reach unaided.
6. A stackable garden crate sits in the garden. Dani carries the crate under the high control box and climbs it. She presses the control box. This opens the main padlocked gate.

**Act 2 — The Front Path:**
7. Scarlet enters through the now-open main gate.
8. A parked garden cart (dumpster stand-in) blocks the front path to the door. Scarlet pushes it aside.
9. The front path leads to the door. But the door has a crosswalk-style welcome pad — both characters must stand on it simultaneously to trigger reunion (the "welcome home" mat).

**Act 3 — Getting to the Door Together:**
10. The welcome mat is at the top of a short flight of steps (a raised stoop, ~1.0 m above the garden path). The steps are too steep for Scarlet (stepHeight blocked). The steps have a ramp on the side accessible from a scaffold plank that deploys when a pad in the garden is held.
11. Scarlet stands on the garden pad. The scaffold plank extends from the garden path up to the stoop landing.
12. Dani crosses the plank to the stoop.
13. Dani reaches a buzzer on the stoop that latches the plank permanently (one-shot).
14. Scarlet steps off the garden pad. The plank stays.
15. Scarlet crosses the plank to the stoop.
16. Both stand on the welcome mat together. Reunion fires.

**Neither alone proof:** Dani must use the manhole to unlock the main gate (Scarlet cannot). Scarlet must push the garden cart (Dani cannot). The stoop requires Scarlet to hold the pad so Dani can cross, and Dani to latch the plank so Scarlet can follow. The welcome mat requires BOTH simultaneously. No step is bypassable.

**Enable moment (finale):** Every mechanic from every previous puzzle appears once. The final "enable" — both standing on the welcome mat together — is also the reunion. The mechanic and the emotion are the same action.

**Design note:** The house should feel unmistakably like home: warm lit windows, familiar suburban details. The story beat lands here — the `PuzzleCompleteUI` delay gives a moment of stillness on the stoop. The camera holds on both characters facing the door, then fades. No text. No narration. The image says it.

**Camera Entry:** Ground-level isometric looking up at the house. Warm window glow visible. Both characters small against the house's front — emphasizes how far they've come.
**Camera Exit / Ending:** After reunion, slow crane up and back — revealing the full city behind them, the long distance they traveled. Then fade to the credits / main menu.

---

## Difficulty Curve — V2 City World

| Puzzle | Zone | Mechanic introduced | Difficulty |
|---|---|---|---|
| 1 | Downtown plaza | Movement / Dani opens gate | Tutorial |
| 2 | Parking lot | Separation / fence gap | Very easy |
| 3 | Downtown alley | Dumpster push | Easy |
| 4 | First house yard | Manhole crawl | Easy |
| 5 | Basketball court | Lift + hold-and-cross combo | Easy-Medium |
| 6 | Park | Trust / cross-wired levers | Medium |
| 7 | Second house yard | Dumpster + manhole combined | Medium |
| 8 | Road overpass | Hold-and-cross + spatial separation | Medium-Hard |
| 9 | Suburb gate | Multi-step / decoy pad | Hard |
| 10 | Cul-de-sac | Full combination (4 mechanics) | Hard |
| 11 | Home house | Everything + stacking; finale | Very Hard |

---

## Success Criteria (Sprint Definition of Done)

This sprint is a design spec sprint, not a build sprint. Done means:

- All 11 puzzle specs are written with: zone, mechanic, enable moment, numbered solve sequence, neither-alone proof, camera entry, camera exit.
- Each puzzle's "neither alone" proof is explicitly verified on paper — no puzzle is solvable by a single character.
- The V2 mechanic-to-prop map is complete and actionable (every V1 mechanic has a city equivalent the developer or agent can build from).
- The difficulty curve escalates correctly: puzzle 1 is learnable in one attempt; puzzle 11 is the hardest.
- The manhole/crawl system risk is flagged with a clear resolution path.
- The stacking mechanic (Puzzle 11) is scoped — it is the one genuinely new mechanic introduced in V2.
- The camera entry and exit anchors are defined for all 11 zones.
- `docs/sprints/SPRINTS.md` is updated.

---

## Backlog (for the `unity-senior-developer` agent)

The sprint spec is complete. The following implementation tasks flow from it and are ready to be handed to the `unity-senior-developer` agent in subsequent sprints:

| # | Task | Size | Acceptance criteria | Depends on |
|---|------|------|---------------------|------------|
| 1 | Audit `CrawlTrigger.cs` — confirm whether it has a `LevelObjectType` enum value and a `LevelPrefabRegistry` entry. If not, add `CrawlTrigger = 11` to the enum, a `crawlTriggerPrefab` field to `LevelPrefabRegistry`, a `SpawnCrawlTrigger` method to `LevelBuilder`, and a matching `case` in `PlaceObjects()`. | M | Placing a `CrawlTrigger` object in a `LevelData` asset causes `LevelBuilder` to instantiate the entry + exit trigger pair at runtime. | — |
| 2 | Audit the `CityWorld.unity` scene — identify usable zones for each of the 11 puzzle locations and confirm the city geometry matches the zone descriptions above. Note any zones that don't exist or need additional props (SM_Bin dumpsters, crosswalk pad visuals, manhole cover props). | M | A written zone-map confirming which city geometry in the scene corresponds to each of the 11 puzzle zones, with any missing prop list. | — |
| 3 | Design and author a `CityLevelSequenceController` (or adapt the existing `LevelSequenceController`) to support within-scene zone activation rather than `SceneManager.LoadScene`. V2 uses a single `CityWorld.unity` scene with zones that activate in sequence. | L | Playing the game from a city start point activates zones in sequence without scene loads. The existing `OnReunionAchieved` event still drives progression. |  Task 1 |
| 4 | Implement the cinematic camera pan system — a Cinemachine dolly track (or free-look sequence) that carries the camera from each zone's Camera Exit Point to the next zone's Camera Entry Point after reunion fires. | L | After reunion in any puzzle zone, the camera smoothly travels to the next zone's entry angle before characters become controllable in the new zone. | Task 3 |
| 5 | Build Puzzle 1 zone ("The Intersection") in `CityWorld.unity` using the spec above. Wire one `Lever`/`Gate` pair. Place `ReunionTrigger`. Confirm full solve from start to reunion in play mode. | M | Play mode: Dani presses the wall buzzer, gate opens, both characters walk through, reunion fires. | Tasks 1–2 |

Tasks 3–5 are the foundation for all subsequent puzzle builds. They are L-sized and should each be their own sprint or at minimum their own agent session.

---

## Out of Scope

- Building any individual puzzle zone in Unity (that is the `unity-senior-developer`'s task in subsequent sprints).
- The cinematic camera transition system implementation (flagged but deferred — spec anchors defined above).
- The `CityLevelSequenceController` within-scene zone system (flagged; deferred to implementation sprints).
- Audio / sound design (the trust mechanic in Puzzle 6 depends on sound — flag for a dedicated audio sprint).
- Stacking mechanic implementation (stacking is used only in Puzzle 11; `DaniController.TryPickUpObject` exists but is untested as a puzzle element — validate before designing Puzzle 11's final layout in Unity).
- Any world beyond the 11 city puzzles (World 3, etc.).
- Replacing the existing V1 scenes (V1 remains intact; V2 is additive).

---

## Risks & Assumptions

- **CrawlTrigger pipeline gap (HIGH):** `CrawlTrigger.cs` exists in the codebase but V1 Sprint 05 confirmed that `CrawlTunnelEntrance` has no `LevelObjectType`, no `LevelPrefabRegistry` entry, and no `LevelBuilder` case. If this is still true, Puzzles 4, 7, and 11 (all manhole-crawl puzzles) require a code task before data authoring can begin. This is the one non-trivial code addition in V2 — it is small (one enum value, one registry field, one spawner method, one `case`) but must be done first. Verify before committing to a build sprint timeline.

- **Single-scene zone architecture (HIGH):** V1 used one scene per puzzle. V2 uses one scene for all 11 puzzles. `LevelSequenceController` currently calls `SceneManager.LoadScene` — it needs to be adapted or replaced for within-scene zone progression. This is a medium-sized systems task that gates every puzzle build. Plan a dedicated systems sprint before puzzle implementation sprints.

- **Stacking mechanic in Puzzle 11 (MEDIUM):** `DaniController.TryPickUpObject` is coded but has never been used as a puzzle element in V1. Puzzle 11 depends on it for the garden crate step. Validate the stacking mechanic works end-to-end (pick up, carry, place, climb) before finalizing Puzzle 11's layout. If it does not work as expected, substitute a Scarlet-lifts-Dani beat for the high control box step.

- **Cartoon City pack prop availability (MEDIUM):** The spec references `SM_Bin` prefabs (dumpsters) and `SM_Bars` (scaffold planks). Confirm these prefab names exist in the Cartoon City Environment Pack v2.0 before designing prop-specific layouts. If the names differ, adjust the prop map. The mechanics are stable regardless of prop names.

- **City geometry zone fit (LOW):** The 11 puzzle zones are mapped to natural city spaces described in the scene. The CityWorld demo scene may not have all zones exactly as described — some may be too small, too open, or spatially adjacent in ways that make zone transitions awkward. The zone audit (Backlog Task 2) must happen before any puzzle is built.

- **Overpass height (LOW):** Puzzle 8 uses a raised overpass (y ≈ 1.0 m) that Scarlet cannot climb unaided. Confirm the city scene has a road overpass or elevated section, or plan to build one from available city geometry. The `stepHeight = 0.6` constraint means the raised section needs to be at least 0.7 m to block Scarlet — verify this matches what's in the scene.

- **Assumption — welcome mat as `ReunionTrigger`:** Puzzle 11's welcome mat is a `ReunionTrigger` with a wide enough `triggerSize` for both characters to stand on together. This is a data-authoring choice, not a code change. Confirmed safe.

---

## References

- Design docs: `README-Design-Feedback.md` (sibling relationship, coin system, failure state, "very difficult by the end"), `README-Puzzle-Design.md` (design rules, mechanic legend, difficulty curve), `README.md` (core pillars)
- V1 sprint docs: `docs/V1/sprints/sprint-05-puzzles-9-and-10.md` (CrawlTunnel pipeline gap confirmed, stacking mechanic status, decoy-plate technique, lift mechanic params)
- Core systems: `Assets/Scripts/Core/LevelBuilder.cs` (all spawner methods, `PlaceObjects` switch), `Assets/Scripts/Data/LevelObjectData.cs` (full `LevelObjectType` enum — values 0–10 used), `Assets/Scripts/Core/LevelSequenceController.cs` (scene-name array, `OnReunionAchieved` listener — needs within-scene adaptation for V2)
- Puzzle systems: `Assets/Scripts/Puzzle/Lever.cs`, `Gate.cs`, `Bridge.cs`, `PressurePlate.cs`, `Hazard.cs`, `ReunionTrigger.cs`, `CrawlTrigger.cs`, `CheckpointTrigger.cs`
- Character abilities: `ScarletController.cs` (`OnControllerColliderHit` boulder/dumpster push, `TryLiftDani`), `DaniController.cs` (`TryActivateSwitch`, `BeginLiftedState`, `TryPickUpObject` — stacking, `SetClimbingState` — stub, do not use)
- Camera: `Assets/Scripts/Camera/IsometricCameraFollow.cs` (current follow system — dolly/pan system is new work for V2)
- City scene: `Assets/Scenes/CityWorld.unity` — Clay City demo scene (Hayq Art, Cartoon City Environment Pack v2.0)
- Standing preference: vertical climb mechanic remains a stub — do not plan any puzzle beat requiring rope/vine ascent.

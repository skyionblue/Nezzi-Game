# Meshy Asset Prompts — One Way Together

> Organized by priority. Start with characters, then core mechanic props,
> then environment pieces. Don't build environment art until the core
> gameplay loop is validated.

------------------------------------------------------------------------

# Characters

**Use Image-to-3D for both characters, not text prompts.**

The turnaround reference sheets in `art/concept/` are exactly what
Meshy's Image-to-3D feature expects — front, side, and back views in
one image.

| Character | File to upload                              |
| --------- | ------------------------------------------- |
| Scarlet       | `art/concept/big-character-turnaround-169.png`    |
| Dani    | `art/concept/little-character-turnaround-169.png` |

**Image-to-3D settings to use:**

-   Style: Cartoon / Stylized
-   Texture: Baked (not PBR — keeps the storybook feel)
-   Topology: Quad (better for animation rigging)

If the result needs cleanup, use these text descriptions as correction
prompts:

**Scarlet correction prompt:**
```
Cartoon stylized female character, older teenage girl, strong sturdy
build, bright red hair in a low ponytail, blue denim button-up shirt,
brown bib overalls, brown leather work boots, warm skin tone, large
expressive eyes, game-ready 3D model, storybook illustration style
```

**Dani correction prompt:**
```
Cartoon stylized young child character, approximately 7 years old, natural
black curly hair with small yellow hair clips, bright yellow zip-up hoodie,
green shorts, orange knee-high socks, white sneakers with orange accents,
warm medium-dark skin tone, large expressive eyes, slightly oversized
hoodie sleeves, game-ready 3D model, storybook illustration style
```

------------------------------------------------------------------------

# Core Mechanic Props

These are the interactive objects needed before any puzzle can be built.
Create these before environment art.

------------------------------------------------------------------------

## 1. Push Boulder

> Scarlet's primary physical object — something she can push and roll.

```
Large round mossy boulder, ancient stone texture, covered in green moss
patches and small cracks, slightly irregular shape not perfectly round,
cartoon stylized game asset, warm earthy tones, grey stone with green
moss, sits on the ground with a flat bottom, 3D game prop, storybook style
```

------------------------------------------------------------------------

## 2. Stone Push Block

> A square-edged version for more precise puzzle mechanics.

```
Ancient stone cube block, worn rectangular stone block with chiseled
edges, faded carved markings on sides, moss growing in cracks, cartoon
stylized 3D game asset, grey stone with green and brown weathering,
looks heavy and old, fits in a grid-based puzzle environment, storybook
illustration style
```

------------------------------------------------------------------------

## 3. Crawl Tunnel Entrance

> The passage only Dani can fit through — Scarlet cannot enter.

```
Small stone archway tunnel entrance, ancient ruins style, low narrow
arch just big enough for a small child, rough-cut stone blocks forming
the arch, moss and vines hanging from the top edge, dark interior,
cartoon stylized 3D game asset, warm grey stone tones with green moss,
storybook style
```

------------------------------------------------------------------------

## 4. Stone Pressure Plate

> A floor switch — pressing it triggers something elsewhere in the puzzle.

```
Ancient stone pressure plate floor switch, large flat circular stone
slab set into the ground, worn carved rune markings around the edge,
slightly depressed in the center, glows faintly amber when active,
cartoon stylized 3D game prop, grey stone with gold amber glow accent,
storybook illustration style
```

------------------------------------------------------------------------

## 5. Ancient Stone Lever

> A wall-mounted lever — pulling it triggers a mechanism elsewhere.

```
Ancient stone wall lever, mounted on a stone wall block, large wooden
handle with iron fittings, can be in two positions up or down, worn and
weathered, faint carved stonework around the mount, cartoon stylized 3D
game prop, grey stone and dark wood with iron metal accents, storybook
style
```

------------------------------------------------------------------------

## 6. Hanging Rope / Vine

> Dani can grab and climb these. Scarlet is too heavy.

```
Thick hanging jungle vine, long rope-like vine hanging vertically from
above, wrapped with smaller leaf tendrils, slightly swaying curve,
cartoon stylized 3D game asset, dark green vine with small bright green
leaves, natural organic texture, storybook illustration style
```

------------------------------------------------------------------------

## 7. Wooden Rope Bridge Section

> A single repeating section for building rope bridges — can be broken
> or intact.

```
Wooden plank rope bridge section, two thick rope rails on either side,
three worn wooden planks as footing, slight sag in the middle, frayed
rope details, cartoon stylized 3D game asset, warm brown wood with tan
rope, slightly weathered and aged, storybook illustration style
```

------------------------------------------------------------------------

## 8. Ancient Stone Door

> A massive door only Scarlet can force open.

```
Massive ancient stone double door, tall imposing stone gate, large iron
ring handles, carved geometric patterns across the surface, cracks
running through the stone, a faint glowing seam down the center when
locked, cartoon stylized 3D game prop, grey stone with dark iron handles
and amber glow accent, storybook illustration style
```

------------------------------------------------------------------------

## 9. Ancient Elevator Platform

> A stone platform that rises or lowers when a mechanism is activated.

```
Ancient stone elevator platform, square flat stone slab, carved
geometric patterns on the surface and sides, four iron chain attachments
at the corners going upward, slight worn groove marks from repeated
movement, cartoon stylized 3D game prop, grey stone with dark iron
chains, storybook style
```

------------------------------------------------------------------------

# Environment — World 1 (Forest + Ruins)

Build these after core mechanics are validated. These are the pieces
that form the visual world.

------------------------------------------------------------------------

## 10. Forest Platform Tile (Grass Top)

> The standard standing surface. Needs to tile horizontally.

```
Cartoon forest platform tile, flat topped grassy surface, lush green
grass on top with small wildflowers, dark rich soil sides with small
rocks visible, rounded soft corners, cartoon stylized 3D game asset
tileable platform, warm earthy tones with bright green top, storybook
illustration style
```

------------------------------------------------------------------------

## 11. Ancient Stone Platform Tile

> Ruins-style surface — used deeper into levels or in ancient areas.

```
Ancient stone platform tile, flat rectangular stone slab, worn and
chipped edges, carved geometric pattern across the surface, small cracks
with moss growing in them, cartoon stylized tileable 3D game asset, warm
grey stone with green moss, storybook style
```

------------------------------------------------------------------------

## 12. Large Background Tree

> A tall forest tree for background depth — non-interactive.

```
Large cartoon forest tree, very tall thick trunk with rough bark texture,
wide rounded canopy of bright green leaves, some small branches visible,
roots partially visible at the base, warm afternoon light feel, cartoon
stylized 3D background prop, rich dark brown trunk with bright and
mid-green canopy, storybook illustration style
```

------------------------------------------------------------------------

## 13. Stone Ruins Arch

> A freestanding arch from an ancient structure — decorative and
> atmospheric.

```
Ancient stone ruins archway, freestanding crumbled stone arch, missing
some blocks from the sides, vines and moss growing across the surface,
slight lean suggesting age, cartoon stylized 3D environment prop, warm
grey stone with green vines, storybook style
```

------------------------------------------------------------------------

## 14. Forest Lantern Post

> A glowing light source — guides the player visually through dark
> sections.

```
Wooden lantern post, tall wooden post with a hanging iron cage lantern at
the top, warm amber glowing light inside the cage, slight weathering on
the post, cartoon stylized 3D environment prop, dark wood post with iron
lantern and warm amber glow, storybook illustration style
```

------------------------------------------------------------------------

## 15. Waterfall Section

> Background water feature — atmospheric, non-interactive.

```
Cartoon waterfall section, wide sheet of falling water from a ledge
above into a shallow pool below, foam and mist at the base, wet rocks
visible through the water, cartoon stylized 3D environment prop, bright
cool blue and white water with grey wet rocks, storybook illustration
style
```

------------------------------------------------------------------------

# Collectibles

------------------------------------------------------------------------

## 16. Coin

> The hint currency — scattered throughout each level.

```
Cartoon golden coin collectible, round flat coin, bright warm gold
color, simple embossed design on the face, slight shine effect, small
enough to be overlooked in tight corners, cartoon stylized 3D game
collectible, bright gold with warm highlight, storybook style
```

------------------------------------------------------------------------

# Order of Operations

1.  Characters (Image-to-3D from turnarounds)
2.  Push Boulder + Stone Push Block
3.  Crawl Tunnel Entrance
4.  Pressure Plate + Lever
5.  Stone Door
6.  Rope / Vine + Bridge Section
7.  Elevator Platform
8.  Coin
9.  Environment tiles and props (only after gameplay is tested)

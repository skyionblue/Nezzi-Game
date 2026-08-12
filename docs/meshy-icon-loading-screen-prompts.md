# Meshy Prompts — App Icon & Loading Screen
## One Way Together

> These are **Text to 3D** scene prompts. Generate each scene, then capture
> a clean render from a front-facing camera in Blender or Unity. The render
> becomes the final 2D art. Aim for a 1:1 crop for the icon and 9:19.5
> (portrait) for the loading screen.
>
> Match the character descriptions exactly — they must look identical to
> Scarlet and Dani as established in `README-Meshy-Asset-Prompts.md`.

---

## App Icon

**Target output:** 1024 × 1024 px square (iOS + Android)

The icon must read clearly at 58 × 58 px (smallest iOS home screen size).
One bold central image, warm and immediately recognisable. No text — the
title goes in the store listing, not the icon.

### Composition

Scarlet crouching down to Dani's eye level, both facing the viewer, looking
up together with hopeful expressions. Scarlet's arm is around Dani's
shoulder. The framing is tight — shoulders up. Warm late-afternoon light
from behind them creates a soft rim glow. A blurred forest and ancient stone
arch are visible in the background, suggesting the world without competing
with the faces.

### Prompt

```
Two cartoon stylized siblings standing close together, older teenage girl
with bright red hair in a low ponytail wearing a blue denim button-up shirt
and brown bib overalls, crouching to the eye level of a small child with
natural black curly hair and small yellow hair clips wearing a bright yellow
zip-up hoodie and green shorts, both looking up toward the viewer with warm
hopeful expressions, the older sibling has her arm around the younger one,
tight portrait framing from the shoulders up, soft blurred forest background
with a faint stone arch, warm golden hour rim lighting from behind, storybook
illustration style, cartoon 3D render, rich warm colors, heartwarming
emotional tone, mobile game icon composition
```

### Render settings (Blender / Unity)

- Camera: orthographic or very long focal length (100 mm+) to flatten depth
- Lighting: warm key light from behind (rim), cool fill from front-below
- Background: soft bokeh of greens and amber — not white, not transparent
- Export: PNG, 1024 × 1024, no transparency (app stores require solid bg)

---

## Loading / Splash Screen

**Target output:** 1242 × 2688 px portrait (iPhone 14 Pro Max native)
Scale down for other sizes. Keep all critical content within the safe zone
(middle 80% of height).

### Composition

Wide establishing shot: Scarlet and Dani standing together in the center of
a forest clearing, viewed from a low isometric angle. Scarlet holds a small
lantern that casts a warm cone of light around them both. Towering ancient
stone ruins frame the left and right. A winding forest path ahead of them
disappears into soft mist and warm light, suggesting the journey home. The
mood is hopeful but slightly uncertain — they are small against a big world.
Leave the top ~20% of the frame as darker, simpler sky for the title text
overlay.

### Prompt

```
Two cartoon stylized siblings standing together on a forest path, older
teenage girl with bright red hair in a low ponytail wearing a blue denim
shirt and brown bib overalls holding a small glowing lantern, small child
with natural black curly hair and yellow hair clips wearing a bright yellow
hoodie and green shorts standing close beside her, viewed from a low
slightly isometric perspective, ancient mossy stone ruins and towering trees
framing both sides, a winding path ahead disappears into warm golden mist
and soft light suggesting home in the distance, the two figures are small
against the grand environment conveying a sense of adventure and scale, warm
golden and green color palette, dramatic atmospheric depth, soft volumetric
light rays through the trees, storybook illustration style, cartoon 3D
scene render, heartwarming and slightly mysterious emotional tone, mobile
game splash screen composition, clear open sky at the top of the frame for
title text overlay
```

### Render settings (Blender / Unity)

- Camera: low angle (roughly eye-level with Dani), slight isometric tilt
- Lighting: warm volumetric key from ahead-left, cool ambient fill
- Post: slight bloom on the lantern and the distant mist glow
- Safe zone: keep Scarlet and Dani fully within the middle 60% of height
- Export: PNG, 1242 × 2688, no transparency

---

## Style reference — both assets

Pull these details from the established character designs when generating
or correcting:

| Character | Key visual tags |
|-----------|----------------|
| Scarlet | Bright red hair, low ponytail, blue denim shirt, brown bib overalls, brown work boots, warm skin tone, large expressive eyes |
| Dani | Natural black curly hair, small yellow hair clips, bright yellow zip-up hoodie, green shorts, orange knee-high socks, white sneakers with orange accents, warm medium-dark skin tone, large expressive eyes |

**Shared style tags to add to any correction prompt:**
```
cartoon stylized, storybook illustration style, warm earthy tones,
large expressive eyes, game-ready aesthetic, heartwarming emotional tone
```

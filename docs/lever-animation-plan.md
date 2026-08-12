# Plan — Animated, PBR-Textured Stone Shrine Lever

**Objective:** Replace the current `AncientStoneLever` with a version whose **handle physically throws up/down** on activation, built from the segmented shrine mesh (`models/Stone_Switch_Shrine_part-segmentation.fbx`) and textured with the **original Meshy PBR set** (BaseColor + Normal + Metallic + Roughness).

**Why:** The current lever only changes color on activation. The segmented mesh has the handle as a separate part, so it can rotate. The parts lost their UVs during segmentation, but they are the same geometry as the original textured Meshy lever, so UVs (and thus the texture) can be transferred.

## Phase A — Blender (game-ready animated mesh)
- [ ] Import the **raw Meshy original** (from `models/Ancient_Stone_Lever_fbx.zip`) as the UV/texture **donor** (has UVs matching the PBR maps).
- [ ] Import the **segmented parts** (6 meshes, no UVs) as targets.
- [ ] **Verify alignment** — donor and parts should share Meshy's coordinate space (overlap). If not, align by rotation/scale before transfer. **(Critical risk.)**
- [ ] **Decimate** each part hard (886k verts total → a few thousand). Mobile budget.
- [ ] **Transfer UVs** donor → each part (nearest-surface data transfer), then apply.
- [ ] **Re-pivot the handle** part (`model_part5`) origin to the hub so it rotates cleanly; keep it a separate object.
- [ ] Assign a material using BaseColor/Normal/Metallic/Roughness.
- [ ] Export FBX with the **handle as a separate child object** (base parts can be joined).

## Phase B — Unity import
- [ ] Import FBX + the 4 PBR textures into `Assets/Art/Props/StoneShrineLever/`.
- [ ] Build a URP Lit material (BaseColor→Base Map, Normal→Normal Map, Metallic+Roughness→Metallic/Smoothness). Mark normal map as normal type.
- [ ] Build prefab: base + **handle as a pivoted child**, interaction Collider (trigger), `Lever` component.

## Phase C — Code
- [ ] Extend `Lever.cs`: on `Interact`, tween the handle's local rotation between a **rest** and **thrown** angle (serialized axis + angles + speed). Keep `plateId` broadcast, `oneShot`, `Init()` API, and the existing color feedback (optional).

## Phase D — Replace & verify
- [ ] Repoint `LevelPrefabRegistry.leverPrefab` to the new prefab.
- [ ] Play-mode: Dani interacts → handle throws up/down smoothly → gate opens; stone is textured correctly.
- [ ] Retire the old `AncientStoneLever` prefab once verified.

## Acceptance
In play mode, interacting with the lever rotates the handle from rest to thrown (visible up/down motion), the mesh is PBR-textured stone, and the linked gate still opens via the `plateId` event.

## Risks
- **UV-transfer alignment** — donor/target must overlap in space; different FBX export orientations can misalign them.
- **Decimation vs. UVs** — decimate before UV transfer so the transfer targets final topology.
- **Pivot/axis tuning** — the exact throw axis/angles are tuned against the hub after rigging.

## Outcome — ✅ COMPLETE

- **Approach change (better than planned):** the donor/segmented meshes did *not* overlap (different proportions), so UV transfer was risky. Instead the handle was **split off the already-textured donor mesh** — no UV transfer needed, texture stays exact. The user's segmented FBX ended up unused (it had no UVs).
- Handle split at the hub (`Y < 0.07`), origin set to the hub pivot; decimated to ~7k tris total (base 4.7k + handle 2.4k); exported with the handle as a separate pivoted object.
- Full PBR material built (BaseColor 2048 + Normal 2048 + packed Metallic/Smoothness 1024). Textures downsized from 4K; raw metallic/roughness packed into one metallic-smoothness map.
- `Lever.cs` swings the handle between rest and a thrown pose (`_thrownEuler`, default (-60,0,0)) via `RotateTowards`; keeps `plateId`/`oneShot`/`Init()` API.
- `Assets/Prefabs/Puzzle/Lever.prefab` visual replaced in place (registry reference preserved), handle wired, **visual rotated 180°** so it faces the play area (root rotation is overwritten by LevelBuilder's yRotation, so the turn lives on the visual child).
- Verified in play mode: textured, correctly oriented, handle animates on `Interact`.
- **Cleanup pending:** the old `AncientStoneLever` prefab/FBX/material/texture are now orphaned and can be deleted.

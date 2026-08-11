---
name: character-importer
description: Full character import pipeline. Processes Meshy 3D models through Blender (mesh validation, orientation fix, rig check, weight painting) then imports to Unity with a configured Animator Controller. Invoke with the character name and source file path.
---

# Character Importer Skill

Executes the complete character pipeline: raw Meshy model → Blender processing → weight-painted, validated character → Unity with Humanoid rig and Animator Controller.

## Usage

```
/character-importer <CharacterName> <sourcePath>
```

Examples:
- `/character-importer Scarlet art/raw/Scarlet.zip`
- `/character-importer Dani art/raw/Dani.zip`

---

## Reference Images (Required)

Before processing begins, the user must provide reference images.

**Minimum:** 1 front image
**Recommended:** Front, side, back

**Storage:**
```
art/characters/<CharacterName>/
  reference/
    front.png       ← REQUIRED
    side.png
    back.png
```

**At skill startup:**
1. Check if `art/characters/<CharacterName>/reference/` exists
2. If missing: prompt — "Provide at minimum a front reference image."
3. If present: display to user — "Using these references. Correct?"
4. If user has no references: offer to generate from the raw import as baseline

**Reference images are used at:**
- Step 3 (Orientation gate): compare Blender front view against reference/front.png
- Step 6 (Scale gate): compare height against reference
- Step 10 (Unity gate): compare final scene result against reference

---

## Pipeline Overview

| Step | Who | What |
|---|---|---|
| 0 | **Claude asks** | Reference images |
| 1 | Claude | Extract source, import to Blender |
| 2 | Claude | Mesh validation & auto-fix |
| 3 | Claude + **User** | Orientation & pivot gate |
| 4 | Claude | Rig check — armature present? |
| 4a *(no rig)* | Claude + **User** | Create Humanoid armature in Blender |
| 4b *(existing rig)* | Claude | Auto-fix weight painting |
| 5 *(existing rig)* | Claude + **User** | Interactive weight paint review |
| 6 | Claude | FBX export |
| 7 | Claude | Import to Unity as Humanoid |
| 8 | Claude | Create URP material with textures |
| 9 | Claude | Create Animator Controller |
| 10 | Claude + **User** | Place in scene, verify in Play mode |

> **Before Step 1**, ask: "Where are your reference images?"

> **Rig strategy (Step 4 decision):**
> - FBX has armature → fix weights (Steps 4b–5), export, Unity Humanoid
> - FBX is mesh-only → build Blender rig (Step 4a), export, Unity Humanoid
> - Mixamo is last resort only — if Blender rigging fails on extreme proportions

---

## Step 1: Extract & Import to Blender

**Extract:**
```bash
mkdir -p art/characters/<CharacterName>
unzip -o "<sourcePath>" -d "art/characters/<CharacterName>"
```

**Identify files:**
- `*_Character_output.fbx` or `*.fbx` — base mesh + rig
- `*_texture_0.png` — base color
- `*_metallic.png` — metallic map
- `*_roughness.png` — roughness map

**Clear Blender before import:**
```python
for obj in list(bpy.data.objects):
    if obj.type not in ('CAMERA', 'LIGHT'):
        bpy.data.objects.remove(obj, do_unlink=True)
for img in list(bpy.data.images):
    if img.name not in ('Render Result', 'Viewer Node'):
        bpy.data.images.remove(img)
for mesh in list(bpy.data.meshes):
    if mesh.users == 0:
        bpy.data.meshes.remove(mesh)
for mat in list(bpy.data.materials):
    if mat.users == 0:
        bpy.data.materials.remove(mat)
```

**Import:**
```python
bpy.ops.import_scene.fbx(filepath='<path_to_fbx>')
```

---

## Step 2: Mesh Validation & Auto-Fix

**Create checkpoint:**
```
create_checkpoint(name="{CharacterName}_pre_validation")
```

**Inspect:**
- Triangle count
- UV maps present
- Vertex count, material slots, bone count

**Run validation:**
```
validate_mesh(object_name)
```

**Auto-fixes (in order):**
1. `merge_vertices(distance=0.0001)`
2. `remove_doubles(threshold=0.0001)`
3. `recalculate_normals`
4. `fill_holes`

Re-run validation after fixes to confirm clean.

**Triangle budget gate:**
- Standard character budget: 20,000 tris
- If over: prompt — "{count} tris (budget: 20k). [Proceed] [Auto-decimate]"

**UV gate:**
- If no UVs: `smart_uv_unwrap(island_margin=0.02)`, screenshot, confirm
- If UVs exist: verify no overlapping islands

---

## Step 3: Orientation & Pivot Gate

- Meshy FBX typically imports with X=90° rotation (Z-up → Y-up). Normal.
- Model should face -Y in Blender (becomes +Z in Unity after export)
- Take screenshot from front view
- Compare against reference/front.png
- Prompt: "Does orientation match reference? [Yes] [Rotate needed]"
- Check base is at Z=0

---

## Step 4: Rig Check

```python
has_rig = any(o.type == 'ARMATURE' for o in bpy.data.objects)
```

- **`has_rig = True`** → Step 4b
- **`has_rig = False`** → Step 4a

---

### Step 4a: Create Humanoid Rig (mesh-only)

Build a 22-bone armature using standard Mixamo bone names (maps cleanly to Unity Humanoid):

**Required bones:**
`Hips, Spine, Spine1, Spine2, Neck, Head`
`LeftShoulder, LeftArm, LeftForeArm, LeftHand`
`RightShoulder, RightArm, RightForeArm, RightHand`
`LeftUpLeg, LeftLeg, LeftFoot, LeftToeBase`
`RightUpLeg, RightLeg, RightFoot, RightToeBase`

**Process:**
1. Estimate bone positions from mesh bounding box
2. Create armature programmatically
3. Pause — show user bone overlay, ask them to position bones in Edit Mode
4. User adjusts, signals done
5. Apply auto weights: select mesh + armature, `parent_set(type='ARMATURE_AUTO')`
6. Run weight cleanup pass (Step 4b fixes)

**Bone position estimates for ~1.7m character:**
```python
# Z: 0=ground, 0.65=hips, 0.85=spine, 1.08=shoulder, 1.25=neck, 1.57=head top
# Leg X offset: ±0.11, Arm T-pose X: shoulder ±0.18, wrist ±0.60
```

---

### Step 4b: Auto-Fix Weight Painting (existing rig)

```
create_checkpoint(name="{CharacterName}_pre_weight_fix")
begin_transaction(name="weight_fixes", auto_checkpoint=True)
```

**Fix 1: Remove feather bleed (< 0.05 weight)**
```python
for bone_name in all_bones:
    for each vert with bone weight < 0.05:
        remove vert from bone's vertex group
```

**Fix 2: Rigidify dominant verts (> 0.85 → 0.95)**
```python
for verts with weight > 0.85 on a bone:
    set weight to 0.95
```

**Fix 3: Cross-lateral contamination removal**
```python
# Left-side verts should not have significant right-bone weights, and vice versa
for each vert:
    if clearly on left side and has right bone weight < 0.3: remove
    if clearly on right side and has left bone weight < 0.3: remove
```

**Fix 4: Shoulder cleanup**
```python
# Shoulders should not influence above neck cutoff
for verts above shoulder_cutoff:
    remove LeftShoulder/RightShoulder weight
```

**Fix 5: Arm/torso hard separation (prevents armpit stretching)**
```python
# Below armpit Z: remove arm influence
# On arm geometry (X > threshold): remove spine/hips influence
```

**Fix 6: Normalize all weights**
```python
for each vert:
    total = sum of all group weights
    if total != 1.0: scale proportionally
    if total == 0: assign Hips as fallback
```

After all fixes: `take_screenshot` + `commit`. Show result before proceeding.

---

## Step 5: Interactive Weight Paint Review

Per bone:
1. Enter Weight Paint mode
2. Set active bone and vertex group
3. `take_screenshot()`
4. Report stats
5. Prompt: "Review {bone_name}. [Looks good] [Pause for manual fix] [Auto-fix] [Skip]"

**Bone order:** Head → Neck → Spine chain → Shoulders → Arms → Hands → Hips → Legs → Feet

**If "Pause for manual fix":** Skill pauses, user paints in Blender, user signals done, skill continues.

---

## Step 6: FBX Export

```python
bpy.ops.object.mode_set(mode='OBJECT')

bpy.ops.export_scene.fbx(
    filepath=out_path,
    use_selection=True,
    axis_forward='-Z',
    axis_up='Y',
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_ALL',
    bake_space_transform=True,
    mesh_smooth_type='FACE',
    use_mesh_modifiers=True,
    add_leaf_bones=False,
    path_mode='COPY',
    embed_textures=False,
)
```

Also save .blend: `art/characters/<CharacterName>/<CharacterName>_rigged.blend`

**Output:** `art/characters/<CharacterName>/<CharacterName>_processed.fbx`

---

## Step 7: Unity Import

**Copy to Unity:**
```
Assets/Art/Characters/<CharacterName>/
  <CharacterName>.fbx
  Textures/
    <CharacterName>_BaseColor.png
    <CharacterName>_Metallic.png
    <CharacterName>_Roughness.png
```

**Configure ModelImporter:**
```csharp
importer.animationType = ModelImporterAnimationType.Human;
importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
importer.globalScale = 1f;
importer.useFileScale = true;
importer.importAnimation = false;
importer.materialImportMode = ModelImporterMaterialImportMode.None;
importer.SaveAndReimport();
```

**Verify:** Avatar must be `isHuman=True, isValid=True`

---

## Step 8: Create Material

**URP Lit material:**
```csharp
var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
mat.SetTexture("_BaseMap", baseColorTexture);
if (metallicTexture) mat.SetTexture("_MetallicGlossMap", metallicTexture);
mat.SetFloat("_Smoothness", 0.3f);
mat.SetFloat("_Metallic", 0.0f);
```

**Save to:** `Assets/Art/Characters/<CharacterName>/MAT_<CharacterName>.mat`

---

## Step 9: Animator Controller

Create a minimal Animator Controller with the states the character needs.

**Parameters:**
| Name | Type | Purpose |
|---|---|---|
| Speed | Float | Locomotion blend (0=idle, 1=run) |
| Grounded | Bool | Ground state |
| Jump | Trigger | Jump |

**States:**
| State | Loop | Transition |
|---|---|---|
| Idle | Yes | Default; Speed < 0.1 |
| Walk/Run | Yes | Speed > 0.1 |
| Jump | No | Jump trigger |
| Land | No | On grounded |

Wire in animation clips when available. For now, create the controller structure with empty states — the Animator will not throw errors and can be populated with clips later.

**Save to:** `Assets/Animation/<CharacterName>/<CharacterName>Animator.controller`

Assign to the Animator component on the character prefab.

---

## Step 10: Scene Placement & Verification

- Place character in the active scene
- Set `CharacterModel` localRotation = (0, 180, 0) — **model faces -Z in FBX, Unity expects +Z forward**
- Assign Animator Controller and Avatar
- Set `cullingMode = AlwaysAnimate`

**Verify in Play mode:**
- Character visible and in correct position
- No mesh distortion
- Arms do not wing or stretch
- Compare screenshot against reference/front.png

---

## Checkpoint Strategy

| Checkpoint | When |
|---|---|
| `{Name}_pre_validation` | Before Step 2 |
| `{Name}_pre_lod` | Before decimate (if triggered) |
| `{Name}_pre_weight_fix` | Before Step 4b |
| `{Name}_pre_export` | Before Step 6 |

---

## Critical Rules

**Model Rotation:** CharacterModel MUST have `localRotation = (0, 180, 0)`. Meshy faces -Z; Unity expects +Z forward.

**Animator Culling:** Always `AlwaysAnimate`. `CullUpdateTransforms` skips bone writes when SkinnedMeshRenderer is a sibling of the Armature.

**FBX Scale:** Always `FBX_SCALE_ALL` + `bake_space_transform=True`. Deviating causes scale problems in Unity.

**Clear Blender Between Characters:** Old texture/mesh data persists in memory. Always clear before importing a new character.

**Humanoid for Blender-Native Rigs:** Use Humanoid rig type in Unity for characters rigged in Blender. The Mixamo-specific scale bug (0.01 localScale) does not affect Blender-native rigs.

**Never Re-import Mixamo FBX Through Blender:** Corrupts bind pose → NaN vertices → invisible character.

---

## File Structure (Final State)

```
art/characters/<CharacterName>/
  reference/
    front.png
    side.png (optional)
    back.png (optional)
  raw/
    *.fbx                         ← raw Meshy source
    *.png                         ← textures
  <CharacterName>_processed.fbx   ← Blender output
  <CharacterName>_rigged.blend    ← Blender file

Assets/Art/Characters/<CharacterName>/
  <CharacterName>.fbx
  MAT_<CharacterName>.mat
  Textures/
    <CharacterName>_BaseColor.png
    <CharacterName>_Metallic.png
    <CharacterName>_Roughness.png

Assets/Animation/<CharacterName>/
  <CharacterName>Animator.controller
```

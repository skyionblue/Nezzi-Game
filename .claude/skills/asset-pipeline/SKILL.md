---
name: asset-pipeline
description: Routes 3D assets (props, environment pieces) through the correct import pipeline. All assets go through Blender for validation, orientation fix, and UV checks before Unity import. For characters, use the character-importer skill instead.
---

# Asset Pipeline Skill

Routes any non-character game asset through the correct import pipeline. All assets are processed through Blender first — never copy raw Meshy FBX directly into Unity.

## Usage

```
/asset-pipeline <AssetName> <type> [source-path]
```

| Type | Example |
|---|---|
| `prop` | `/asset-pipeline Boulder prop art/raw/boulder.zip` |
| `env` | `/asset-pipeline RuinsArch env art/raw/ruins_arch.zip` |
| `character` | Delegate to `/character-importer` instead |

---

## Reference Images (Required)

**Minimum:** 1 front image  
**Recommended:** Front, side, and back (or top for large ENV pieces)

**Storage:**
```
art/<type>/<AssetName>/
  reference/
    front.png     ← REQUIRED
    side.png
    back.png
```

**If no references provided:** Prompt user to supply one, or offer to generate a baseline screenshot from the raw import.

---

## Routing Logic

```
type == character → use /character-importer skill instead
type == prop      → Static Mesh Pipeline
type == env       → Static Mesh Pipeline
```

---

## Static Mesh Pipeline

All static assets share this 9-step pipeline.

### Step SM-1: Extract & Import to Blender

```bash
mkdir -p art/<type>/<AssetName>
unzip -o "<sourcePath>" -d "art/<type>/<AssetName>"
```

**Clear Blender before each asset:**
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

```python
bpy.ops.import_scene.fbx(filepath='<path_to_fbx>')
```

---

### Step SM-2: Mesh Validation & Auto-Fix

```
create_checkpoint(name="{AssetName}_pre_validation")
validate_mesh(object_name)
```

**Auto-fix in order:**
1. `merge_vertices(distance=0.0001)`
2. `remove_doubles(threshold=0.0001)`
3. `recalculate_normals`
4. `fill_holes`

Re-run validation after fixes.

**Triangle budget:**

| Type | Budget | Over-budget action |
|---|---|---|
| Prop (small) | 200–600 tris | Prompt: "[Proceed] [Auto-decimate]" |
| Prop (medium) | 600–2,000 tris | Prompt: "[Proceed] [Auto-decimate]" |
| ENV | 200–1,000 tris | Prompt: "[Proceed] [Auto-decimate]" |

If auto-decimate selected:
1. `create_checkpoint(name="{AssetName}_pre_lod")`
2. `ratio = budget / current_tris`
3. `add_modifier(object_name, type='DECIMATE', ratio=ratio)`
4. `apply_modifier(object_name, modifier_name, preview=True)` — before/after screenshots
5. Prompt: "Reduced {original} → {result} tris. [Accept] [Adjust] [Rollback]"

---

### Step SM-3: UV Verification

- `get_uv_map_info(object_name)` — check existing UV layout
- If no UVs: `smart_uv_unwrap(island_margin=0.02)`, screenshot, confirm
- If UVs exist: verify no overlapping islands

---

### Step SM-4: Pivot & Orientation Fix

**Props:**
- `set_origin(type='ORIGIN_GEOMETRY')`
- Verify base is at Z=0 (upright on ground plane)
- Apply transforms

**ENV:**
- Get bounding box, translate so `min.z = 0`
- `apply_transforms(location=True, rotation=True, scale=True)`

**Reference comparison gate:**
- Screenshot front view
- Compare against reference/front.png
- Prompt: "Does orientation match? [Yes] [Rotate needed] [Manual fix]"

---

### Step SM-5: Material & Texture Extraction

**Identify textures via material node tree** (prevents wrong-texture bugs):
```python
obj = bpy.data.objects['MeshName']
mat = obj.data.materials[0]
textures = {}
for node in mat.node_tree.nodes:
    if node.type == 'TEX_IMAGE' and node.image:
        textures[node.image.name] = node.image
```

**When textures exist as separate files (preferred):**
```python
# Check disk first
import os
textures_on_disk = [f for f in os.listdir(src_dir) if f.endswith('.png')]
# If found: copy directly to Unity, skip Blender extraction
```

**When textures are packed in FBX:** Extract via material node connections only — do NOT iterate `bpy.data.images` globally.

**Multiple materials:** If the asset has multiple materials and the project needs a single material (mobile optimization):
- Prompt: "{count} materials. [Merge and bake] [Keep separate]"
- If merge: bake DIFFUSE and NORMAL using Blender bake tools, create single merged material

---

### Step SM-6: Scale Verification

Get bounding box dimensions and compare against expected real-world scale:

| Asset | Expected Size |
|---|---|
| Small prop (crate, barrel) | 0.4–0.8m |
| Medium prop (chest, boulder) | 0.8–1.5m |
| Large prop (door, gate) | 2–4m |
| ENV piece (arch, platform tile) | 1–5m |
| Background element (tree) | 4–10m |

Prompt: "Dimensions: {w}×{h}×{d}m. Correct for this asset? [Yes] [Adjust scale]"

---

### Step SM-7: FBX Export

```python
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

**Output:** `art/<type>/<AssetName>/<AssetName>_processed.fbx`

---

### Step SM-8: Unity Import

**Copy to Unity:**
```
Assets/Art/<Type>/<AssetName>/
  <AssetName>.fbx
  Textures/
    <AssetName>_BaseColor.png
    <AssetName>_Metallic.png  (if exists)
    <AssetName>_Roughness.png (if exists)
```

**Configure ModelImporter:**
```csharp
importer.globalScale = 1f;
importer.useFileScale = true;
importer.importAnimation = false;
importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
importer.SaveAndReimport();
```

**Create URP Lit material:**
```csharp
var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
mat.SetTexture("_BaseMap", baseColor);
if (metallic) mat.SetTexture("_MetallicGlossMap", metallic);
mat.SetFloat("_Smoothness", 0.3f);
```

**Type-specific colliders:**

| Type | Collider | Notes |
|---|---|---|
| Prop | MeshCollider or BoxCollider | isTrigger=false unless pickup item |
| ENV | MeshCollider | isTrigger=false, mark as Static |

**ENV static marking:**
```csharp
prefab.isStatic = true;
foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
    t.gameObject.isStatic = true;
```

---

### Step SM-9: Scene Placement & Verification

- Instantiate in the active test scene
- Position next to a 1m reference cube for scale check
- Screenshot
- Compare against reference/front.png
- Prompt: "Verify: [Scale correct] [Orientation correct] [Textures correct] [All good]"

---

### Step SM-10: Save as Prefab

After verification, save as a reusable prefab:

```
Assets/Prefabs/<Type>/<AssetName>.prefab
```

```csharp
PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
```

---

## Checkpoint Strategy

| Checkpoint | When |
|---|---|
| `{Name}_pre_validation` | Before SM-2 |
| `{Name}_pre_lod` | Before decimate (if triggered) |
| `{Name}_pre_export` | Before SM-7 |
| `{Name}_pre_unity` | Before SM-8 |

---

## Critical Rules

**Never copy raw Meshy FBX to Unity.** Always process through Blender first. Meshy uses Z-up coordinates; Unity expects Y-up.

**Clear Blender between assets.** Old texture blocks persist in memory and cause texture mix-ups on subsequent imports.

**FBX export settings are non-negotiable.** `axis_forward='-Z'`, `axis_up='Y'`, `FBX_SCALE_ALL`, `bake_space_transform=True`.

**Identify textures via material node connections.** Never iterate `bpy.data.images` globally — it includes textures from previously imported assets.

**ENV objects must be marked Static.** Required for batching and lightmapping.

**Material shader must be URP.** Any Standard shader material must be upgraded to `Universal Render Pipeline/Lit`.

---

## File Structure (Final State)

```
art/<type>/<AssetName>/
  reference/
    front.png
  raw/
    *.fbx
    *.png
  <AssetName>_processed.fbx

Assets/Art/<Type>/<AssetName>/
  <AssetName>.fbx
  MAT_<AssetName>.mat
  Textures/
    <AssetName>_BaseColor.png

Assets/Prefabs/<Type>/
  <AssetName>.prefab
```

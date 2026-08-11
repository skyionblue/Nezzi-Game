---
name: profiling-workflow
description: Guide frame budget setup, Unity Profiler sessions, and performance baseline establishment for mobile builds. Invoke when starting optimization work, after implementing a major feature, or when frame rate drops below target.
---

# Profiling Workflow Skill

Establishes frame budgets, runs Unity Profiler sessions, and validates performance against mobile targets (iOS and Android).

## When to Use

- Before starting a new milestone (establish baseline)
- After implementing a major feature (character controller, physics mechanics, camera system)
- When frame rate drops below 60 FPS on target devices
- To validate optimization work

---

## Frame Budget Setup

### Target: 16.6ms per frame (60 FPS)

| System | Budget | Unity Profiler Module |
|---|---|---|
| Rendering | 6–8ms | GPU Usage, Frame Debugger |
| Scripts | 3–4ms | CPU Usage (PlayerLoop) |
| Physics | 1–2ms | Physics |
| Animation | 1–1.5ms | CPU Usage (Animation) |
| Audio | 0.5–1ms | Audio |
| GC/Other | < 1ms | Memory |

**These are budgets, not targets.** If rendering takes 9ms, investigate — even if still hitting 60 FPS.

---

## Workflow: 4-Step Profiling Cycle

### Step 1: Build to Device

**Do NOT profile in the Editor** — overhead skews all numbers.

```
# iOS: File → Build Settings → iOS → Build and Run
# Android: File → Build Settings → Android → Build and Run
```

**Target devices:**
- iOS: iPhone 12 or newer
- Android: Samsung Galaxy A54 or equivalent mid-range (not flagship)

---

### Step 2: Connect Profiler

1. Device connected via USB
2. Unity Editor: `Window → Analysis → Profiler`
3. Profiler dropdown → select device (not "Editor")
4. Click **Record**

---

### Step 3: Capture Baseline

**What to profile:**
- 300 frames of typical gameplay (not menu, not transition)
- Include: characters moving, physics active, puzzle mechanics running, HUD visible
- Avoid: paused state, scene loading

**Metrics to record:**

| Metric | How to Read | Target |
|---|---|---|
| **Frame time (95th percentile)** | CPU Usage → select 300 frames → right panel percentiles | < 16.6ms |
| **Worst frame** | Scrub timeline for tallest spike | < 33ms (occasional OK) |
| **GC allocations** | Memory → GC Alloc column | 0 bytes/frame steady state |
| **Draw calls** | Rendering → Draw Calls | < 100 |
| **SetPass calls** | Rendering → SetPass Calls | < 50 |
| **Batched draws** | Rendering → Batches | > 50% of total |

**Save baseline:** `File → Save Profile` → `profiling/baseline-<milestone>.data`

---

### Step 4: Identify Bottlenecks

**CPU-bound:**
- CPU frame time consistently > GPU frame time
- High `PlayerLoop.Update` or `PlayerLoop.FixedUpdate`
- Scripts category > 4ms

**Action:** Audit Update loops, physics callbacks, and coroutines. Look for GC-allocating code (`string.Format`, LINQ, closures in hot paths).

---

**GPU-bound:**
- GPU frame time consistently > CPU frame time
- Draw calls > 100
- High `Render.OpaqueGeometry` or `Render.TransparentGeometry`

**Action:**
1. `Window → Analysis → Frame Debugger`
2. Step through draw calls — look for:
   - Same shader/material called repeatedly without batching
   - Transparent UI elements rendered multiple times (overdraw)
   - Shadow caster passes on small props (disable shadow casting on small objects)

---

**Memory-bound:**
- GC.Collect spikes every few seconds
- Total allocated > 300 MB and growing
- Texture memory > 200 MB

**Action:**
1. Install Memory Profiler: `Window → Package Manager → Memory Profiler`
2. `Window → Analysis → Memory Profiler → Capture`
3. Sort Textures and Meshes by size descending
4. Re-import oversized textures with ASTC/ETC2 compression

---

## Custom Profiler Markers

Add markers to measure specific game systems:

```csharp
using Unity.Profiling;

public class ScarletController : MonoBehaviour
{
    private static readonly ProfilerMarker _pushMarker = new ProfilerMarker("Scarlet.PushBoulder");

    private void HandlePush()
    {
        using (_pushMarker.Auto())
        {
            // push logic
        }
    }
}
```

**Recommended markers to add:**

| System | Marker Name | Location |
|---|---|---|
| Input routing | `Input.RouteCharacter` | `InputRouter.cs` |
| Character movement | `Character.Move` | `CharacterBase.cs` |
| Physics interactions | `Puzzle.PressurePlate` | `PressurePlate.cs` |
| Camera | `Camera.Follow` | `CameraController.cs` |
| HUD | `UI.HUDUpdate` | `HUDController.cs` |

Markers appear in Profiler under "Scripts" → expand for nested timing.

---

## Device Testing Checklist

Before declaring a feature "performance validated":

- [ ] Profiled on iOS device (iPhone 12 or newer)
- [ ] Profiled on Android device (Galaxy A54 or equivalent mid-range)
- [ ] **95th percentile frame time < 16.6ms** (not just average)
- [ ] Zero GC allocations during 300-frame steady gameplay
- [ ] Draw calls < 100 per frame
- [ ] Texture memory < 300 MB
- [ ] No thermal throttling after 10 minutes of play

**Thermal check:** Play for 10 minutes, touch back of device. If uncomfortably hot, thermal throttling is reducing FPS — optimize further.

---

## Common Mistakes

| Mistake | Why Wrong | Correct Approach |
|---|---|---|
| Profiling in Editor | 2–3× overhead makes numbers meaningless | Always profile on-device |
| Looking at average FPS | 59 FPS average hides 30 FPS spikes | Track 95th percentile and worst frame |
| Optimizing the wrong bottleneck | CPU fixes don't help GPU-bound builds | Identify CPU vs GPU bound first |
| No baseline before optimization | Can't prove improvement | Capture baseline before any optimization |
| Single frame analysis | One frame is noise | Analyze 300-frame window |
| Ignoring GC spikes | "Only 2ms" still causes stutter | Zero tolerance for GC in gameplay |

---

## Output Format

When this skill completes, produce:

```markdown
## Profiling Session — [Milestone/Date]

**Device:** [iPhone model / Android model]
**Scene:** [Scene profiled]
**Scenario:** [What was happening during the 300-frame capture]

### Baseline Metrics

- Frame time (95th percentile): X.Xms
- Worst frame: X.Xms
- GC allocations: X bytes/frame
- Draw calls: X
- Texture memory: X MB

### Bottleneck Analysis

**Primary:** [CPU-bound / GPU-bound / Memory-bound]
**Evidence:** [Specific profiler readings that indicate the bottleneck]

### Recommended Next Steps

1. [Specific action]
2. [Specific action]
3. [Specific action]

### Profiler Data

Saved to: `profiling/<milestone>-baseline.data`
```

---

## Files & Directories

```
profiling/
  baseline-<milestone>.data       ← Unity Profiler capture (binary)
  baseline-<milestone>.png        ← Screenshot of Profiler timeline
  post-optimization-<milestone>.data
  comparison-<milestone>.md       ← Before/after metrics table
```

Create `profiling/` at repo root if it doesn't exist.

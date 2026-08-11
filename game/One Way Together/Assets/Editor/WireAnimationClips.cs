// WireAnimationClips.cs — one-shot Editor utility.
// Triggered via: Tools > Wire Animation Clips
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class WireAnimationClips
{
    [MenuItem("Tools/Wire Animation Clips")]
    public static void Run()
    {
        WireScarlet();
        WireDani();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WireAnimationClips] Done. All clips assigned and assets saved.");
    }

    private static AnimationClip LoadClip(string assetPath, string clipName)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object obj in all)
        {
            if (obj is AnimationClip clip && clip.name == clipName)
                return clip;
        }
        foreach (Object obj in all)
        {
            if (obj is AnimationClip clip)
            {
                Debug.LogWarning($"[WireAnimationClips] '{clipName}' not found by name in {assetPath}; using '{clip.name}'.");
                return clip;
            }
        }
        Debug.LogError($"[WireAnimationClips] No AnimationClip found in {assetPath}");
        return null;
    }

    private static void AssignMotion(AnimatorStateMachine sm, string stateName, AnimationClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[WireAnimationClips] Skipping '{stateName}' - clip is null.");
            return;
        }
        foreach (ChildAnimatorState cas in sm.states)
        {
            if (cas.state.name == stateName)
            {
                cas.state.motion = clip;
                EditorUtility.SetDirty(cas.state);
                Debug.Log($"[WireAnimationClips] {stateName} <- {clip.name}");
                return;
            }
        }
        Debug.LogWarning($"[WireAnimationClips] State '{stateName}' not found in '{sm.name}'.");
    }

    private static void WireScarlet()
    {
        const string controllerPath = "Assets/Animation/Scarlet/ScarletAnimator.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) { Debug.LogError($"[WireAnimationClips] Cannot load {controllerPath}"); return; }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        const string p = "Assets/Kevin Iglesias/Human Animations/Animations/Male";

        AssignMotion(sm, "Idle",  LoadClip($"{p}/Idles/HumanM@Idle01.fbx", "HumanM@Idle01"));
        AssignMotion(sm, "Walk",  LoadClip($"{p}/Movement/Walk/HumanM@Walk01_Forward.fbx", "HumanM@Walk01_Forward"));
        AssignMotion(sm, "Jump",  LoadClip($"{p}/Movement/Jump/HumanM@Jump01 - Begin.fbx", "HumanM@Jump01 - Begin"));
        AssignMotion(sm, "Lift",  LoadClip($"{p}/Idles/HumanM@Idle01.fbx", "HumanM@Idle01"));

        EditorUtility.SetDirty(controller);
        Debug.Log("[WireAnimationClips] ScarletAnimator.controller wired.");
    }

    private static void WireDani()
    {
        const string controllerPath = "Assets/Animation/Dani/DaniAnimator.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) { Debug.LogError($"[WireAnimationClips] Cannot load {controllerPath}"); return; }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        const string p = "Assets/Kevin Iglesias/Human Animations/Animations/Female";

        AssignMotion(sm, "Idle",  LoadClip($"{p}/Idles/HumanF@Idle01.fbx", "HumanF@Idle01"));
        AssignMotion(sm, "Walk",  LoadClip($"{p}/Movement/Walk/HumanF@Walk01_Forward.fbx", "HumanF@Walk01_Forward"));
        AssignMotion(sm, "Jump",  LoadClip($"{p}/Movement/Jump/HumanF@Jump01 - Begin.fbx", "HumanF@Jump01 - Begin"));
        // No crouch-walk in FREE package - forward walk used as closest substitute.
        AssignMotion(sm, "Crawl", LoadClip($"{p}/Movement/Walk/HumanF@Walk01_Forward.fbx", "HumanF@Walk01_Forward"));
        // No climb clip in FREE package - Idle used as placeholder.
        AssignMotion(sm, "Climb", LoadClip($"{p}/Idles/HumanF@Idle01.fbx", "HumanF@Idle01"));

        EditorUtility.SetDirty(controller);
        Debug.Log("[WireAnimationClips] DaniAnimator.controller wired.");
    }
}

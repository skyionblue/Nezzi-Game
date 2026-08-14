#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneWayTogether.Editor
{
    /// <summary>
    /// Editor window that lists every zone in CityZoneSequencer dynamically.
    /// No code changes needed when new puzzle zones are added.
    /// Open via: Tools → One Way Together → Jump to Zone…
    /// </summary>
    public class TestStartAtZone : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Tools/One Way Together/Jump to Zone…")]
        static void OpenWindow()
        {
            var win = GetWindow<TestStartAtZone>("Jump to Zone");
            win.minSize = new Vector2(280, 160);
            win.Show();
        }

        void OnGUI()
        {
            var seq = FindAnyObjectByType<Core.CityZoneSequencer>();
            if (seq == null)
            {
                EditorGUILayout.HelpBox("Open CityWorld scene first.", MessageType.Warning);
                return;
            }

            var bf         = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var zonesField = typeof(Core.CityZoneSequencer).GetField("_zones", bf);
            var zones      = zonesField?.GetValue(seq) as System.Array;
            if (zones == null || zones.Length == 0)
            {
                EditorGUILayout.HelpBox("No zones configured on CityZoneSequencer.", MessageType.Info);
                return;
            }

            var zoneType    = typeof(Core.CityZoneSequencer).GetNestedType("ZoneAnchor");
            var currentField= typeof(Core.CityZoneSequencer).GetField("_currentZone", bf);
            int current     = (int)(currentField?.GetValue(seq) ?? 0);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Select a zone to jump to:", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < zones.Length; i++)
            {
                var z    = zones.GetValue(i);
                var root = zoneType.GetField("zoneRoot").GetValue(z) as GameObject;

                // Label: "Zone 0  (active)" or "Zone 0 — Puzzle2_ParkingLot"
                string zoneName = root != null ? root.name : "(Zone 0 — always active)";
                string label    = "Zone " + i + " — " + zoneName;
                bool   isCurrent= i == current;

                GUI.color = isCurrent ? new Color(0.6f, 1f, 0.6f) : Color.white;
                if (GUILayout.Button(label, GUILayout.Height(28)))
                    SetupZone(seq, zones, zoneType, i);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Click a zone, then press Play.\n" +
                "Zone roots are activated/deactivated automatically.\n" +
                "Characters are teleported to each zone's spawn point.",
                MessageType.None);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Health Check", EditorStyles.boldLabel);
            if (GUILayout.Button("✔  Verify All ReunionTriggers", GUILayout.Height(26)))
                VerifyReunionTriggers();
        }

        static void VerifyReunionTriggers()
        {
            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int fixed2 = 0, ok = 0;
            foreach (var t in all)
            {
                if (!t.name.Contains("Reunion") || t.parent == null) continue;
                var rt = t.GetComponent<Puzzle.ReunionTrigger>();
                if (rt != null) { ok++; continue; }

                // Missing — add it back
                t.gameObject.AddComponent<Puzzle.ReunionTrigger>();
                var bc = t.GetComponent<BoxCollider>();
                if (bc != null) { bc.isTrigger = true; bc.size = new Vector3(5f,2f,5f); bc.center = new Vector3(0f,1f,0f); }
                EditorUtility.SetDirty(t.gameObject);
                fixed2++;
                Debug.LogWarning($"[HealthCheck] ReunionTrigger was MISSING on {t.name} — re-added.");
            }

            var scene = SceneManager.GetActiveScene();
            if (fixed2 > 0) EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[HealthCheck] ReunionTriggers: {ok} OK, {fixed2} fixed. " +
                      (fixed2 > 0 ? "Scene marked dirty — save before building." : "All good!"));
        }

        static void SetupZone(Core.CityZoneSequencer seq, System.Array zones,
                               System.Type zoneType, int zoneIndex)
        {
            var bf          = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var currentField= typeof(Core.CityZoneSequencer).GetField("_currentZone", bf);

            // Activate only the selected zone root
            for (int i = 0; i < zones.Length; i++)
            {
                var z    = zones.GetValue(i);
                var root = zoneType.GetField("zoneRoot").GetValue(z) as GameObject;
                if (root == null) continue;
                root.SetActive(i == zoneIndex);
            }

            currentField.SetValue(seq, zoneIndex);
            EditorUtility.SetDirty(seq.gameObject);

            // Teleport characters to spawn.
            // Zone 0 always resets to the hardcoded Puzzle 1 positions so the
            // scene is always in a valid state for a real game launch.
            var targetZone  = zones.GetValue(zoneIndex);
            var spawnCenter = (Vector3)zoneType.GetField("spawnCenter").GetValue(targetZone);
            if (zoneIndex == 0 || spawnCenter == Vector3.zero)
            {
                MoveCharacter("Scarlet", new Vector3(-104f, 22f, 272f));
                MoveCharacter("Dani",    new Vector3( -96f, 22f, 272f));
            }
            else
            {
                MoveCharacter("Scarlet", spawnCenter + new Vector3(-4f, 0f, 0f));
                MoveCharacter("Dani",    spawnCenter + new Vector3( 4f, 0f, 0f));
            }

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[TestStartAtZone] Ready at Zone " + zoneIndex + ". Press Play.");
        }

        static void MoveCharacter(string tag, Vector3 pos)
        {
            var go = GameObject.FindWithTag(tag);
            if (go != null) { go.transform.position = pos; EditorUtility.SetDirty(go); }
        }
    }
}

#endif

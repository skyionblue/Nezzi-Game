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

            // Teleport characters to spawn
            var targetZone  = zones.GetValue(zoneIndex);
            var spawnCenter = (Vector3)zoneType.GetField("spawnCenter").GetValue(targetZone);
            if (spawnCenter != Vector3.zero)
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

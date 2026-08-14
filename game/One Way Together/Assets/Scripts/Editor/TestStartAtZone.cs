using UnityEditor;
using UnityEngine;

namespace OneWayTogether.Editor
{
    /// <summary>
    /// Editor-only helper to start play mode at a specific puzzle zone.
    /// Tools → One Way Together → Start at Zone N
    /// </summary>
    public static class TestStartAtZone
    {
        [MenuItem("Tools/One Way Together/Start at Zone 0 (Puzzle 1)")]
        static void Zone0() => SetupZone(0);

        [MenuItem("Tools/One Way Together/Start at Zone 1 (Puzzle 2)")]
        static void Zone1() => SetupZone(1);

        [MenuItem("Tools/One Way Together/Start at Zone 2 (Puzzle 3)")]
        static void Zone2() => SetupZone(2);

        [MenuItem("Tools/One Way Together/Start at Zone 3 (Puzzle 4 - Hospital)")]
        static void Zone3() => SetupZone(3);

        static void SetupZone(int zoneIndex)
        {
            // Find the sequencer and set its starting zone
            var seq = Object.FindAnyObjectByType<Core.CityZoneSequencer>();
            if (seq == null) { Debug.LogError("CityZoneSequencer not found. Open CityWorld scene first."); return; }

            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var zonesField = typeof(Core.CityZoneSequencer).GetField("_zones", bf);
            var currentField = typeof(Core.CityZoneSequencer).GetField("_currentZone", bf);
            var zones = zonesField.GetValue(seq) as System.Array;
            if (zones == null || zoneIndex >= zones.Length) { Debug.LogError("Zone " + zoneIndex + " out of range."); return; }

            // Activate the requested zone root, deactivate others
            var zoneType = typeof(Core.CityZoneSequencer).GetNestedType("ZoneAnchor");
            for (int i = 0; i < zones.Length; i++)
            {
                var z = zones.GetValue(i);
                var root = zoneType.GetField("zoneRoot").GetValue(z) as GameObject;
                if (root == null) continue;
                root.SetActive(i == zoneIndex);
            }

            // Set the sequencer to start at this zone
            currentField.SetValue(seq, zoneIndex);
            EditorUtility.SetDirty(seq.gameObject);

            // Move characters to the zone spawn point
            var targetZone = zones.GetValue(zoneIndex);
            var spawnCenter = (Vector3)zoneType.GetField("spawnCenter").GetValue(targetZone);
            if (spawnCenter != Vector3.zero)
            {
                MoveCharacter("Scarlet", spawnCenter + new Vector3(-4f, 0f, 0f));
                MoveCharacter("Dani",    spawnCenter + new Vector3( 4f, 0f, 0f));
            }

            EditorSceneManager_MarkDirty();
            Debug.Log("[TestStartAtZone] Ready at Zone " + zoneIndex + ". Press Play to begin there.");
        }

        static void MoveCharacter(string tag, Vector3 pos)
        {
            var go = GameObject.FindWithTag(tag);
            if (go != null) { go.transform.position = pos; EditorUtility.SetDirty(go); }
        }

        // Avoids a 'using' import that isn't available in all Unity versions
        static void EditorSceneManager_MarkDirty()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}

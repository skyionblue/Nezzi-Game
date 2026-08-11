using UnityEngine;
using UnityEditor;
using OneWayTogether.Data;

namespace OneWayTogether.Editor
{
    /// <summary>
    /// One-shot bootstrap that creates the required ScriptableObject assets.
    /// Run via menu: OneWayTogether/Bootstrap/Create ScriptableObject Assets
    /// Safe to run multiple times — skips assets that already exist.
    /// </summary>
    public static class ScriptableObjectBootstrap
    {
        [MenuItem("OneWayTogether/Bootstrap/Create ScriptableObject Assets")]
        public static void CreateAssets()
        {
            CreateFolderIfMissing("Assets", "ScriptableObjects");
            CreateFolderIfMissing("Assets/ScriptableObjects", "Characters");
            CreateFolderIfMissing("Assets/ScriptableObjects", "Coins");

            CreateAssetIfMissing<CharacterData>("Assets/ScriptableObjects/Characters/ScarletData.asset");
            CreateAssetIfMissing<CharacterData>("Assets/ScriptableObjects/Characters/DaniData.asset");
            CreateAssetIfMissing<CoinSystemData>("Assets/ScriptableObjects/Coins/CoinSystemData.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ScriptableObjectBootstrap] All ScriptableObject assets created successfully.");
        }

        private static void CreateFolderIfMissing(string parent, string folderName)
        {
            string full = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void CreateAssetIfMissing<T>(string path) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
            {
                Debug.Log($"[ScriptableObjectBootstrap] Already exists, skipping: {path}");
                return;
            }
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[ScriptableObjectBootstrap] Created: {path}");
        }
    }
}

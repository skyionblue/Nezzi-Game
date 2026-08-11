using UnityEditor;

/// <summary>
/// One-shot AssetPostprocessor that configures the Scarlet FBX on first import.
/// Delete this file after confirming the FBX is correctly imported.
/// </summary>
public class ScarletImporter : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!assetPath.Contains("Scarlet.fbx"))
            return;

        ModelImporter importer = assetImporter as ModelImporter;
        if (importer == null)
            return;

        importer.animationType      = ModelImporterAnimationType.Human;
        importer.avatarSetup        = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.globalScale        = 1f;
        importer.useFileScale       = true;
        importer.importAnimation    = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
    }
}

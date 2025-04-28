using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Fwk.Editor
{
    public static class AddressablesBuildTool
    {
        [MenuItem("Tools/Build/Build Addressables")]
        public static void BuildAddressables()
        {
            Debug.Log("[AddressablesBuildTool] Starting Addressables Build...");

            // Step 1: Backup original settings
            AddressableFolderAutoAssign.BackupOriginalSettings();

            // Step 2: Prepare temporary build settings
            AddressableFolderAutoAssign.PrepareBuildSettings();

            // Step 3: Auto assign addressables
            AddressableFolderAutoAssign.AssignAddressables();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Step 4: Build Addressables
            AddressableAssetSettings.BuildPlayerContent();

            // Step 5: Restore original settings
            AddressableFolderAutoAssign.RestoreOriginalSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

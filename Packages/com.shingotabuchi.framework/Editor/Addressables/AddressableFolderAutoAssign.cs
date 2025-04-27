using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fwk.Editor
{
    [InitializeOnLoad]
    public static class AddressableFolderAutoAssign
    {
        private const string FolderToScan = "Assets/AddressableResources";
        private const string DefaultGroupName = "Default Local Group";
        private const string SettingsAssetPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        private const string BackupAssetPath = "Assets/AddressableAssetsData/AddressableAssetSettings_Backup.asset";


        static AddressableFolderAutoAssign()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    BackupSettingsFile();
                    AssignAddressables();
                }
                else if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    RestoreSettingsFile();
                }
            };
        }

        private static void BackupSettingsFile()
        {
            if (File.Exists(SettingsAssetPath))
            {
                File.Copy(SettingsAssetPath, BackupAssetPath, overwrite: true);
                Debug.Log("[AddressableFolderAutoAssign] Backed up AddressableAssetSettings.asset.");
            }
        }

        private static void RestoreSettingsFile()
        {
            if (File.Exists(BackupAssetPath))
            {
                if (File.Exists(SettingsAssetPath))
                {
                    File.Delete(SettingsAssetPath);
                }
                File.Move(BackupAssetPath, SettingsAssetPath);
                File.Delete(BackupAssetPath);
                AssetDatabase.Refresh();
                Debug.Log("[AddressableFolderAutoAssign] Restored AddressableAssetSettings.asset to original state.");
            }
            else
            {
                Debug.LogWarning("[AddressableFolderAutoAssign] No backup found to restore!");
            }
        }

        public static void AssignAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressableFolderAutoAssign] Could not load AddressableAssetSettings.");
                return;
            }

            var templateGroup = settings.DefaultGroup;
            var defaultSchemas = templateGroup.Schemas.ToList();

            var guids = AssetDatabase.FindAssets("", new[] { FolderToScan })
                .Where(g =>
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    return !AssetDatabase.IsValidFolder(p);
                })
                .ToArray();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var rel = path.Substring(FolderToScan.Length).TrimStart('/');
                var slash = rel.IndexOf('/');
                var groupName = slash >= 0
                    ? rel.Substring(0, slash)
                    : DefaultGroupName;

                var group = settings.FindGroup(groupName)
                    ?? settings.CreateGroup(groupName, false, false, false, defaultSchemas);

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = Path.GetFileNameWithoutExtension(path);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AddressableFolderAutoAssign] Assigned {guids.Length} assets (temporary for Play Mode).");
        }
    }

    public class AddressableFolderAutoAssignBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            AddressableFolderAutoAssign.AssignAddressables();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

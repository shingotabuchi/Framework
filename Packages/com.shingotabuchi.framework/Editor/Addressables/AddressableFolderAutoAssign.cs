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
        private const string PlayModeSettingsPath = "Assets/AddressableAssetsData/Build/AddressableAssetSettings_PlayMode.asset";
        private const string BackupSettingsPath = "Assets/AddressableAssetsData/Build/AddressableAssetSettings_Backup.asset";

        static AddressableFolderAutoAssign()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    BackupOriginalSettings();
                    PreparePlayModeSettings();
                    AssignAddressables();
                }
                else if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    RestoreOriginalSettings();
                }
            };
        }

        private static void BackupOriginalSettings()
        {
            if (File.Exists(SettingsAssetPath))
            {
                var buildDir = Path.GetDirectoryName(BackupSettingsPath);
                if (!Directory.Exists(buildDir))
                {
                    Directory.CreateDirectory(buildDir);
                }

                File.Copy(SettingsAssetPath, BackupSettingsPath, overwrite: true);
                Debug.Log("[AddressableFolderAutoAssign] Backed up AddressableAssetSettings.asset.");
            }
            else
            {
                Debug.LogWarning("[AddressableFolderAutoAssign] No AddressableAssetSettings.asset found to backup.");
            }
        }

        private static void PreparePlayModeSettings()
        {
            if (File.Exists(SettingsAssetPath))
            {
                File.Copy(SettingsAssetPath, PlayModeSettingsPath, overwrite: true);
                AssetDatabase.Refresh();

                var playModeSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(PlayModeSettingsPath);
                if (playModeSettings != null)
                {
                    AddressableAssetSettingsDefaultObject.Settings = playModeSettings;
                    Debug.Log("[AddressableFolderAutoAssign] Using Play Mode AddressableAssetSettings.");
                }
                else
                {
                    Debug.LogError("[AddressableFolderAutoAssign] Failed to load Play Mode AddressableAssetSettings.");
                }
            }
            else
            {
                Debug.LogWarning("[AddressableFolderAutoAssign] No AddressableAssetSettings.asset found to prepare for Play Mode.");
            }
        }

        private static void RestoreOriginalSettings()
        {
            if (File.Exists(BackupSettingsPath))
            {
                if (File.Exists(SettingsAssetPath))
                {
                    File.Delete(SettingsAssetPath);
                }

                File.Move(BackupSettingsPath, SettingsAssetPath);
                Debug.Log("[AddressableFolderAutoAssign] Restored AddressableAssetSettings.asset from backup.");
            }
            else
            {
                Debug.LogWarning("[AddressableFolderAutoAssign] No backup found to restore AddressableAssetSettings.asset.");
            }

            if (File.Exists(PlayModeSettingsPath))
            {
                File.Delete(PlayModeSettingsPath);
                Debug.Log("[AddressableFolderAutoAssign] Deleted temporary Play Mode AddressableAssetSettings.");
            }

            AssetDatabase.Refresh();

            var originalSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(SettingsAssetPath);
            if (originalSettings != null)
            {
                AddressableAssetSettingsDefaultObject.Settings = originalSettings;
                Debug.Log("[AddressableFolderAutoAssign] Restored original AddressableAssetSettings.");
            }
            else
            {
                Debug.LogWarning("[AddressableFolderAutoAssign] No original AddressableAssetSettings found to restore.");
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
            Debug.Log($"[AddressableFolderAutoAssign] Assigned {guids.Length} assets (temporary Play Mode only).");
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

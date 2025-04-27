using System.Collections.Generic;
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

        private static List<GroupBackup> backupGroups;
        private static bool isPlayModeChange = false;

        static AddressableFolderAutoAssign()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    BackupGroups();
                    AssignAddressables();
                    isPlayModeChange = true;
                }
                else if (state == PlayModeStateChange.EnteredEditMode && isPlayModeChange)
                {
                    RestoreGroups();
                    isPlayModeChange = false;
                }
            };
        }

        private static void BackupGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            backupGroups = new List<GroupBackup>();

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                var entries = group.entries.Select(e => new EntryBackup
                {
                    guid = e.guid,
                    address = e.address,
                    labels = e.labels.ToList()
                }).ToList();

                backupGroups.Add(new GroupBackup
                {
                    groupName = group.Name,
                    entries = entries
                });
            }

            Debug.Log("[AddressableFolderAutoAssign] Groups backed up.");
        }

        private static void RestoreGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || backupGroups == null) return;

            // Delete all non-null groups
            foreach (var group in settings.groups.ToList())
            {
                if (group != null && group != settings.DefaultGroup)
                {
                    settings.RemoveGroup(group);
                }
            }

            var entries = settings.DefaultGroup.entries.ToList();
            foreach (var entry in entries)
            {
                settings.DefaultGroup.RemoveAssetEntry(entry);
            }
            // Restore backup
            foreach (var groupBackup in backupGroups)
            {
                var group = groupBackup.groupName == DefaultGroupName
                          ? settings.DefaultGroup
                          : settings.FindGroup(groupBackup.groupName) ?? settings.CreateGroup(groupBackup.groupName, false, false, false, settings.DefaultGroup.Schemas.ToList());

                foreach (var entryBackup in groupBackup.entries)
                {
                    var entry = settings.CreateOrMoveEntry(entryBackup.guid, group, false, false);
                    entry.address = entryBackup.address;
                    foreach (var label in entryBackup.labels)
                    {
                        entry.SetLabel(label, true, true);
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[AddressableFolderAutoAssign] Groups restored after Play Mode.");
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
                         ?? settings.CreateGroup(groupName,
                                                 false,
                                                 false,
                                                 false,
                                                 defaultSchemas);

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = Path.GetFileNameWithoutExtension(path);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);

            Debug.Log($"[AddressableFolderAutoAssign] Assigned {guids.Length} assets (temporary for Play Mode only).");
        }

        private class GroupBackup
        {
            public string groupName;
            public List<EntryBackup> entries;
        }

        private class EntryBackup
        {
            public string guid;
            public string address;
            public List<string> labels;
        }
    }

    public class AddressableFolderAutoAssignBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            AddressableFolderAutoAssign.AssignAddressables();
            AssetDatabase.SaveAssets();
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
internal static class AutoSyncSolution
{
    static AutoSyncSolution()
    {
        CompilationPipeline.compilationFinished += _ => SyncProjectFiles();
    }

    private static void SyncProjectFiles()
    {
        // ── locate the first *.sln* in the project root ────────────────────────────
        var projectRoot = Path.GetDirectoryName(Application.dataPath);
        var slnPath = Directory.GetFiles(projectRoot, "*.sln").FirstOrDefault();

        // Debug.Log($"[AutoSyncSolution] Syncing project files: {slnPath}");

        DateTime preTime = slnPath != null ? File.GetLastWriteTimeUtc(slnPath)
                                           : DateTime.MinValue;

        bool success = TrySyncViaCodeEditor() || TrySyncViaSyncVS();

        // ── quick validation ───────────────────────────────────────────────────────
        if (!success)
        {
            Debug.LogWarning("[AutoSyncSolution] No sync API found ‑ project files NOT regenerated.");
            return;
        }

        if (slnPath == null)
        {
            Debug.Log("[AutoSyncSolution] Sync triggered (no existing .sln to compare).");
            return;
        }

        DateTime postTime = File.GetLastWriteTimeUtc(slnPath);

        // if (postTime > preTime)
        //     Debug.Log($"[AutoSyncSolution] .sln regenerated ({preTime:HH:mm:ss} → {postTime:HH:mm:ss}).");
        // else
        //     Debug.Log("[AutoSyncSolution] Sync method ran, but .sln timestamp didn’t change.");
    }

    private static bool TrySyncViaCodeEditor()
    {
        // public API route (requires an IDE integration package)
        var codeEditorType = typeof(Editor).Assembly.GetType("Unity.CodeEditor.CodeEditor");
        var currentEditor = codeEditorType?
            .GetProperty("CurrentEditor", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null);

        var syncAll = currentEditor?.GetType().GetMethod("SyncAll");
        if (syncAll == null) return false;

        syncAll.Invoke(currentEditor, null);
        return true;
    }

    private static bool TrySyncViaSyncVS()
    {
        // reflection fallback on internal class
        var syncVSType = typeof(Editor).Assembly.GetType("UnityEditor.SyncVS");
        var sync = syncVSType?.GetMethod("SyncSolution",
                          BindingFlags.NonPublic | BindingFlags.Static);
        if (sync == null) return false;

        sync.Invoke(null, null);
        return true;
    }
}

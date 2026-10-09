using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build entry points for the Unity CLI and the Build menu:
//   unity run <project> -- -executeMethod BuildScript.BuildWindows
//   unity run <project> -- -executeMethod BuildScript.BuildAndroid
// Output goes to Builds/ (ignored by git). In batch mode the editor exits non-zero on failure.
public static class BuildScript
{
    private const string AndroidPackage = "com.nttp.zombiesurvival";

    [MenuItem("Build/Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/ZombieSurvival.exe");
    }

    [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        // The Standalone id contains a hyphen, which Android package names don't allow
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackage);
        // 64-bit ARM needs IL2CPP; many current phones no longer run 32-bit apps
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        // Landscape only, either way up
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        EditorUserBuildSettings.buildAppBundle = false;
        Build(BuildTarget.Android, "Builds/Android/ZombieSurvival.apk");
    }

    private static void Build(BuildTarget target, string path)
    {
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target),
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[BuildScript] {target}: {summary.result}, {summary.totalSize / (1024 * 1024)} MB, " +
                  $"{summary.totalErrors} errors, {summary.totalTime.TotalSeconds:0}s -> {path}");
        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}

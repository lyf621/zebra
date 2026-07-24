using System.IO;
using System.Collections.Generic;
using UnityEditor;

public static class WebGLBuilder
{
    // 将正式游戏构建到命令行指定的 WebGL 目录。
    public static void Build()
    {
        string outputPath = GetOutputPath("Build/WebGL");
        Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = GetEnabledScenes(), locationPathName = outputPath, target = BuildTarget.WebGL, options = BuildOptions.None });
    }

    public static void BuildWindows()
    {
        string outputPath = GetOutputPath("Build/Windows/MapAndEvents.exe");
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = GetEnabledScenes(), locationPathName = outputPath, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
    }

    private static string GetOutputPath(string defaultPath)
    {
        string outputPath = defaultPath;
        string[] arguments = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == "-outputPath")
            {
                outputPath = arguments[i + 1];
            }
        }
        return outputPath;
    }

    private static string[] GetEnabledScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                scenes.Add(scene.path);
        }

        return scenes.Count > 0 ? scenes.ToArray() : new[] { "Assets/Scenes/MainMap.unity" };
    }
}

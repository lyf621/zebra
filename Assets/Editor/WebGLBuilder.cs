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

    // GitHub Pages does not reliably emit Content-Encoding for Unity's .br files. Build an
    // uncompressed variant so the published site works without server-specific headers.
    public static void BuildPages()
    {
        string outputPath = GetOutputPath("Build/WebGLPages");
        Directory.CreateDirectory(outputPath);

        // Batch builds can inherit development flags from the last Editor session.
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = false;

        WebGLCompressionFormat originalCompression = PlayerSettings.WebGL.compressionFormat;
        try
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = GetEnabledScenes(), locationPathName = outputPath, target = BuildTarget.WebGL, options = BuildOptions.None });
        }
        finally
        {
            PlayerSettings.WebGL.compressionFormat = originalCompression;
        }
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

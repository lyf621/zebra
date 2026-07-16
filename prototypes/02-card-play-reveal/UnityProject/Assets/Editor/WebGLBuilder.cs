using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLBuilder
{
    // 构建可直接放在 GitHub Pages 上运行的无压缩 WebGL 版本。
    public static void Build()
    {
        string outputPath = Environment.GetEnvironmentVariable("WEBGL_BUILD_PATH");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("WEBGL_BUILD_PATH is required.");
        }

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/CardPlayReveal.unity" },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
        }
    }
}

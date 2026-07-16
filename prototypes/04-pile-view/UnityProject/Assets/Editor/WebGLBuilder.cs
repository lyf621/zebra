using System.IO;
using UnityEditor;

public static class WebGLBuilder
{
    // 将当前场景构建到命令行指定的 WebGL 目录。
    public static void Build()
    {
        string outputPath = "Build/WebGL";
        string[] arguments = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == "-outputPath")
            {
                outputPath = arguments[i + 1];
            }
        }
        Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/PileView.unity" }, locationPathName = outputPath, target = BuildTarget.WebGL, options = BuildOptions.None });
    }
}

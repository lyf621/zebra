using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneBuilder
{
    // 创建正式游戏主场景，并将其设为唯一构建场景。
    [MenuItem("Zebra/Create Main Game Scene")]
    public static void Build()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.07f);
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        new GameObject("Zebra Game Controller").AddComponent<ZebraGameController>();

        const string scenePath = "Assets/Scenes/MainGame.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        PlayerSettings.productName = "Zebra";
        PlayerSettings.companyName = "Zebra Team";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.WebGL.template = "PROJECT:ZebraResponsive";
        AssetDatabase.SaveAssets();
    }
}

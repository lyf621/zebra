using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeSceneBuilder
{
    // 创建纯 2D 买牌删牌场景，并将其设为唯一构建场景。
    [MenuItem("Prototype/Create Card Buy-Delete Scene")]
    public static void Build()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.white;
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameObject prototypeObject = new GameObject("Card Buy Delete Prototype");
        prototypeObject.AddComponent<CardBuyDeletePrototype>();

        const string scenePath = "Assets/Scenes/CardBuyDelete.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        PlayerSettings.productName = "Card Buy Delete Prototype";
        PlayerSettings.companyName = "Zebra Team";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        AssetDatabase.SaveAssets();
    }
}

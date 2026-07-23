using UnityEngine;

/// <summary>
/// Persistent looping background music for the whole game.
///
/// A single music player is created automatically at application start
/// (RuntimeInitializeOnLoadMethod), survives every scene load (DontDestroyOnLoad),
/// and plays the track on loop across MainMenu, MainMap and TutorialScene.
/// No per-scene setup and no Inspector wiring are required — it just works once the
/// script and the clip exist in the project.
///
/// The clip is loaded from Resources/Audio/BackgroundMusic (the imported MP3).
/// To swap tracks, replace that file (keep the name) or change kClipResourcePath.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    private const string kClipResourcePath = "Audio/BackgroundMusic";
    private const float kVolume = 0.5f;   // 0..1 — adjust to taste

    private static BackgroundMusic sInstance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (sInstance != null) return;   // already created this session
        GameObject host = new GameObject("Background Music");
        host.AddComponent<BackgroundMusic>();
    }

    private void Awake()
    {
        // Singleton: keep the first player alive across scenes, destroy any duplicate.
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        sInstance = this;
        DontDestroyOnLoad(gameObject);

        AudioClip clip = Resources.Load<AudioClip>(kClipResourcePath);
        if (clip == null)
        {
            Debug.LogWarning("BackgroundMusic: no clip found at Resources/" + kClipResourcePath);
            return;
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = kVolume;
        source.spatialBlend = 0f;   // 2D, non-positional
        source.Play();
    }
}

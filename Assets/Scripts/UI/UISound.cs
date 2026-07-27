using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Persistent click sound for every UI button in the game.
///
/// Like BackgroundMusic, a single player is created automatically at application start
/// (RuntimeInitializeOnLoadMethod), survives every scene load (DontDestroyOnLoad), and needs
/// no per-scene setup and no Inspector wiring. The clip is loaded from
/// Resources/Audio/ButtonClick; to swap it, replace that file keeping the name.
///
/// Why this watches the pointer instead of subscribing to Button.onClick:
///   1. Not every button is styled. Only NextPhaseButton and ShowMissionButton pass through
///      GameUITheme.StyleButton; all four MainMenu buttons never do. So there is no single
///      creation point to hook.
///   2. onClick listeners get wiped. MissionPanelUI calls StyleButton and *then*
///      onClick.RemoveAllListeners(), which would silently delete any sound registered there.
/// Reading the pointer one level above the buttons sidesteps both, and automatically covers
/// scene buttons, runtime-generated overlay buttons, and any button added in future.
/// </summary>
public class UISound : MonoBehaviour
{
    private const string kClipResourcePath = "Audio/ButtonClick";
    private const float kVolume = 0.6f;   // 0..1 — adjust to taste

    private static UISound sInstance;

    private AudioClip mClip;
    private AudioSource mSource;

    // Reused between clicks so raycasting does not allocate a new list every time.
    private readonly List<RaycastResult> mHits = new List<RaycastResult>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (sInstance != null) return;   // already created this session
        GameObject host = new GameObject("UI Sound");
        host.AddComponent<UISound>();
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

        mClip = Resources.Load<AudioClip>(kClipResourcePath);
        if (mClip == null)
        {
            Debug.LogWarning("UISound: no clip found at Resources/" + kClipResourcePath);
            return;
        }

        mSource = gameObject.AddComponent<AudioSource>();
        mSource.playOnAwake = false;
        mSource.spatialBlend = 0f;   // 2D, non-positional
    }

    private void Update()
    {
        if (mClip == null || mSource == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        EventSystem events = EventSystem.current;
        if (events == null) return;   // a scene may legitimately have no EventSystem yet

        PointerEventData pointer = new PointerEventData(events) { position = Input.mousePosition };
        mHits.Clear();
        events.RaycastAll(pointer, mHits);
        if (mHits.Count == 0) return;

        // Results come back sorted front-to-back, so only the topmost graphic counts. A click
        // landing on a modal's dim backdrop finds no Button above it and stays silent, and a
        // hit on a button's label resolves up to the button, so one click is never two sounds.
        Button button = mHits[0].gameObject.GetComponentInParent<Button>();
        if (button == null) return;
        if (!button.IsInteractable()) return;   // also respects a parent CanvasGroup

        // PlayOneShot, not Play: overlapping clicks layer instead of cutting each other off,
        // and this source outlives buttons that destroy themselves when clicked (the mission
        // resolution buttons do exactly that).
        mSource.PlayOneShot(mClip, kVolume);
    }
}

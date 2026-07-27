using UnityEngine;

/// <summary>
/// Swaps the complete district-label image when the game language changes. Both the
/// ornament and localized name remain baked into the sprites, so no runtime text object
/// needs to overlap the map and the teammate-authored Label transform stays untouched.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class LocationLabelLocalizer : MonoBehaviour
{
    [SerializeField] private Sprite mEnglishSprite;
    [SerializeField] private Sprite mChineseSprite;

    private SpriteRenderer mRenderer;
    private ZebraGameController mController;

    public void Configure(Sprite englishSprite, Sprite chineseSprite)
    {
        mEnglishSprite = englishSprite;
        mChineseSprite = chineseSprite;
        Apply(GameSessionSettings.UseChinese);
    }

    private void Awake()
    {
        mRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        mController = FindFirstObjectByType<ZebraGameController>();
        if (mController != null)
        {
            mController.LanguageChanged += Apply;
        }

        // Controller.Start and label Start have no guaranteed relative order. The session
        // setting is already finalized before MainMap loads, so use it for the first frame;
        // subsequent in-game changes arrive through LanguageChanged.
        Apply(GameSessionSettings.UseChinese);
    }

    private void OnDestroy()
    {
        if (mController != null)
        {
            mController.LanguageChanged -= Apply;
        }
    }

    private void Apply(bool useChinese)
    {
        if (mRenderer == null) mRenderer = GetComponent<SpriteRenderer>();
        Sprite selected = useChinese ? mChineseSprite : mEnglishSprite;
        if (selected == null) selected = useChinese ? mEnglishSprite : mChineseSprite;
        mRenderer.sprite = selected;
        mRenderer.enabled = selected != null;
    }
}

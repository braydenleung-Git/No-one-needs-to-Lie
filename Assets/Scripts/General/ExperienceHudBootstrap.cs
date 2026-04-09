using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ensures <see cref="ExperienceManager"/> and a bottom-screen XP bar exist in play mode
/// (matches the XP test scene layout) so gameplay levels like Crime Scene show progress without
/// manually duplicating UI.
/// </summary>
public static class ExperienceHudBootstrap
{
    const string CanvasName = "XPHudCanvas";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureExperienceManager()
    {
        if (ExperienceManager.Instance != null) return;
        var go = new GameObject("ExperienceManager");
        go.AddComponent<ExperienceManager>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExperienceBar()
    {
        if (Object.FindFirstObjectByType<ExperienceBar>(FindObjectsInactive.Include) != null)
            return;

        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/ari_w9500(default)/ari-w9500 SDF");

        var canvasGO = new GameObject(CanvasName);
        Object.DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("XPBarBackground");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 0);
        panelRT.anchorMax = new Vector2(1, 0);
        panelRT.pivot = new Vector2(0.5f, 0f);
        panelRT.anchoredPosition = new Vector2(0, 10);
        panelRT.sizeDelta = new Vector2(-20, 28);

        panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        var fillGo = new GameObject("XPFill");
        fillGo.transform.SetParent(panel.transform, false);
        var fillRT = fillGo.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        var fillImg = fillGo.AddComponent<Image>();
        var white = Texture2D.whiteTexture;
        fillImg.sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height),
            new Vector2(0.5f, 0.5f), 100f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.color = new Color(0.13f, 0.77f, 0.37f, 1f);
        if (ExperienceManager.Instance != null)
        {
            fillImg.fillAmount = ExperienceManager.Instance.GetXPProgress();
        }

        var levelGo = new GameObject("LevelText");
        levelGo.transform.SetParent(panel.transform, false);
        var levelRT = levelGo.AddComponent<RectTransform>();
        levelRT.anchorMin = new Vector2(0, 0.5f);
        levelRT.anchorMax = new Vector2(0, 0.5f);
        levelRT.pivot = new Vector2(0, 0.5f);
        levelRT.anchoredPosition = new Vector2(8, 0);
        levelRT.sizeDelta = new Vector2(80, 28);
        var levelTmp = levelGo.AddComponent<TextMeshProUGUI>();
        levelTmp.text = "LVL 1";
        levelTmp.fontSize = 11;
        levelTmp.color = new Color(0.13f, 0.77f, 0.37f, 1f);
        if (font != null) levelTmp.font = font;

        var xpGo = new GameObject("XPText");
        xpGo.transform.SetParent(panel.transform, false);
        var xpRT = xpGo.AddComponent<RectTransform>();
        xpRT.anchorMin = new Vector2(1, 0.5f);
        xpRT.anchorMax = new Vector2(1, 0.5f);
        xpRT.pivot = new Vector2(1, 0.5f);
        xpRT.anchoredPosition = new Vector2(-8, 0);
        xpRT.sizeDelta = new Vector2(140, 28);
        var xpTmp = xpGo.AddComponent<TextMeshProUGUI>();
        if (ExperienceManager.Instance != null)
        {
            var xp = ExperienceManager.Instance;
            xpTmp.text = $"{xp.currentXP} / {xp.xpToNextLevel} XP";
        }
        else
            xpTmp.text = "20 / 100 XP";
        xpTmp.fontSize = 11;
        xpTmp.color = new Color(0.53f, 0.53f, 0.5f, 1f);
        xpTmp.alignment = TextAlignmentOptions.MidlineRight;
        if (font != null) xpTmp.font = font;

        var bar = panel.AddComponent<ExperienceBar>();
        bar.fillImage = fillImg;
        bar.levelText = levelTmp;
        bar.xpText = xpTmp;
    }
}

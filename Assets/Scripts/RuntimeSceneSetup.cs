using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to Main Camera. On Awake it builds the Grandpa NPC, DialogueCanvas,
/// PromptCanvas and EventSystem if they are not already in the scene.
/// Assign npcSprite in the Inspector.
/// </summary>
public class RuntimeSceneSetup : MonoBehaviour
{
    [Header("NPC Sprite")]
    public Sprite npcSprite;

    void Awake()
    {
        EnsurePlayerColliderAndSorting();

        if (GameObject.Find("Grandpa")        == null) CreateNPC();
        if (GameObject.Find("DialogueCanvas") == null) CreateDialogueCanvas();
        if (GameObject.Find("PromptCanvas")   == null) CreatePromptCanvas();
        if (FindObjectOfType<EventSystem>()   == null) CreateEventSystem();
    }

    // ── Player ────────────────────────────────────────────────────────────────

    void EnsurePlayerColliderAndSorting()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        if (player.GetComponent<Collider2D>() == null)
        {
            var col = player.AddComponent<CapsuleCollider2D>();
            col.size      = new Vector2(0.5f, 0.9f);
            col.isTrigger = false;
        }

        if (player.GetComponent<YSortingOrder>() == null)
            player.AddComponent<YSortingOrder>();
    }

    // ── Grandpa NPC ───────────────────────────────────────────────────────────

    void CreateNPC()
    {
        var npc = new GameObject("Grandpa");
        npc.transform.position   = new Vector3(3f, 0f, 0f);
        npc.transform.localScale = new Vector3(3f, 3f, 1f);

        var sr   = npc.AddComponent<SpriteRenderer>();
        sr.sprite = npcSprite;

        npc.AddComponent<Animator>();
        npc.AddComponent<YSortingOrder>();

        var col       = npc.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.8f;

        var ctrl          = npc.AddComponent<NPCController>();
        ctrl.npcName      = "Old Man";
        ctrl.dialogueLines = new[]
        {
            "Oh... a visitor. Haven't seen one of those in a while.",
            "They say the old manor has a secret. I wouldn't go snooping if I were you.",
            "...But what do I know? I'm just an old man."
        };
    }

    // ── Dialogue Canvas ───────────────────────────────────────────────────────

    void CreateDialogueCanvas()
    {
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // DialoguePanel — bottom strip
        var panel = MakePanel(canvasGO.transform, "DialoguePanel",
            new Vector2(0, 0),    new Vector2(1, 0.22f),
            new Vector2(30, 20),  new Vector2(-30, -10),
            new Color(0.08f, 0.08f, 0.08f, 0.88f));

        // Speaker name (golden, bold)
        var nameText = MakeText(panel.transform, "SpeakerNameText",
            new Vector2(0, 0.72f), new Vector2(0.45f, 1f),
            new Vector2(15, -8),   new Vector2(-5, -8),
            "Speaker", 20, new Color(1f, 0.85f, 0.2f), bold: true);

        // Dialogue body
        var bodyText = MakeText(panel.transform, "DialogueBodyText",
            Vector2.zero,         new Vector2(1f, 0.72f),
            new Vector2(15, 12),  new Vector2(-15, -5),
            "", 17, Color.white);

        // Continue prompt
        var contGO = new GameObject("ContinuePrompt");
        contGO.transform.SetParent(panel.transform, false);
        var cRT           = contGO.AddComponent<RectTransform>();
        cRT.anchorMin     = new Vector2(0.78f, 0f);
        cRT.anchorMax     = new Vector2(1f, 0.35f);
        cRT.offsetMin     = Vector2.zero;
        cRT.offsetMax     = new Vector2(-10f, 0f);
        var contTMP       = contGO.AddComponent<TextMeshProUGUI>();
        contTMP.text      = "▶  E";
        contTMP.fontSize  = 15;
        contTMP.color     = new Color(0.4f, 1f, 1f);
        contTMP.alignment = TextAlignmentOptions.BottomRight;

        // Wire up DialogueManager
        var dm = canvasGO.AddComponent<DialogueManager>();
        dm.Initialize(panel, nameText, bodyText, contGO, 0.04f);
    }

    // ── Prompt Canvas ─────────────────────────────────────────────────────────

    void CreatePromptCanvas()
    {
        var canvasGO = new GameObject("PromptCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Prompt pill
        var panel             = new GameObject("PromptPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT           = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta     = new Vector2(240f, 44f);
        panelRT.anchoredPosition = Vector2.zero;
        var img               = panel.AddComponent<Image>();
        img.color             = new Color(0f, 0f, 0f, 0.72f);

        var promptText = MakeText(panel.transform, "PromptText",
            Vector2.zero, Vector2.one,
            new Vector2(8, 4), new Vector2(-8, -4),
            "Press [E] to talk", 15, Color.white);
        promptText.alignment = TextAlignmentOptions.Center;

        var ipUI = canvasGO.AddComponent<InteractionPromptUI>();
        ipUI.Initialize(panel, promptText, 1.2f);
    }

    // ── EventSystem ───────────────────────────────────────────────────────────

    void CreateEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go          = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt          = go.AddComponent<RectTransform>();
        rt.anchorMin    = anchorMin;
        rt.anchorMax    = anchorMax;
        rt.offsetMin    = offsetMin;
        rt.offsetMax    = offsetMax;
        var img         = go.AddComponent<Image>();
        img.color       = color;
        return go;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        string text, float fontSize, Color color, bool bold = false)
    {
        var go          = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt          = go.AddComponent<RectTransform>();
        rt.anchorMin    = anchorMin;
        rt.anchorMax    = anchorMax;
        rt.offsetMin    = offsetMin;
        rt.offsetMax    = offsetMax;
        var tmp         = go.AddComponent<TextMeshProUGUI>();
        tmp.text        = text;
        tmp.fontSize    = fontSize;
        tmp.color       = color;
        tmp.enableWordWrapping = true;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }
}

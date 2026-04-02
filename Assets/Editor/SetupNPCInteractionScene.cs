using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility: Tools > Setup NPC Interaction Scene
/// Run this once while "Sunny Player" scene is open.
/// Creates: Grandpa NPC, DialogueCanvas, PromptCanvas, EventSystem.
/// </summary>
public static class SetupNPCInteractionScene
{
    [MenuItem("Tools/Setup NPC Interaction Scene")]
    public static void Setup()
    {
        EnsurePlayerCollider();
        CreateGrandpaNPC();
        GameObject dialogueCanvas = CreateDialogueCanvas();
        CreateInteractionPromptCanvas();
        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SetupNPCInteractionScene] Done. Save the scene (Ctrl+S) then press Play to test.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. GRANDPA NPC
    // ─────────────────────────────────────────────────────────────────────────

    static void CreateGrandpaNPC()
    {
        if (GameObject.Find("Grandpa") != null)
        {
            Debug.Log("[Setup] Grandpa already exists — skipping.");
            return;
        }

        // Load the first sub-sprite from the sprite sheet
        Sprite npcSprite = null;
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/PlayerArt/NPC's/Random_Grandpa_NPC.png");
        foreach (Object obj in allAssets)
        {
            if (obj is Sprite s && s.name == "Random_Grandpa_NPC_0")
            {
                npcSprite = s;
                break;
            }
        }

        GameObject npc = new GameObject("Grandpa");
        // Place the NPC 3 units to the right of origin, near the player
        npc.transform.position = new Vector3(3f, 0f, 0f);
        // Scale up so the sprite is visible at similar size to the player
        npc.transform.localScale = new Vector3(3f, 3f, 1f);

        // SpriteRenderer
        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        if (npcSprite != null)
            sr.sprite = npcSprite;
        else
            Debug.LogWarning("[Setup] Could not find Random_Grandpa_NPC_0 sprite. Assign it manually.");

        // Animator — required by [RequireComponent] on NPCController
        npc.AddComponent<Animator>();

        // CircleCollider2D as the interaction trigger zone
        CircleCollider2D col = npc.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;   // world units (pre-scale); after scale x3 = 2.4 unit radius

        // Y-sorting so player renders in front when below the NPC
        npc.AddComponent<YSortingOrder>();

        // NPCController (subclass of Interactable)
        NPCController npcCtrl = npc.AddComponent<NPCController>();
        npcCtrl.npcName = "Old Man";
        npcCtrl.dialogueLines = new[]
        {
            "Oh... a visitor. Haven't seen one of those in a while.",
            "They say the old manor has a secret. I wouldn't go snooping if I were you.",
            "...But what do I know? I'm just an old man."
        };

        // Set the inherited interactPrompt via SerializedObject (it's [SerializeField] protected)
        SerializedObject so = new SerializedObject(npcCtrl);
        so.FindProperty("interactPrompt").stringValue = "Press [E] to talk";
        so.ApplyModifiedProperties();

        Debug.Log("[Setup] Grandpa NPC created at (3, 0, 0).");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. DIALOGUE CANVAS
    // ─────────────────────────────────────────────────────────────────────────

    static GameObject CreateDialogueCanvas()
    {
        if (GameObject.Find("DialogueCanvas") != null)
        {
            Debug.Log("[Setup] DialogueCanvas already exists — skipping.");
            return GameObject.Find("DialogueCanvas");
        }

        // ── Root canvas ──────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("DialogueCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── DialoguePanel — anchored to bottom strip ─────────────────────────
        GameObject panelGO = CreateUIPanel(
            parent: canvasGO.transform,
            name: "DialoguePanel",
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0.22f),
            offsetMin: new Vector2(30f, 20f),
            offsetMax: new Vector2(-30f, -10f),
            color: new Color(0.08f, 0.08f, 0.08f, 0.88f)
        );

        // ── SpeakerNameText ──────────────────────────────────────────────────
        TextMeshProUGUI nameText = CreateTMProText(
            parent: panelGO.transform,
            name: "SpeakerNameText",
            anchorMin: new Vector2(0f, 0.72f),
            anchorMax: new Vector2(0.45f, 1f),
            offsetMin: new Vector2(15f, -8f),
            offsetMax: new Vector2(-5f, -8f),
            defaultText: "Speaker",
            fontSize: 20,
            bold: true,
            color: new Color(1f, 0.85f, 0.2f)   // golden yellow
        );

        // ── DialogueBodyText ─────────────────────────────────────────────────
        TextMeshProUGUI bodyText = CreateTMProText(
            parent: panelGO.transform,
            name: "DialogueBodyText",
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0.72f),
            offsetMin: new Vector2(15f, 12f),
            offsetMax: new Vector2(-15f, -5f),
            defaultText: "",
            fontSize: 17,
            bold: false,
            color: Color.white
        );

        // ── ContinuePrompt ───────────────────────────────────────────────────
        GameObject continueGO = new GameObject("ContinuePrompt");
        continueGO.transform.SetParent(panelGO.transform, false);
        RectTransform cRT = continueGO.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.78f, 0f);
        cRT.anchorMax = new Vector2(1f, 0.35f);
        cRT.offsetMin = Vector2.zero;
        cRT.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI continueText = continueGO.AddComponent<TextMeshProUGUI>();
        continueText.text = "▶  E";
        continueText.fontSize = 15;
        continueText.color = new Color(0.4f, 1f, 1f);
        continueText.alignment = TextAlignmentOptions.BottomRight;

        // ── DialogueManager on the canvas root ──────────────────────────────
        DialogueManager dm = canvasGO.AddComponent<DialogueManager>();
        SerializedObject dmSO = new SerializedObject(dm);
        dmSO.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
        dmSO.FindProperty("speakerNameText").objectReferenceValue = nameText;
        dmSO.FindProperty("dialogueBodyText").objectReferenceValue = bodyText;
        dmSO.FindProperty("continuePrompt").objectReferenceValue = continueGO;
        dmSO.FindProperty("charDelay").floatValue = 0.04f;
        dmSO.ApplyModifiedProperties();

        Debug.Log("[Setup] DialogueCanvas created and wired.");
        return canvasGO;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. INTERACTION PROMPT CANVAS
    // ─────────────────────────────────────────────────────────────────────────

    static void CreateInteractionPromptCanvas()
    {
        if (GameObject.Find("PromptCanvas") != null)
        {
            Debug.Log("[Setup] PromptCanvas already exists — skipping.");
            return;
        }

        // ── Root canvas ──────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("PromptCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── PromptPanel — small floating pill ────────────────────────────────
        GameObject panelGO = new GameObject("PromptPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(220f, 42f);
        panelRT.anchoredPosition = Vector2.zero;
        Image img = panelGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.72f);

        // ── PromptText ───────────────────────────────────────────────────────
        TextMeshProUGUI promptText = CreateTMProText(
            parent: panelGO.transform,
            name: "PromptText",
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            offsetMin: new Vector2(8f, 4f),
            offsetMax: new Vector2(-8f, -4f),
            defaultText: "Press [E] to talk",
            fontSize: 15,
            bold: false,
            color: Color.white
        );
        promptText.alignment = TextAlignmentOptions.Center;

        // ── InteractionPromptUI on canvas root ───────────────────────────────
        InteractionPromptUI ipUI = canvasGO.AddComponent<InteractionPromptUI>();
        SerializedObject so = new SerializedObject(ipUI);
        so.FindProperty("promptPanel").objectReferenceValue = panelGO;
        so.FindProperty("promptText").objectReferenceValue = promptText;
        so.FindProperty("verticalOffset").floatValue = 1.2f;
        so.ApplyModifiedProperties();

        panelGO.SetActive(false);

        Debug.Log("[Setup] PromptCanvas created and wired.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. PLAYER COLLIDER
    // ─────────────────────────────────────────────────────────────────────────

    static void EnsurePlayerCollider()
    {
        // Find the player by tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Setup] No GameObject tagged 'Player' found. Add a CapsuleCollider2D manually.");
            return;
        }

        // Only add if one doesn't already exist
        if (player.GetComponent<Collider2D>() != null)
        {
            Debug.Log("[Setup] Player already has a Collider2D — skipping.");
            return;
        }

        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        // Size matches the Detective sprite (0.7 x 1.13 world units)
        col.size = new Vector2(0.5f, 0.9f);
        col.offset = new Vector2(0f, 0f);
        col.isTrigger = false;   // solid collider so the NPC trigger can detect it

        // Y-sorting so player renders in front when below NPC
        if (player.GetComponent<YSortingOrder>() == null)
            player.AddComponent<YSortingOrder>();

        Debug.Log($"[Setup] Added CapsuleCollider2D and YSortingOrder to '{player.name}'.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. EVENT SYSTEM
    // ─────────────────────────────────────────────────────────────────────────

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Debug.Log("[Setup] EventSystem created.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI CreateTMProText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        string defaultText, float fontSize, bool bold, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }
}

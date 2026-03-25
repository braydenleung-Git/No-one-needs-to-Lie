using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to Main Camera in Sunny Player scene.
/// Builds Grandpa NPC, DialogueCanvas, PromptCanvas and EventSystem on Awake.
/// All objects are explicitly placed in the SAME scene as this script so that
/// 2D physics (OnTriggerEnter2D) works correctly even when multiple scenes are
/// open simultaneously in the editor.
/// </summary>
public class RuntimeSceneSetup : MonoBehaviour
{
    [Header("NPC Sprite")]
    public Sprite npcSprite;

    void Awake()
    {
        Scene myScene = gameObject.scene;

        EnsurePlayerColliderAndSorting(myScene);

        if (!FindInScene(myScene, "Grandpa"))
        {
            var go = CreateNPC();
            SceneManager.MoveGameObjectToScene(go, myScene);
        }

        if (!FindInScene(myScene, "DialogueCanvas"))
        {
            var go = CreateDialogueCanvas();
            SceneManager.MoveGameObjectToScene(go, myScene);
        }

        if (!FindInScene(myScene, "PromptCanvas"))
        {
            var go = CreatePromptCanvas();
            SceneManager.MoveGameObjectToScene(go, myScene);
        }

        if (!FindInScene(myScene, "EventSystem"))
        {
            var go = CreateEventSystem();
            SceneManager.MoveGameObjectToScene(go, myScene);
        }
    }

    // Returns true if a root GameObject with this name exists in the given scene
    static bool FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == name) return true;
        return false;
    }

    // ── Player ────────────────────────────────────────────────────────────────

    void EnsurePlayerColliderAndSorting(Scene scene)
    {
        // Only look inside the same scene to avoid touching other scenes' players
        foreach (var root in scene.GetRootGameObjects())
        {
            var player = root.CompareTag("Player") ? root
                       : root.GetComponentInChildren<Transform>() != null
                         ? FindTaggedInHierarchy(root, "Player")
                         : null;

            if (player == null) continue;

            if (player.GetComponent<Collider2D>() == null)
            {
                var col   = player.AddComponent<CapsuleCollider2D>();
                col.size  = new Vector2(0.5f, 0.9f);
                col.isTrigger = false;
            }

            if (player.GetComponent<YSortingOrder>() == null)
                player.AddComponent<YSortingOrder>();

            break;
        }
    }

    static GameObject FindTaggedInHierarchy(GameObject root, string tag)
    {
        if (root.CompareTag(tag)) return root;
        foreach (Transform child in root.transform)
        {
            var result = FindTaggedInHierarchy(child.gameObject, tag);
            if (result != null) return result;
        }
        return null;
    }

    // ── Grandpa NPC ───────────────────────────────────────────────────────────

    GameObject CreateNPC()
    {
        var npc = new GameObject("Grandpa");
        npc.transform.position   = new Vector3(3f, 0f, 0f);
        npc.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        var sr    = npc.AddComponent<SpriteRenderer>();
        sr.sprite = npcSprite;

        npc.AddComponent<Animator>();
        npc.AddComponent<YSortingOrder>();

        var col       = npc.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 3.0f;   // 3.0 × scale 0.4 = 1.2 world units

        var ctrl           = npc.AddComponent<NPCController>();
        ctrl.npcName       = "Old Man";
        ctrl.dialogueLines = new[]
        {
            "Oh... a visitor. Haven't seen one of those in a while.",
            "They say the old manor has a secret. I wouldn't go snooping if I were you.",
            "...But what do I know? I'm just an old man."
        };

        return npc;
    }

    // ── Dialogue Canvas ───────────────────────────────────────────────────────

    GameObject CreateDialogueCanvas()
    {
        var canvasGO = new GameObject("DialogueCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = MakePanel(canvasGO.transform, "DialoguePanel",
            new Vector2(0f, 0f),   new Vector2(1f, 0.22f),
            new Vector2(30f, 20f), new Vector2(-30f, -10f),
            new Color(0.08f, 0.08f, 0.08f, 0.88f));

        var nameText = MakeText(panel.transform, "SpeakerNameText",
            new Vector2(0f, 0.72f), new Vector2(0.45f, 1f),
            new Vector2(15f, -8f),  new Vector2(-5f, -8f),
            "Speaker", 20, new Color(1f, 0.85f, 0.2f), bold: true);

        var bodyText = MakeText(panel.transform, "DialogueBodyText",
            Vector2.zero,          new Vector2(1f, 0.72f),
            new Vector2(15f, 12f), new Vector2(-15f, -5f),
            "", 17, Color.white);

        var contGO        = new GameObject("ContinuePrompt");
        contGO.transform.SetParent(panel.transform, false);
        var cRT           = contGO.AddComponent<RectTransform>();
        cRT.anchorMin     = new Vector2(0.78f, 0f);
        cRT.anchorMax     = new Vector2(1f, 0.35f);
        cRT.offsetMin     = Vector2.zero;
        cRT.offsetMax     = new Vector2(-10f, 0f);
        var contTMP       = contGO.AddComponent<TextMeshProUGUI>();
        contTMP.text      = "[ E ]";        // avoid special unicode triangle
        contTMP.fontSize  = 15;
        contTMP.color     = new Color(0.4f, 1f, 1f);
        contTMP.alignment = TextAlignmentOptions.BottomRight;

        var dm = canvasGO.AddComponent<DialogueManager>();
        dm.Initialize(panel, nameText, bodyText, contGO, 0.04f);

        return canvasGO;
    }

    // ── Prompt Canvas ─────────────────────────────────────────────────────────

    GameObject CreatePromptCanvas()
    {
        var canvasGO = new GameObject("PromptCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel                = new GameObject("PromptPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT              = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta        = new Vector2(240f, 44f);
        panelRT.anchoredPosition = Vector2.zero;
        var img                  = panel.AddComponent<Image>();
        img.color                = new Color(0f, 0f, 0f, 0.72f);

        var promptText       = MakeText(panel.transform, "PromptText",
            Vector2.zero, Vector2.one,
            new Vector2(8f, 4f), new Vector2(-8f, -4f),
            "Press [E] to talk", 15, Color.white);
        promptText.alignment = TextAlignmentOptions.Center;

        var ipUI = canvasGO.AddComponent<InteractionPromptUI>();
        ipUI.Initialize(panel, promptText, 1.2f);

        return canvasGO;
    }

    // ── EventSystem ───────────────────────────────────────────────────────────

    GameObject CreateEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
        return es;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go       = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var img      = go.AddComponent<Image>();
        img.color    = color;
        return go;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        string text, float fontSize, Color color, bool bold = false)
    {
        var go       = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = color;
        tmp.enableWordWrapping = true;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }
}

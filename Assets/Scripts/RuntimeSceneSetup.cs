using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// attach this to the Main Camera in the Sunny Player scene
// it builds the grandpa NPC, dialogue box, prompt UI and event system at runtime
// so we don't have to manually recreate everything in the Unity inspector
//
// important: everything gets moved into the same scene as this script using
// SceneManager.MoveGameObjectToScene - had a bug where objects were landing in
// the wrong scene and 2D physics wouldn't fire across scene boundaries
public class RuntimeSceneSetup : MonoBehaviour
{
    [Header("NPC Sprite")]
    public Sprite npcSprite;

    void Awake()
    {
        Scene myScene = gameObject.scene;

        EnsurePlayerColliderAndSorting(myScene);

        // only create stuff that doesn't already exist
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

    // checks root objects in the scene by name, avoids searching other open scenes
    static bool FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == name) return true;
        return false;
    }

    // ── Player ────────────────────────────────────────────────────────────────

    void EnsurePlayerColliderAndSorting(Scene scene)
    {
        // scope the search to this scene only so we don't mess with other scenes' players
        foreach (var root in scene.GetRootGameObjects())
        {
            var player = root.CompareTag("Player") ? root
                       : root.GetComponentInChildren<Transform>() != null
                         ? FindTaggedInHierarchy(root, "Player")
                         : null;

            if (player == null) continue;

            // player needs a non-trigger collider or OnTriggerEnter2D never fires on the npc
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
        npc.transform.localScale = new Vector3(0.4f, 0.4f, 1f);  // scaled down, looks better

        var sr    = npc.AddComponent<SpriteRenderer>();
        sr.sprite = npcSprite;

        npc.AddComponent<Animator>();
        npc.AddComponent<YSortingOrder>();

        // trigger zone - radius 3.0 at scale 0.4 = 1.2 world units, feels natural
        var col       = npc.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 3.0f;

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

        // dark panel at the bottom of the screen for dialogue text
        var panel = MakePanel(canvasGO.transform, "DialoguePanel",
            new Vector2(0f, 0f),   new Vector2(1f, 0.22f),
            new Vector2(30f, 20f), new Vector2(-30f, -10f),
            new Color(0.08f, 0.08f, 0.08f, 0.88f));

        var nameText = MakeText(panel.transform, "SpeakerNameText",
            new Vector2(0f, 0.72f), new Vector2(0.45f, 1f),
            new Vector2(15f, -8f),  new Vector2(-5f, -8f),
            "Speaker", 32, new Color(1f, 0.85f, 0.2f), bold: true);

        var bodyText = MakeText(panel.transform, "DialogueBodyText",
            Vector2.zero,          new Vector2(1f, 0.72f),
            new Vector2(15f, 12f), new Vector2(-15f, -5f),
            "", 28, Color.white);

        // little "[ E ]" hint at the bottom right so the player knows to press E
        var contGO        = new GameObject("ContinuePrompt");
        contGO.transform.SetParent(panel.transform, false);
        var cRT           = contGO.AddComponent<RectTransform>();
        cRT.anchorMin     = new Vector2(0.78f, 0f);
        cRT.anchorMax     = new Vector2(1f, 0.35f);
        cRT.offsetMin     = Vector2.zero;
        cRT.offsetMax     = new Vector2(-10f, 0f);
        var contTMP       = contGO.AddComponent<TextMeshProUGUI>();
        contTMP.text      = "[ E ]";        // unicode triangle caused a font warning so using this instead
        contTMP.fontSize  = 24;
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
        canvas.sortingOrder = 11;  // above dialogue canvas
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
            "Press [E] to talk", 24, Color.white);
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

        // DON'T use StandaloneInputModule here - that's the old input system's UI handler
        // and it calls Input.mousePosition every single frame which throws a hard exception
        // the moment you switch Player Settings to "New Input System" package
        // InputSystemUIInputModule is the correct replacement from com.unity.inputsystem
        es.AddComponent<InputSystemUIInputModule>();
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

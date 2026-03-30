using UnityEngine;
using UnityEngine.SceneManagement;

// attach to the Main Camera in the Crime Scene level alongside RuntimeSceneSetup
// builds all the level-specific stuff: background, walls, cassette tape, cassette player, witness NPC
// runs after RuntimeSceneSetup (which handles player collider, dialogue canvas, prompt canvas, event system)
//
// Cassette sprites load automatically from Assets/Resources/CassetteSheet.png
// (Sprite Mode: Multiple, sliced into CassettePlayer left half and CassetteTape right half).
// Inspector fields are optional overrides — leave empty and Resources.Load handles it.
[ExecuteAlways]
[DefaultExecutionOrder(10)]
public class Level2SceneBuilder : MonoBehaviour
{
    // Scene_Lvl2.png - assign in inspector on the Main Camera
    public Sprite backgroundSprite;

    // witness NPC sprite - assign in inspector
    public Sprite witnessSprite;

    // optional inspector overrides; auto-loaded from Resources/CassetteSheet if left empty
    public Sprite cassetteTapeSprite;
    public Sprite cassettePlayerSprite;

    const string GeneratedRootName = "__Level2_Generated";

    private void Awake()
    {
        // In the editor we also want to see the level art/walls without pressing Play.
        // Awake runs both in play mode and (because of ExecuteAlways) in edit mode.
        EnsureBuilt(gameObject.scene, isEditorPreview: !Application.isPlaying);

        if (!Application.isPlaying)
            return;

        LoadCassetteSpritesIfNeeded();

        PuzzleState.Reset();

        Scene myScene = gameObject.scene;
        var tapeGO    = CreateCassetteTape(myScene);
        var playerGO  = CreateCassettePlayer(myScene);
        var witness   = CreateWitnessNPC(myScene);

        // wire the cassette player to the witness NPC so it activates after playback
        var cassetteComp = playerGO.GetComponent<CassettePlayerInteractable>();
        if (cassetteComp != null)
            cassetteComp.witnessNPC = witness.GetComponent<NPCController>();
    }

    void OnEnable()
    {
        // If inspector values change (sprite reassigned), keep the preview updated.
        EnsureBuilt(gameObject.scene, isEditorPreview: !Application.isPlaying);
    }

    void OnValidate()
    {
        EnsureBuilt(gameObject.scene, isEditorPreview: !Application.isPlaying);
    }

    void EnsureBuilt(Scene scene, bool isEditorPreview)
    {
        if (!scene.IsValid())
            return;

        // Keep edit-time preview minimal: only background + walls (no puzzle objects/NPC).
        // In play mode, we also want background + walls, but we avoid duplicates.
        var root = FindOrCreateRoot(scene);

        var bgParent = FindOrCreateChild(root, "BackgroundRoot");
        var wallsParent = FindOrCreateChild(root, "WallsRoot");

        // Rebuild children deterministically (prevents duplicates on domain reload).
        ClearChildren(bgParent);
        ClearChildren(wallsParent);

        CreateBackground(scene, bgParent.transform);
        CreateWalls(scene, wallsParent.transform);

        FrameCameraToBackground();

#if UNITY_EDITOR
        if (isEditorPreview)
        {
            // Avoid cluttering the hierarchy; still saves with the scene so it shows immediately.
            root.hideFlags = HideFlags.None;
        }
#endif
    }

    GameObject FindOrCreateRoot(Scene scene)
    {
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == GeneratedRootName)
                return r;

        var go = new GameObject(GeneratedRootName);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    static GameObject FindOrCreateChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void ClearChildren(GameObject parent)
    {
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            var child = parent.transform.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child);
            else
                Destroy(child);
#else
            Destroy(child);
#endif
        }
    }

    void LoadCassetteSpritesIfNeeded()
    {
        if (cassetteTapeSprite != null && cassettePlayerSprite != null) return;

        Sprite[] sheet = Resources.LoadAll<Sprite>("CassetteSheet");
        if (sheet == null || sheet.Length == 0)
        {
            Debug.LogWarning("Level2SceneBuilder: Could not load CassetteSheet from Resources. " +
                             "Ensure Assets/Resources/CassetteSheet.png exists with Sprite Mode: Multiple.");
            return;
        }

        foreach (var s in sheet)
        {
            if (cassetteTapeSprite   == null && s.name == "CassetteTape")   cassetteTapeSprite   = s;
            if (cassettePlayerSprite == null && s.name == "CassettePlayer") cassettePlayerSprite = s;
        }
    }

    // ── Background ────────────────────────────────────────────────────────────

    void CreateBackground(Scene scene, Transform parent)
    {
        var go = new GameObject("Background");
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = backgroundSprite;
        sr.sortingOrder = -10;
        // The imported level sprite is currently using a bottom-left pivot (see Scene_Lvl2.png.meta).
        // Our wall coordinates assume the artwork is centered at world (0,0), so we auto-center it here.
        // If the sprite pivot is later changed to center, backgroundSprite.bounds.center becomes (0,0)
        // and this becomes a no-op.
        if (backgroundSprite != null)
            go.transform.position = -backgroundSprite.bounds.center;
        else
            go.transform.position = Vector3.zero;
        // Parent is already in the correct scene; moving would throw because this is not a root object.
    }

    // ── Walls ─────────────────────────────────────────────────────────────────

    GameObject CreateWalls(Scene scene, Transform parent)
    {
        var walls = new GameObject("Walls");
        walls.transform.SetParent(parent, false);
        // Parent is already in the correct scene; moving would throw because this is not a root object.

        // outer walls
        MakeWall(walls, "Wall_Top",            new Vector2(0f,     3.22f),  new Vector2(13.92f, 0.3f));
        MakeWall(walls, "Wall_Left",           new Vector2(-6.96f, 0f),     new Vector2(0.3f,   6.44f));
        MakeWall(walls, "Wall_Bottom_Left",    new Vector2(-3.5f,  -3.22f), new Vector2(7.0f,   0.3f));
        MakeWall(walls, "Wall_Bottom_Right",   new Vector2(4.5f,   -3.22f), new Vector2(5.0f,   0.3f));
        MakeWall(walls, "Wall_Right_Top",      new Vector2(6.96f,  1.6f),   new Vector2(0.3f,   3.2f));
        MakeWall(walls, "Wall_Right_Bottom",   new Vector2(6.96f,  -2.4f),  new Vector2(0.3f,   0.9f));

        // L-shape cutout edges
        MakeWall(walls, "Wall_LCutout_H",      new Vector2(3.5f,   -1.6f),  new Vector2(6.9f,   0.3f));
        MakeWall(walls, "Wall_LCutout_V",      new Vector2(0.05f,  -2.4f),  new Vector2(0.3f,   1.7f));

        // interior room dividers
        MakeWall(walls, "Wall_KitchenDivider", new Vector2(-2.8f,  0.5f),   new Vector2(0.3f,   4.5f));
        MakeWall(walls, "Wall_LivingBedroom",  new Vector2(1.4f,   1.6f),   new Vector2(0.3f,   3.3f));
        MakeWall(walls, "Wall_Bedroom1_Floor", new Vector2(4.2f,   0.7f),   new Vector2(5.5f,   0.3f));
        MakeWall(walls, "Wall_Bedroom2_Floor", new Vector2(4.2f,   -0.5f),  new Vector2(5.5f,   0.3f));
        MakeWall(walls, "Wall_Entrance_Hall",  new Vector2(0f,     -1.8f),  new Vector2(2.8f,   0.3f));

        return walls;
    }

    void MakeWall(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;

        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = size;
        col.isTrigger = false;

        go.AddComponent<MapBoundary>();
    }

    void FrameCameraToBackground()
    {
        var cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic || backgroundSprite == null)
            return;

        // Keep camera centered on the level.
        var t = cam.transform;
        t.position = new Vector3(0f, 0f, t.position.z);

        // Fit the whole sprite in view (with a small margin).
        float halfH = backgroundSprite.bounds.extents.y;
        float halfW = backgroundSprite.bounds.extents.x;
        float aspect = cam.aspect <= 0f ? (16f / 9f) : cam.aspect;
        float sizeToFitW = halfW / aspect;
        cam.orthographicSize = Mathf.Max(halfH, sizeToFitW) + 0.25f;
    }

    // ── Cassette Tape ─────────────────────────────────────────────────────────
    // Kitchen (left room, x ≈ -4.5) — player finds this first.
    // The sprite is ~687 px wide; at 100 PPU that's 6.87 world units.
    // We scale the renderer child down to ~0.25 wu and keep the collider
    // on the parent at full world-unit scale so the player can actually trigger it.

    GameObject CreateCassetteTape(Scene scene)
    {
        // parent: holds the trigger collider at world scale (no scaling!)
        var go = new GameObject("CassetteTape");
        go.transform.position = new Vector3(-4.5f, 0.8f, 0f);

        // trigger zone — 1.2 world units wide so the player can walk near it
        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1.2f, 1.0f);
        col.isTrigger = true;

        go.AddComponent<YSortingOrder>();
        go.AddComponent<CassetteTapePickup>();

        // child: the visible sprite, scaled down to fit the room
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        if (cassetteTapeSprite != null)
        {
            sr.sprite = cassetteTapeSprite;
            // 687 px / 100 PPU = 6.87 wu → scale to ~0.25 wu wide
            visual.transform.localScale = new Vector3(0.036f, 0.036f, 1f);
        }
        else
        {
            sr.sprite = CreatePlaceholderSprite(Color.yellow);
            visual.transform.localScale = new Vector3(1.0f, 0.8f, 1f);
        }

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Cassette Player ───────────────────────────────────────────────────────
    // Living room (centre, x ≈ -0.3) — different room from the tape.
    // CassettePlayerInteractable blocks use until the tape is in inventory.

    GameObject CreateCassettePlayer(Scene scene)
    {
        // parent: holds the trigger collider at world scale
        var go = new GameObject("CassettePlayer");
        go.transform.position = new Vector3(-0.3f, 0.4f, 0f);

        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1.2f, 1.0f);
        col.isTrigger = true;

        go.AddComponent<CassettePlayerInteractable>();

        // child: the visible sprite
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        if (cassettePlayerSprite != null)
        {
            sr.sprite = cassettePlayerSprite;
            // 700 px / 100 PPU = 7.0 wu → scale to ~0.28 wu wide
            visual.transform.localScale = new Vector3(0.04f, 0.04f, 1f);
        }
        else
        {
            sr.sprite = CreatePlaceholderSprite(new Color(0.25f, 0.25f, 0.25f));
            visual.transform.localScale = new Vector3(1.0f, 0.8f, 1f);
        }

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Witness NPC ───────────────────────────────────────────────────────────

    GameObject CreateWitnessNPC(Scene scene)
    {
        var go = new GameObject("Witness");
        go.transform.position   = new Vector3(0.5f, 0.8f, 0f);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = witnessSprite;
        sr.sortingOrder = 2;

        go.AddComponent<Animator>();
        go.AddComponent<YSortingOrder>();

        var col       = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 3.0f; // 3.0 * scale 0.4 = 1.2 world units interaction range

        var npc           = go.AddComponent<NPCController>();
        npc.npcName       = "Witness";
        npc.dialogueLines = new[]
        {
            "Oh, you found the tape! I recorded that the night it happened.",
            "I saw someone leave the art room around 1 AM. I didn't recognise them.",
            "They were carrying something wrapped in cloth. I should have called someone.",
            "Whatever happened here... it started in that room. Be careful."
        };

        // hidden until CassettePlayerInteractable.OnRecordingComplete fires
        go.SetActive(false);

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    static Sprite CreatePlaceholderSprite(Color color)
    {
        var tex    = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}

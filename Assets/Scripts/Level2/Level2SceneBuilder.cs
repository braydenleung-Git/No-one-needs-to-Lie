using UnityEngine;
using UnityEngine.SceneManagement;

// attach to the Main Camera in the Crime Scene level alongside RuntimeSceneSetup
// builds all the level-specific stuff: background, walls, cassette tape, cassette player, witness NPC
// runs after RuntimeSceneSetup (which handles player collider, dialogue canvas, prompt canvas, event system)
//
// Cassette sprites load automatically from Assets/Resources/CassetteSheet.png
// (Sprite Mode: Multiple, sliced into CassettePlayer left half and CassetteTape right half).
// Inspector fields are optional overrides — leave empty and Resources.Load handles it.
//
// [ExecuteAlways] so the background and walls show up in edit mode without pressing play
// everything gets parked under a single root "__Level2_Generated" so it's easy to find in the hierarchy
// and we don't litter the scene with a bunch of loose GameObjects
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

    [Tooltip("When false, existing colliders under WallsRoot are left alone so manual moves in the editor survive Play mode. Turn on again to re-apply coded layout.")]
    public bool regenerateWallsFromLayout = true;

    const string GeneratedRootName = "__Level2_Generated";
    bool _rebuildScheduled;

    private void Awake()
    {
        // In the editor we also want to see the level art/walls without pressing Play.
        // Awake runs both in play mode and (because of ExecuteAlways) in edit mode.
        RequestRebuild();

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
        RequestRebuild();
    }

    void OnValidate()
    {
        RequestRebuild();
    }

    void RequestRebuild()
    {
        // in play mode just rebuild straight away, no drama
        if (Application.isPlaying)
        {
            EnsureBuilt(gameObject.scene, isEditorPreview: false);
            return;
        }

#if UNITY_EDITOR
        // in edit mode we can't do heavy GameObject work directly inside OnValidate
        // because Unity might call it before the scene is fully loaded and things explode
        // delayCall defers it to the next editor frame when everything is safe to touch
        if (_rebuildScheduled) return;  // already queued, don't stack up duplicate calls
        _rebuildScheduled = true;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            _rebuildScheduled = false;
            if (this == null) return;  // component might have been destroyed while we waited
            EnsureBuilt(gameObject.scene, isEditorPreview: true);
        };
#endif
    }

    void EnsureBuilt(Scene scene, bool isEditorPreview)
    {
        if (!scene.IsValid())
            return;
        if (!scene.isLoaded)
            return;

        // Keep edit-time preview minimal: only background + walls (no puzzle objects/NPC).
        // In play mode, we also want background + walls, but we avoid duplicates.
        var root = FindOrCreateRoot(scene);

        var bgParent = FindOrCreateChild(root, "BackgroundRoot");
        var wallsParent = FindOrCreateChild(root, "WallsRoot");

        ClearChildren(bgParent);
        CreateBackground(scene, bgParent.transform);

        if (regenerateWallsFromLayout)
        {
            ClearChildren(wallsParent);
            CreateWalls(scene, wallsParent.transform);
        }

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
        // if both are already wired in the inspector we don't need to do anything
        if (cassetteTapeSprite != null && cassettePlayerSprite != null) return;

        // CassetteSheet.png lives in Assets/Resources/ so we can load it at runtime
        // without needing an asset reference in the inspector
        // Resources.LoadAll returns ALL sub-sprites when Sprite Mode is Multiple
        Sprite[] sheet = Resources.LoadAll<Sprite>("CassetteSheet");
        if (sheet == null || sheet.Length == 0)
        {
            Debug.LogWarning("Level2SceneBuilder: Could not load CassetteSheet from Resources. " +
                             "Make sure Assets/Resources/CassetteSheet.png exists and is set to Sprite Mode: Multiple.");
            return;
        }

        // match by name - names come from the sprite sheet slice definitions in the .meta file
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
        // Keep the background far behind anything that uses YSortingOrder.
        sr.sortingOrder = -1000;
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

        // Sizes come from the current background sprite so cropping/reimport does not drift colliders.
        if (backgroundSprite == null)
            return walls;

        float halfW = backgroundSprite.bounds.extents.x;
        float halfH = backgroundSprite.bounds.extents.y;
        float t = 0.22f;         // collider thickness (thin = forgiving for doorways)
        float door = 1.05f;      // half-width of entrance gap at bottom center (total gap = 2*door)
        float yTop = halfH - t * 0.5f;
        float yBot = -halfH + t * 0.5f;

        void VSeg(string nm, float x, float yLo, float yHi)
        {
            float lo = Mathf.Min(yLo, yHi);
            float hi = Mathf.Max(yLo, yHi);
            float h = hi - lo;
            if (h < 0.08f) return;
            float cy = (lo + hi) * 0.5f;
            MakeWall(walls, nm, new Vector2(x, cy), new Vector2(t, h));
        }

        void HSeg(string nm, float y, float xLo, float xHi)
        {
            float lo = Mathf.Min(xLo, xHi);
            float hi = Mathf.Max(xLo, xHi);
            float w = hi - lo;
            if (w < 0.08f) return;
            float cx = (lo + hi) * 0.5f;
            MakeWall(walls, nm, new Vector2(cx, y), new Vector2(w, t));
        }

        // --- Outer shell (entrance gap at bottom center) ---
        MakeWall(walls, "Wall_Outer_Top", new Vector2(0f, halfH), new Vector2(2f * halfW, t));
        MakeWall(walls, "Wall_Outer_Left", new Vector2(-halfW, 0f), new Vector2(t, 2f * halfH));

        float bottomY = -halfH;
        float leftSegW  = halfW - door;
        float rightSegW = halfW - door;
        MakeWall(walls, "Wall_Outer_Bot_L", new Vector2(-(halfW + door) * 0.5f, bottomY), new Vector2(leftSegW, t));
        MakeWall(walls, "Wall_Outer_Bot_R", new Vector2((halfW + door) * 0.5f,  bottomY), new Vector2(rightSegW, t));

        // Right side L-shape (tall upper + short lower leg for bottom-right jog)
        float xR = halfW - t * 0.5f;
        VSeg("Wall_Outer_Right_Tall", xR, 0.35f, yTop);
        float yRightLowTop = yBot + 1.08f; // meet small interior jog, stay in bottom strip
        VSeg("Wall_Outer_Right_Low",  xR, yBot, yRightLowTop);

// Bottom-right “step” only — was a full-width bar before and cut through the living room
        float jogY = yBot + 0.98f;
        HSeg("Wall_LNotch_H", jogY, 0.40f * halfW, xR - t);
        VSeg("Wall_LNotch_V", 0.40f * halfW, yBot + 0.65f, jogY + t * 0.5f);

        // --- Kitchen | living (vertical) — wider doorway, wall closer to art divider ---
        float xKL = -0.30f * halfW;
        VSeg("Wall_Kitchen_Liv_Up",   xKL, 0.82f, yTop);
        VSeg("Wall_Kitchen_Liv_Down", xKL, yBot, -0.62f);

        // --- Living | hallway (vertical) ---
        float xLH = 0.26f * halfW;
        VSeg("Wall_Liv_Hall_Up",   xLH, 1.18f, yTop);
        VSeg("Wall_Liv_Hall_Down", xLH, yBot, 0.08f);

        // --- Hallway | bedrooms (vertical) — nudge for door strips in the art ---
        float xHB = 0.54f * halfW;
        VSeg("Wall_Hall_Bed_Top",    xHB, 1.18f, yTop);
        VSeg("Wall_Hall_Bed_Mid",    xHB, 0.02f, 0.88f);
        VSeg("Wall_Hall_Bed_Bottom", xHB, yBot, -0.88f);

        // --- Bedroom stack: horizontals only east of the hall–bedroom divider ---
        float xBedL = xHB + t * 0.85f;
        float xBedR = xR - t * 0.65f;
        HSeg("Wall_Bed12_H",  0.50f, xBedL, xBedR);
        HSeg("Wall_Bed23_H", -0.48f, xBedL, xBedR);

        // --- Foyer band: sit just above outer bottom wall, leave wide opening into living ---
        float foyerY = yBot + 0.88f;
        HSeg("Wall_Foyer_L", foyerY, -halfW + 0.4f, -1.0f);
        HSeg("Wall_Foyer_R", foyerY, 1.45f, xHB - 0.35f);

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
    // sits on the kitchen floor (left room, x ≈ -4.5) — first thing the player finds
    // the sprite is ~687 px wide at 100 PPU = 6.87 world units native, way too big
    // so we put the SpriteRenderer on a child object and scale THAT down
    // the parent keeps the trigger collider at normal world-space size so the player can actually walk near it
    // (if you scale the parent, the collider shrinks with it and becomes a tiny dot that's impossible to hit)

    GameObject CreateCassetteTape(Scene scene)
    {
        // parent: no scale transform, owns the trigger and the Interactable component
        var go = new GameObject("CassetteTape");
        go.transform.position = new Vector3(-4.5f, 0.8f, 0f);

        // 1.2 × 1.0 wu trigger so the player doesn't have to pixel-perfectly step on it
        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1.2f, 1.0f);
        col.isTrigger = true;

        go.AddComponent<YSortingOrder>();
        go.AddComponent<CassetteTapePickup>();

        // child: just the renderer, scaled down so the pixel art looks right on screen
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        if (cassetteTapeSprite != null)
        {
            sr.sprite = cassetteTapeSprite;
            // 687 px / 100 PPU = 6.87 wu native → 6.87 * 0.036 ≈ 0.25 wu on screen, looks about right
            visual.transform.localScale = new Vector3(0.036f, 0.036f, 1f);
        }
        else
        {
            // yellow square placeholder if the sprite sheet hasn't been imported yet
            sr.sprite = CreatePlaceholderSprite(Color.yellow);
            visual.transform.localScale = new Vector3(1.0f, 0.8f, 1f);
        }

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Cassette Player ───────────────────────────────────────────────────────
    // sits on the table in the living room (centre, x ≈ -0.3) — different room from the tape
    // the player has to find the tape in the kitchen first before this does anything
    // CassettePlayerInteractable handles that gate check, this script just builds the object

    GameObject CreateCassettePlayer(Scene scene)
    {
        // same parent/child split as CassetteTape — collider on parent, sprite on child
        var go = new GameObject("CassettePlayer");
        go.transform.position = new Vector3(-0.3f, 0.4f, 0f);

        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1.2f, 1.0f);
        col.isTrigger = true;

        go.AddComponent<CassettePlayerInteractable>();

        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        if (cassettePlayerSprite != null)
        {
            sr.sprite = cassettePlayerSprite;
            // 700 px / 100 PPU = 7.0 wu native → 7.0 * 0.04 = 0.28 wu on screen
            visual.transform.localScale = new Vector3(0.04f, 0.04f, 1f);
        }
        else
        {
            // dark gray placeholder
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

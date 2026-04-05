using System;
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

    // witness NPC — prefer Owner prefab; else built from sprite
    public GameObject witnessPrefab;
    public Sprite witnessSprite;

    // optional inspector overrides; auto-loaded from Resources/CassetteSheet if left empty
    public Sprite cassetteTapeSprite;
    public Sprite cassettePlayerSprite;

    [Tooltip("When true, replaces all wall colliders from code (wipes manual edits). Set false to hand-tune positions in the scene; turn on once to regenerate defaults.")]
    public bool regenerateWallsFromLayout = false;

    [Tooltip("When true, replaces painting/safe triggers from code. When false, your moved colliders survive Play mode. Empty folder still gets defaults once.")]
    public bool regeneratePaintingTriggersFromLayout = false;

    const string GeneratedRootName = "__Level2_Generated";
    bool _rebuildScheduled;

    private void Awake()
    {
        // In the editor we also want to see the level art/walls without pressing Play.
        // Awake runs both in play mode and (because of ExecuteAlways) in edit mode.
        RequestRebuild();

        if (!Application.isPlaying)
            return;

#if UNITY_EDITOR
        if (witnessPrefab == null)
            witnessPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Owner.prefab");
#endif

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

        // Background always syncs to the assigned sprite. Walls / painting triggers can be
        // regenerated from code or hand-edited under __Level2_Generated (see inspector flags).
        var root = FindOrCreateRoot(scene);

        var bgParent = FindOrCreateChild(root, "BackgroundRoot");
        var wallsParent = FindOrCreateChild(root, "WallsRoot");
        var paintingParent = FindOrCreateChild(root, "PaintingPuzzleTriggers");

        ClearChildren(bgParent);
        CreateBackground(scene, bgParent.transform);

        if (regenerateWallsFromLayout)
        {
            ClearChildren(wallsParent);
            CreateWalls(scene, wallsParent.transform);
        }

        if (regeneratePaintingTriggersFromLayout)
        {
            ClearChildren(paintingParent);
            CreatePaintingPuzzleAndSafe(scene, paintingParent.transform);
        }
        else if (paintingParent.transform.childCount == 0)
        {
            CreatePaintingPuzzleAndSafe(scene, paintingParent.transform);
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

        var tapeItem = go.AddComponent<Item>();
        tapeItem.ItemName   = "Cassette Tape";
        tapeItem.Description = "A small cassette. It might fit the player in the living room.";

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
        GameObject go;

        if (witnessPrefab != null)
        {
            go = Instantiate(witnessPrefab);
            go.name = "Witness";
            go.transform.position   = new Vector3(0.35f, 0.05f, 0f);
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            NpcPrefabUtility.ConfigureOwnerPrefabForDialogueNpc(go);

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 2;
        }
        else
        {
            go = new GameObject("Witness");
            go.transform.position   = new Vector3(0.35f, 0.05f, 0f);
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            var srNew          = go.AddComponent<SpriteRenderer>();
            srNew.sprite       = witnessSprite;
            srNew.sortingOrder = 2;

            go.AddComponent<Animator>();
            go.AddComponent<YSortingOrder>();

            var col       = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius    = 3.0f;
        }

        var npc           = go.AddComponent<NPCController>();
        npc.npcName       = "Witness";
        npc.dialogueLines = new[]
        {
            "Oh, you found the tape! I recorded that the night it happened.",
            "I saw someone leave the art room around 1 AM. I didn't recognise them.",
            "They were carrying something wrapped in cloth. I should have called someone.",
            "Whatever happened here... it started in that room. Be careful."
        };

        go.SetActive(false);

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Painting cipher + art room safe ─────────────────────────────────────
    // Four interactables under CipherPaintings_LUCY spell L-U-C-Y by position (1→4). Decorative frames are optional.
    // ArtRoom_Safe uses ArtRoomSafeInteractable: type PuzzleState.ArtRoomCodeSolution ("LUCY") + Enter.

    void CreatePaintingPuzzleAndSafe(Scene scene, Transform paintingPuzzleRoot)
    {
        var cipherRoot = new GameObject("CipherPaintings_LUCY");
        cipherRoot.transform.SetParent(paintingPuzzleRoot, false);

        var decorRoot = new GameObject("DecorativePaintings");
        decorRoot.transform.SetParent(paintingPuzzleRoot, false);

        void AddPainting(Transform parent, string name, Vector3 pos, Vector2 size, System.Action<PaintingClueInteractable> setup)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
            var clue = go.AddComponent<PaintingClueInteractable>();
            setup?.Invoke(clue);
            // Do not call SceneManager.MoveGameObjectToScene on children — Unity only moves root objects.
            // Parent is already in the target scene, so this object inherits the correct scene.
        }

        // ── 4 cipher boxes → anagram orders to LUCY (position index = letter slot in the name)
        AddPainting(cipherRoot.transform, "Cipher_01_Pos1_L_Seascape", new Vector3(0.62f, 1.58f, 0f), new Vector2(0.5f, 0.45f), c =>
        {
            c.paintingTitle = "Seascape";
            c.isCipherClue = true;
            c.clueLetter = 'L';
            c.positionIndex = 1;
            c.countedObjectSingular = "boat";
            c.countedObjectPlural = "boats";
        });

        AddPainting(cipherRoot.transform, "Cipher_02_Pos2_U_Cabin", new Vector3(-1.12f, 1.58f, 0f), new Vector2(0.5f, 0.45f), c =>
        {
            c.paintingTitle = "Landscape (cabin)";
            c.isCipherClue = true;
            c.clueLetter = 'U';
            c.positionIndex = 2;
            c.countedObjectSingular = "chimney stack";
            c.countedObjectPlural = "chimney stacks";
        });

        AddPainting(cipherRoot.transform, "Cipher_03_Pos3_C_Skyline", new Vector3(5.18f, 1.92f, 0f), new Vector2(0.65f, 0.4f), c =>
        {
            c.paintingTitle = "City skyline";
            c.isCipherClue = true;
            c.clueLetter = 'C';
            c.positionIndex = 3;
            c.countedObjectSingular = "lit tower";
            c.countedObjectPlural = "lit towers";
        });

        AddPainting(cipherRoot.transform, "Cipher_04_Pos4_Y_Map", new Vector3(4.92f, 0.35f, 0f), new Vector2(0.6f, 0.5f), c =>
        {
            c.paintingTitle = "World map";
            c.isCipherClue = true;
            c.clueLetter = 'Y';
            c.positionIndex = 4;
            c.countedObjectSingular = "compass tick";
            c.countedObjectPlural = "compass ticks";
        });

        // Optional décor (not part of the four-letter code)
        AddPainting(decorRoot.transform, "Decor_Kitchen_Family", new Vector3(-3.82f, 1.76f, 0f), new Vector2(0.55f, 0.45f), c =>
        {
            c.paintingTitle = "Family portrait";
            c.isCipherClue = false;
            c.flavorLinesNoCipher = new[]
            {
                "A posed family dinner. Warm, ordinary — no obvious code."
            };
        });

        AddPainting(decorRoot.transform, "Decor_Art_Botanical", new Vector3(5.12f, -1.72f, 0f), new Vector2(0.45f, 0.65f), c =>
        {
            c.paintingTitle = "Botanical chart";
            c.isCipherClue = false;
            c.flavorLinesNoCipher = new[]
            {
                "Pressed flowers and Latin names. Pretty, but not part of the frame cipher."
            };
        });

        AddPainting(decorRoot.transform, "Decor_Art_Easel", new Vector3(6.05f, -2.55f, 0f), new Vector2(0.5f, 0.55f), c =>
        {
            c.paintingTitle = "Easel study";
            c.isCipherClue = false;
            c.flavorLinesNoCipher = new[]
            {
                "A half-finished tree study. The brushwork is impatient."
            };
        });

        // Wall safe — interact → dialogue → type letters (max 4) + Enter
        var safe = new GameObject("ArtRoom_Safe_CodeLock");
        safe.transform.SetParent(paintingPuzzleRoot, false);
        safe.transform.position = new Vector3(4.85f, -2.05f, 0f);
        var safeCol = safe.AddComponent<BoxCollider2D>();
        safeCol.isTrigger = true;
        safeCol.size = new Vector2(0.65f, 0.55f);
        var safeComp = safe.AddComponent<ArtRoomSafeInteractable>();
        safeComp.correctCode = PuzzleState.ArtRoomCodeSolution;
        safeComp.maxCodeLength = PuzzleState.ArtRoomCodeSolution.Length;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(paintingPuzzleRoot.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
#endif
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

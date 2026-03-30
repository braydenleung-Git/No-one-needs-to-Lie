using UnityEngine;
using UnityEngine.SceneManagement;

// attach to the Main Camera in the Crime Scene level alongside RuntimeSceneSetup
// builds all the level-specific stuff: background, walls, cassette tape, cassette player, witness NPC
// runs after RuntimeSceneSetup (which handles player collider, dialogue canvas, prompt canvas, event system)
[DefaultExecutionOrder(10)]
public class Level2SceneBuilder : MonoBehaviour
{
    // Scene_Lvl2.png - assign in inspector on the Main Camera
    public Sprite backgroundSprite;

    // grandpa/witness NPC sprite - assign in inspector (can reuse same sprite for now)
    public Sprite witnessSprite;

    private void Awake()
    {
        PuzzleState.Reset();

        Scene myScene = gameObject.scene;

        var bg      = CreateBackground(myScene);
        var walls   = CreateWalls(myScene);
        var tape    = CreateCassetteTape(myScene);
        var player  = CreateCassettePlayer(myScene);
        var witness = CreateWitnessNPC(myScene);

        // wire the cassette player to the witness NPC so it can activate it after playback
        var cassetteComp = player.GetComponent<CassettePlayerInteractable>();
        if (cassetteComp != null)
            cassetteComp.witnessNPC = witness.GetComponent<NPCController>();
    }

    // ── Background ────────────────────────────────────────────────────────────

    GameObject CreateBackground(Scene scene)
    {
        var go = new GameObject("Background");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = backgroundSprite;
        sr.sortingOrder = -10; // render behind everything
        go.transform.position = Vector3.zero;
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Walls ─────────────────────────────────────────────────────────────────

    // background is 13.92 x 6.44 world units at 100 PPU, centered at (0,0)
    // house is L-shaped: full width on top, right portion cut off on bottom-right
    // all positions are approximate - tune visually using the red MapBoundary gizmos
    GameObject CreateWalls(Scene scene)
    {
        var parent = new GameObject("Walls");
        SceneManager.MoveGameObjectToScene(parent, scene);

        // outer walls
        MakeWall(parent, "Wall_Top",            new Vector2(0f,     3.22f),  new Vector2(13.92f, 0.3f));
        MakeWall(parent, "Wall_Left",           new Vector2(-6.96f, 0f),     new Vector2(0.3f,   6.44f));
        MakeWall(parent, "Wall_Bottom_Left",    new Vector2(-3.5f,  -3.22f), new Vector2(7.0f,   0.3f));
        MakeWall(parent, "Wall_Bottom_Right",   new Vector2(4.5f,   -3.22f), new Vector2(5.0f,   0.3f));
        MakeWall(parent, "Wall_Right_Top",      new Vector2(6.96f,  1.6f),   new Vector2(0.3f,   3.2f));
        MakeWall(parent, "Wall_Right_Bottom",   new Vector2(6.96f,  -2.4f),  new Vector2(0.3f,   0.9f));

        // L-shape cutout edges (bottom-right corner of the image is cut off)
        MakeWall(parent, "Wall_LCutout_H",      new Vector2(3.5f,   -1.6f),  new Vector2(6.9f,   0.3f));
        MakeWall(parent, "Wall_LCutout_V",      new Vector2(0.05f,  -2.4f),  new Vector2(0.3f,   1.7f));

        // interior room dividers
        MakeWall(parent, "Wall_KitchenDivider", new Vector2(-2.8f,  0.5f),   new Vector2(0.3f,   4.5f));
        MakeWall(parent, "Wall_LivingBedroom",  new Vector2(1.4f,   1.6f),   new Vector2(0.3f,   3.3f));
        MakeWall(parent, "Wall_Bedroom1_Floor", new Vector2(4.2f,   0.7f),   new Vector2(5.5f,   0.3f));
        MakeWall(parent, "Wall_Bedroom2_Floor", new Vector2(4.2f,   -0.5f),  new Vector2(5.5f,   0.3f));
        MakeWall(parent, "Wall_Entrance_Hall",  new Vector2(0f,     -1.8f),  new Vector2(2.8f,   0.3f));

        return parent;
    }

    void MakeWall(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;

        var col      = go.AddComponent<BoxCollider2D>();
        col.size     = size;
        col.isTrigger = false;

        go.AddComponent<MapBoundary>();
    }

    // ── Cassette Tape ─────────────────────────────────────────────────────────

    GameObject CreateCassetteTape(Scene scene)
    {
        var go = new GameObject("CassetteTape");
        go.transform.position = new Vector3(-4.5f, 0.8f, 0f);

        // yellow square placeholder - replace with actual tape sprite when art is ready
        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreatePlaceholderSprite(Color.yellow);
        sr.sortingOrder = 1;
        go.transform.localScale = new Vector3(0.3f, 0.2f, 1f);

        go.AddComponent<YSortingOrder>();

        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1f, 1f); // trigger zone around the tape
        col.isTrigger = true;

        go.AddComponent<CassetteTapePickup>();

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Cassette Player ───────────────────────────────────────────────────────

    GameObject CreateCassettePlayer(Scene scene)
    {
        var go = new GameObject("CassettePlayer");
        go.transform.position = new Vector3(-0.3f, 0.4f, 0f);

        // dark gray square placeholder - replace with actual sprite when art is ready
        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreatePlaceholderSprite(new Color(0.3f, 0.3f, 0.3f));
        sr.sortingOrder = 1;
        go.transform.localScale = new Vector3(0.4f, 0.25f, 1f);

        var col       = go.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(1f, 1f);
        col.isTrigger = true;

        go.AddComponent<CassettePlayerInteractable>();

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Witness NPC ───────────────────────────────────────────────────────────

    GameObject CreateWitnessNPC(Scene scene)
    {
        var go = new GameObject("Witness");
        go.transform.position   = new Vector3(0.5f, 0.8f, 0f);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        var sr    = go.AddComponent<SpriteRenderer>();
        sr.sprite = witnessSprite;
        sr.sortingOrder = 2;

        go.AddComponent<Animator>();
        go.AddComponent<YSortingOrder>();

        var col       = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 3.0f; // 3.0 * scale 0.4 = 1.2 world units

        var npc           = go.AddComponent<NPCController>();
        npc.npcName       = "Witness";
        npc.dialogueLines = new[]
        {
            "Oh, you found the tape! I recorded that the night it happened.",
            "I saw someone leave the art room around 1 AM. I didn't recognize them.",
            "They were carrying something wrapped in cloth. I should have called someone.",
            "Whatever happened here... it started in that room. Be careful."
        };

        // starts hidden - CassettePlayerInteractable.OnRecordingComplete activates it
        go.SetActive(false);

        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    // creates a 4x4 solid-color sprite for placeholder objects
    static Sprite CreatePlaceholderSprite(Color color)
    {
        var tex   = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
    }
}

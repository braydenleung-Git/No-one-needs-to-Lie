using UnityEngine;

// Holds a single UV flashlight Item instance so inventory references stay stable across scenes
public class PersistentGameItems : MonoBehaviour
{
    static PersistentGameItems _instance;
    Item _uvFlashlight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureRoot();
    }

    static void EnsureRoot()
    {
        if (_instance != null) return;

        var go = new GameObject("PersistentGameItems");
        _instance = go.AddComponent<PersistentGameItems>();
        DontDestroyOnLoad(go);

        var uvGo = new GameObject("UVFlashlight_Item");
        uvGo.transform.SetParent(go.transform, false);
        _instance._uvFlashlight = uvGo.AddComponent<Item>();
        _instance._uvFlashlight.ItemName = "UV Flashlight";
        _instance._uvFlashlight.Description =
            "Reveals ink and residue not visible under normal light. Fits in a pocket.";
    }

    public static Item UvFlashlightItem
    {
        get
        {
            EnsureRoot();
            return _instance._uvFlashlight;
        }
    }

    public static void GrantUvFlashlightToPlayer()
    {
        if (GameProgress.HasUvFlashlight) return;

        EnsureRoot();
        GameProgress.SetUvFlashlightOwned();

        var player = GameObject.FindGameObjectWithTag("Player");
        var inv = player != null ? player.GetComponent<PlayerInventory>() : null;
        if (inv != null && !inv.HasItem(_instance._uvFlashlight))
            inv.AddItem(_instance._uvFlashlight);
    }

    /// <summary>Call from PlayerInventory.Start or scene bootstrap so loads after the puzzle still carry the item.</summary>
    public static void HydrateUvFlashlight(PlayerInventory inv)
    {
        if (inv == null || !GameProgress.HasUvFlashlight) return;

        EnsureRoot();
        if (!inv.HasItem(_instance._uvFlashlight))
            inv.AddItem(_instance._uvFlashlight);
    }
}

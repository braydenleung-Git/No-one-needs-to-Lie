using UnityEngine;

/// <summary>
/// Concrete accessory: spawns a hat sprite as a child of the detective.
/// Implemented as a runtime-created ScriptableObject singleton so it can be
/// selected without needing an asset file.
/// </summary>
public sealed class DetectiveHatAccessory : DetectiveAccessory
{
    const string HatObjectName = "DetectiveHat";

    static DetectiveHatAccessory _instance;
    public static DetectiveHatAccessory Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = CreateInstance<DetectiveHatAccessory>();
            _instance.hideFlags = HideFlags.HideAndDontSave;
            return _instance;
        }
    }

    // These are configured by the customization UI at runtime (since it already has the Sprite).
    public Sprite HatSprite { get; set; }
    public Vector3 LocalPosition { get; set; } = new Vector3(0f, 0.2f, 0f);
    public int SortingOrder { get; set; } = 1;

    public override void Apply(GameObject detective)
    {
        if (detective == null) return;

        // Remove any existing hat first (use DestroyImmediate so a same-frame
        // re-apply doesn't leave a ghost that Destroy() would clean up later).
        var existing = detective.transform.Find(HatObjectName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        if (HatSprite == null) return;

        var baseRenderer = detective.GetComponent<SpriteRenderer>();
        if (baseRenderer == null)
            baseRenderer = detective.GetComponentInChildren<SpriteRenderer>();

        var hatGO = new GameObject(HatObjectName);
        hatGO.transform.SetParent(detective.transform, false);
        hatGO.transform.localPosition = ComputeLocalPosition(baseRenderer);
        hatGO.transform.localScale = Vector3.one;

        // Match the detective's layer so camera culling masks still apply.
        hatGO.layer = detective.layer;

        var sr = hatGO.AddComponent<SpriteRenderer>();
        sr.sprite = HatSprite;
        sr.color = Color.white;

        // Render above the detective regardless of scene sorting setup.
        if (baseRenderer != null)
        {
            sr.sortingLayerID = baseRenderer.sortingLayerID;
            sr.sortingOrder = baseRenderer.sortingOrder + 1;
        }
        else
        {
            sr.sortingOrder = SortingOrder;
        }
    }

    Vector3 ComputeLocalPosition(SpriteRenderer baseRenderer)
    {
        float yAboveHead = 0.12f; // fallback: just above center

        if (baseRenderer != null && baseRenderer.sprite != null)
        {
            // top of detective sprite in detective-local units
            yAboveHead = baseRenderer.sprite.bounds.extents.y + 0.02f;
        }

        float xCenter = 0f;
        if (HatSprite != null)
        {
            // Hat_0 has a bottom-left pivot (0,0). bounds.center.x is the
            // offset from that pivot to the sprite's visual center, so we
            // subtract it to place the hat centered over the detective's head.
            xCenter = -HatSprite.bounds.center.x;
        }

        return new Vector3(xCenter, yAboveHead, 0f);
    }

    public override void Remove(GameObject detective)
    {
        if (detective == null) return;

        var existing = detective.transform.Find(HatObjectName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);
    }
}


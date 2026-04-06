using UnityEngine;

// Strips patrol / physics / vision cone from the Owner prefab so it works as a standing dialogue NPC.
public static class NpcPrefabUtility
{
    static void DestroyObj(Object o)
    {
        if (o == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(o);
            return;
        }
#endif
        Object.Destroy(o);
    }

    public static void ConfigureOwnerPrefabForDialogueNpc(GameObject root)
    {
        var patrol = root.GetComponent<Owner_Patrol>();
        if (patrol != null)
            DestroyObj(patrol);

        var rb = root.GetComponent<Rigidbody2D>();
        if (rb != null)
            DestroyObj(rb);

        var cap = root.GetComponent<CapsuleCollider2D>();
        if (cap != null)
            DestroyObj(cap);

        var vision = root.transform.Find("VisionCone");
        if (vision != null)
            DestroyObj(vision.gameObject);

        if (root.GetComponent<YSortingOrder>() == null)
            root.AddComponent<YSortingOrder>();

        if (root.GetComponent<CircleCollider2D>() == null)
        {
            var circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 3f;
        }
    }
}

using UnityEngine;

/// <summary>
/// Generic solid-collision boundary for top-down maps.
///
/// HOW TO USE:
///   1. Create an empty GameObject (or a sprite) where a wall/edge should be.
///   2. Add this component — a BoxCollider2D is added automatically if none exists.
///   3. Resize the collider in the Scene view (green box) to match your wall shape.
///   4. For non-rectangular walls use a PolygonCollider2D or CompositeCollider2D
///      instead — this script keeps whichever collider is already present.
///
/// HOW BLOCKING WORKS:
///   PlayerController.FixedUpdate() calls rb.Cast() in the movement direction.
///   If the cast hits any non-trigger Collider2D the player stops.
///   This script ensures the collider is always solid (isTrigger = false).
///
/// LAYER SETUP (recommended):
///   - Create a layer called "Obstacle" in Project Settings > Tags and Layers.
///   - Set this GameObject's layer to "Obstacle".
///   - On the Detective GameObject set movementFilter.useLayerMask = true
///     and include the "Obstacle" layer so the player only blocks on real walls
///     rather than every non-trigger collider in the scene.
/// </summary>
[DisallowMultipleComponent]
public class MapBoundary : MonoBehaviour
{
    // ── Editor setup ──────────────────────────────────────────────────────────

    // Called by Unity when the component is first added in the Editor.
    // Adds a BoxCollider2D automatically so the designer can start sizing immediately.
    void Reset()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();

        EnforceSolid();
    }

    // ── Runtime ───────────────────────────────────────────────────────────────

    void Awake()
    {
        EnforceSolid();
    }

    // Guarantees the collider is never accidentally set to trigger.
    void EnforceSolid()
    {
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = false;
    }

    // ── Scene-view gizmo ─────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Highlight the boundary in red when selected so it's easy to spot.
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
            Gizmos.DrawWireCube(box.offset, box.size);
        }
        else
        {
            // Fallback for PolygonCollider2D / other shapes
            Gizmos.DrawWireCube(col.bounds.center - transform.position, col.bounds.size);
        }
    }
#endif
}

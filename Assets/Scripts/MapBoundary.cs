using UnityEngine;

// generic wall/boundary script for the top-down maps
// just slap this on any object that should block the player from walking through it
// a BoxCollider2D gets added automatically when you first add this component
//
// how the blocking actually works:
//   PlayerController does rb.Cast() in the move direction each FixedUpdate
//   if it hits any non-trigger collider the player stops
//   this script makes sure the collider is always solid (never a trigger)
//
// tip: make an "Obstacle" layer in project settings and put walls on it,
//   then set the player's movementFilter to only check that layer
//   so the player doesn't randomly stop on trigger zones or other stuff
[DisallowMultipleComponent]
public class MapBoundary : MonoBehaviour
{
    // called when you first drop this component onto an object in the editor
    // adds a BoxCollider2D so you can immediately start resizing it in the scene view
    void Reset()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();

        EnforceSolid();
    }

    void Awake()
    {
        // make sure nobody accidentally set isTrigger = true on the collider at runtime
        EnforceSolid();
    }

    // loops through all colliders and forces them to non-trigger
    // rb.Cast() completely ignores triggers so this is important
    void EnforceSolid()
    {
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // draw a red box in the scene view so walls are easy to spot when selected
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
            // fallback for polygon colliders or whatever else
            Gizmos.DrawWireCube(col.bounds.center - transform.position, col.bounds.size);
        }
    }
#endif
}

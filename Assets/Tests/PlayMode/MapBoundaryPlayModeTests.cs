using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// play mode tests for MapBoundary - these actually spin up the physics engine
// so they're slower than edit mode tests but way more realistic
// run via: Window > General > Test Runner > PlayMode tab > Run All
//
// basically testing the same thing PlayerController does at runtime:
//   rb.Cast() in movement direction -> if count > 0 don't move
//   making sure a MapBoundary wall actually produces those hits
public class MapBoundaryPlayModeTests
{
    GameObject playerGO;
    GameObject wallGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // ── player setup ───────────────────────────────────────────────────────
        playerGO = new GameObject("TestPlayer");
        playerGO.tag = "Player";

        var rb        = playerGO.AddComponent<Rigidbody2D>();
        rb.bodyType   = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var playerCol  = playerGO.AddComponent<CapsuleCollider2D>();
        playerCol.size = new Vector2(0.5f, 0.9f);
        playerCol.isTrigger = false;

        playerGO.transform.position = Vector2.zero;

        // ── wall setup ────────────────────────────────────────────────────────
        wallGO = new GameObject("TestWall");

        var wallCol  = wallGO.AddComponent<BoxCollider2D>();
        wallCol.size = new Vector2(1f, 10f);
        wallGO.AddComponent<MapBoundary>();

        // put the wall 1.5 units to the right of the player
        wallGO.transform.position = new Vector2(1.5f, 0f);

        // wait one physics tick for colliders to register
        yield return new WaitForFixedUpdate();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(playerGO);
        Object.Destroy(wallGO);
    }

    // ── core blocking tests ────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator PlayerCast_HitsWall_WhenMovingTowardsIt()
    {
        var rb     = playerGO.GetComponent<Rigidbody2D>();
        var filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.AllLayers);

        var hits  = new List<RaycastHit2D>();
        float castDist = 2f;   // 2 units to the right, wall is at 1.5 so this should definitely hit

        yield return new WaitForFixedUpdate();

        int count = rb.Cast(Vector2.right, filter, hits, castDist);

        Assert.Greater(count, 0,
            "rb.Cast() towards a MapBoundary wall should return at least one hit.");
    }

    [UnityTest]
    public IEnumerator PlayerCast_ReturnsNoHit_WhenMovingAwayFromWall()
    {
        var rb     = playerGO.GetComponent<Rigidbody2D>();
        var filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.AllLayers);

        var hits = new List<RaycastHit2D>();

        yield return new WaitForFixedUpdate();

        // cast left - wall is on the right so nothing should be in the way
        int count = rb.Cast(Vector2.left, filter, hits, 2f);

        Assert.AreEqual(0, count,
            "rb.Cast() away from the wall should return zero hits.");
    }

    [UnityTest]
    public IEnumerator PlayerPosition_DoesNotChange_WhenBlockedByWall()
    {
        var rb     = playerGO.GetComponent<Rigidbody2D>();
        var filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.AllLayers);

        var hits    = new List<RaycastHit2D>();
        Vector2 dir = Vector2.right;
        float speed = 5f;
        float dist  = speed * Time.fixedDeltaTime + 0.05f;

        Vector2 startPos = rb.position;

        yield return new WaitForFixedUpdate();

        // this mirrors exactly what PlayerController.FixedUpdate does
        int count = rb.Cast(dir, filter, hits, dist);
        if (count == 0)
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(startPos, rb.position,
            "Player position must not change when rb.Cast() hits a MapBoundary wall.");
    }

    [UnityTest]
    public IEnumerator WallCollider_IsNotATrigger_AtRuntime()
    {
        yield return new WaitForFixedUpdate();

        var col = wallGO.GetComponent<Collider2D>();

        Assert.IsNotNull(col, "Wall must have a Collider2D.");
        Assert.IsFalse(col.isTrigger,
            "MapBoundary collider must not be a trigger — triggers are ignored by rb.Cast().");
    }
}

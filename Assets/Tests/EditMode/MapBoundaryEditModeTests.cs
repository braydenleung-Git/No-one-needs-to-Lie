using NUnit.Framework;
using UnityEngine;

// edit mode tests for MapBoundary - these run instantly without entering play mode
// good for checking collider logic without waiting for the full physics sim to boot up
// run via: Window > General > Test Runner > EditMode tab > Run All
public class MapBoundaryEditModeTests
{
    GameObject go;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("TestWall");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    // ── collider enforcement ──────────────────────────────────────────────────

    [Test]
    public void Awake_ForcesIsTrigger_ToFalse_WhenColliderWasATrigger()
    {
        // start with a trigger collider (like someone accidentally checked the box)
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // adding MapBoundary should immediately fix it via EnforceSolid
        go.AddComponent<MapBoundary>();

        // rb.Cast() only detects non-triggers so this absolutely has to be false
        Assert.IsFalse(col.isTrigger,
            "MapBoundary must force isTrigger = false so the player's rb.Cast() can detect it.");
    }

    [Test]
    public void Awake_LeavesNonTriggerCollider_Unchanged()
    {
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        go.AddComponent<MapBoundary>();

        Assert.IsFalse(col.isTrigger, "Non-trigger collider should remain non-trigger.");
    }

    [Test]
    public void Awake_EnforcesAllColliders_WhenMultiplePresent()
    {
        // edge case - two colliders both set to trigger, both should get fixed
        var col1 = go.AddComponent<BoxCollider2D>();
        var col2 = go.AddComponent<CapsuleCollider2D>();
        col1.isTrigger = true;
        col2.isTrigger = true;

        go.AddComponent<MapBoundary>();

        Assert.IsFalse(col1.isTrigger, "First collider must be non-trigger.");
        Assert.IsFalse(col2.isTrigger, "Second collider must be non-trigger.");
    }

    // ── component integrity ───────────────────────────────────────────────────

    [Test]
    public void MapBoundary_CanCoexist_WithBoxCollider2D()
    {
        go.AddComponent<BoxCollider2D>();
        Assert.DoesNotThrow(() => go.AddComponent<MapBoundary>(),
            "Adding MapBoundary alongside an existing BoxCollider2D should not throw.");
    }

    [Test]
    public void MapBoundary_CanCoexist_WithPolygonCollider2D()
    {
        go.AddComponent<PolygonCollider2D>();
        Assert.DoesNotThrow(() => go.AddComponent<MapBoundary>(),
            "MapBoundary should work with any Collider2D type, not just BoxCollider2D.");
    }
}

using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for MapBoundary.
/// These run without entering Play mode so they execute instantly.
///
/// Run via: Window > General > Test Runner > EditMode tab > Run All
/// </summary>
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

    // ── Collider enforcement ──────────────────────────────────────────────────

    [Test]
    public void Awake_ForcesIsTrigger_ToFalse_WhenColliderWasATrigger()
    {
        // Arrange: add a trigger collider first
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Act: adding MapBoundary calls Awake which calls EnforceSolid
        go.AddComponent<MapBoundary>();

        // Assert
        Assert.IsFalse(col.isTrigger,
            "MapBoundary must force isTrigger = false so the player's rb.Cast() can detect it.");
    }

    [Test]
    public void Awake_LeavesNonTriggerCollider_Unchanged()
    {
        // Arrange
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        // Act
        go.AddComponent<MapBoundary>();

        // Assert
        Assert.IsFalse(col.isTrigger, "Non-trigger collider should remain non-trigger.");
    }

    [Test]
    public void Awake_EnforcesAllColliders_WhenMultiplePresent()
    {
        // Arrange: two colliders, both triggers
        var col1 = go.AddComponent<BoxCollider2D>();
        var col2 = go.AddComponent<CapsuleCollider2D>();
        col1.isTrigger = true;
        col2.isTrigger = true;

        // Act
        go.AddComponent<MapBoundary>();

        // Assert
        Assert.IsFalse(col1.isTrigger, "First collider must be non-trigger.");
        Assert.IsFalse(col2.isTrigger, "Second collider must be non-trigger.");
    }

    // ── Component integrity ───────────────────────────────────────────────────

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

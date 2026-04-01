using UnityEngine;

public class XPSystemTester : MonoBehaviour
{
    [Header("Test Settings")]
    public int testXPAmount = 50;
    public int testRequiredLevel = 3;

    [ContextMenu("Test/Add XP (single award)")]
    private void TestAddXP()
    {
        ExperienceManager.Instance.AddXP(testXPAmount);
        LogStatus();
    }

    [ContextMenu("Test/Simulate Puzzle Solved")]
    private void TestPuzzleSolved()
    {
        Debug.Log("[Test] Puzzle solved event fired.");
        ExperienceManager.Instance.AddXP(50);
        LogStatus();
    }

    [ContextMenu("Test/Simulate Level Cleared")]
    private void TestLevelCleared()
    {
        Debug.Log("[Test] Level cleared event fired.");
        ExperienceManager.Instance.AddXP(100);
        LogStatus();
    }

    [ContextMenu("Test/Simulate Inventory Filled")]
    private void TestInventoryFilled()
    {
        Debug.Log("[Test] Inventory filled event fired.");
        ExperienceManager.Instance.AddXP(75);
        LogStatus();
    }

    [ContextMenu("Test/Force Level Up")]
    private void TestForceLevelUp()
    {
        int xpNeeded = ExperienceManager.Instance.xpToNextLevel - ExperienceManager.Instance.currentXP;
        Debug.Log($"[Test] Forcing level up — adding {xpNeeded} XP.");
        ExperienceManager.Instance.AddXP(xpNeeded);
        LogStatus();
    }

    [ContextMenu("Test/Check Item Lock (uses testRequiredLevel)")]
    private void TestItemLock()
    {
        int playerLevel = ExperienceManager.Instance.currentLevel;
        bool unlocked = playerLevel >= testRequiredLevel;
        Debug.Log(unlocked
            ? $"[Test] Item UNLOCKED — Player Level {playerLevel} meets requirement ({testRequiredLevel})."
            : $"[Test] Item LOCKED — Player Level {playerLevel} does not meet requirement ({testRequiredLevel}).");
    }

    [ContextMenu("Test/Reset XP System")]
    private void TestReset()
    {
        ExperienceManager.Instance.currentXP = 0;
        ExperienceManager.Instance.currentLevel = 1;
        ExperienceManager.Instance.xpToNextLevel = 100;
        Debug.Log("[Test] XP system reset to Level 1, 0 XP.");
    }

    [ContextMenu("Test/Log Current Status")]
    private void LogStatus()
    {
        var xp = ExperienceManager.Instance;
        Debug.Log($"[Status] Level: {xp.currentLevel} | XP: {xp.currentXP}/{xp.xpToNextLevel} | Progress: {xp.GetXPProgress() * 100:F1}%");
    }
}
using UnityEngine;

public class LevelLockedItem : MonoBehaviour, ILevelLocked
{
    [Header("Lock Settings")]
    [SerializeField] private int requiredLevel = 3;
    [SerializeField] private string itemName = "Locked Item";

    public int RequiredLevel => requiredLevel;

    public bool IsUnlocked()
    {
        return ExperienceManager.Instance.currentLevel >= requiredLevel;
    }

    public bool TryAccess()
    {
        if (IsUnlocked())
        {
            Debug.Log($"[Access Granted] {itemName} unlocked!");
            return true;
        }

        int levelsNeeded = requiredLevel - ExperienceManager.Instance.currentLevel;
        Debug.Log($"[Locked] {itemName} requires Level {requiredLevel}. You need {levelsNeeded} more level(s).");
        return false;
    }
}
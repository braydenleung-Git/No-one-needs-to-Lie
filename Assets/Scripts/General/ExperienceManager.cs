using UnityEngine;
using UnityEngine.Events;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("XP Settings")]
    public int currentXP = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;

    [Header("Events")]
    public UnityEvent<int> OnXPChanged;
    public UnityEvent<int> OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        OnXPChanged?.Invoke(currentXP);
        Debug.Log($"[XP] +{amount} XP | Total: {currentXP}");
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = CalculateNextLevelXP(currentLevel);
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"[LEVEL UP] Now Level {currentLevel}! Next level needs {xpToNextLevel} XP.");
        }
    }

    private int CalculateNextLevelXP(int level)
    {
        return 100 * level;
    }

    public float GetXPProgress() => (float)currentXP / xpToNextLevel;
}
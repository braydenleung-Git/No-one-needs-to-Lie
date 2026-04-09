using UnityEngine;
using UnityEngine.Events;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("XP Settings")]
    [Tooltip("Starting progress toward level 2 (e.g. 20 / xpToNextLevel ≈ one fifth of the bar).")]
    public int currentXP    = 20;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;

    // Level 5 is the hard cap — no XP is accepted beyond this
    public const int MAX_LEVEL = 5;

    [Header("Events")]
    public UnityEvent<int> OnXPChanged;
    public UnityEvent<int> OnLevelUp;

    private void Awake()
    {
        // Runtime-created managers (e.g. bootstrap) do not get UnityEvent fields serialized — leave them null unless we init here.
        if (OnXPChanged == null) OnXPChanged = new UnityEvent<int>();
        if (OnLevelUp == null) OnLevelUp = new UnityEvent<int>();

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddXP(int amount)
    {
        // Silently ignore XP gain once the player is at max level
        if (currentLevel >= MAX_LEVEL) return;

        currentXP += amount;
        OnXPChanged?.Invoke(currentXP);
        Debug.Log($"[XP] +{amount} XP | Total: {currentXP}");
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (currentXP >= xpToNextLevel && currentLevel < MAX_LEVEL)
        {
            currentXP    -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = CalculateNextLevelXP(currentLevel);
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"[LEVEL UP] Now Level {currentLevel}! Next level needs {xpToNextLevel} XP.");
        }

        // At max level: clamp XP so the bar shows full and clean
        if (currentLevel >= MAX_LEVEL)
        {
            currentXP    = xpToNextLevel;
            OnXPChanged?.Invoke(currentXP);
            Debug.Log("[XP] Max level reached! Bar is now full.");
        }
    }

    private int CalculateNextLevelXP(int level)
    {
        return 100;
    }

    // Returns 0.0 – 1.0 for the fill image. At max level returns exactly 1.
    public float GetXPProgress()
    {
        if (currentLevel >= MAX_LEVEL) return 1f;
        return (float)currentXP / xpToNextLevel;
    }

    public bool IsMaxLevel() => currentLevel >= MAX_LEVEL;
}
using UnityEngine;

// singleton that persists across all scene loads
// attach this to a GameObject in your very first scene (Town)
// it will carry all game flags through every level transition
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // ── progress flags ────────────────────────────────────────────────
    public bool SofaInvestigated = false;
    public bool SafeSolved       = false;
    public bool Level1Complete   = false;
    public bool Level2Complete   = false;
    public bool Level3Complete   = false;
    public bool Level4Complete   = false;
    public bool Level5Complete   = false;

    void Awake()
    {
        // only one GameState ever exists, survives every scene load
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // level completion flags
    public bool Level1Complete = true;// chnage to false later
    public bool Level2Complete = true;
    public bool Level3Complete = false;
    public bool Level4Complete = false;
    public bool Level5Complete = false;

    // level 3 specific flags
    public bool SofaInvestigated = false;
    public bool SafeSolved       = false;

    // check if a level is unlocked based on previous completion
    public bool IsLevelUnlocked(int level)
    {
        switch (level)
        {
            case 1: return true;                    // always unlocked
            case 2: return Level1Complete;
            case 3: return Level2Complete;
            case 4: return Level3Complete;
            case 5: return Level4Complete;
            default: return false;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
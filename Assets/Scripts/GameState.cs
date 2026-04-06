using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // level completion flags
    [HideInInspector] public bool Level1Complete = false;
    [HideInInspector] public bool Level2Complete = false;
    [HideInInspector] public bool Level3Complete = false;
    [HideInInspector] public bool Level4Complete = false;
    [HideInInspector] public bool Level5Complete = false;

    // level 3 specific flags
    [HideInInspector] public bool SofaInvestigated = false;
    [HideInInspector] public bool SafeSolved       = false;

    // stores positions of moved objects across scene reloads
    // key = object unique ID, value = saved position
    private Dictionary<string, Vector3> savedPositions = new Dictionary<string, Vector3>();

    // stores bool states of objects across scene reloads
    // key = object unique ID, value = saved bool
    private Dictionary<string, bool> savedStates = new Dictionary<string, bool>();

    public bool IsLevelUnlocked(int level)
    {
        switch (level)
        {
            case 1: return true;
            case 2: return Level1Complete;
            case 3: return Level2Complete;
            case 4: return Level3Complete;
            case 5: return Level4Complete;
            default: return false;
        }
    }

    // ── Position saving ───────────────────────────────────────────────

    public void SavePosition(string id, Vector3 position)
    {
        savedPositions[id] = position;
    }

    public bool TryGetPosition(string id, out Vector3 position)
    {
        return savedPositions.TryGetValue(id, out position);
    }

    // ── Bool state saving ─────────────────────────────────────────────

    public void SaveState(string id, bool state)
    {
        savedStates[id] = state;
    }

    public bool TryGetState(string id, out bool state)
    {
        return savedStates.TryGetValue(id, out state);
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

        // TEMP for testing - remove before final build
        Level1Complete = true;
        Level2Complete = true;
    }
}
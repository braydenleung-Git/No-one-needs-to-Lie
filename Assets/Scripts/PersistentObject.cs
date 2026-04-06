using UnityEngine;

// attach to any object that needs to remember its state across scene reloads
// teammates just attach this and give it a unique ID
// it automatically saves and restores position and active state
public abstract class PersistentObject : MonoBehaviour
{
    [Header("Persistent Object")]
    [Tooltip("Must be unique across the entire game - use format Level3_Sofa, Level2_Door etc")]
    [SerializeField] protected string uniqueID;

    protected virtual void Start()
    {
        if (GameState.Instance == null) return;
        RestoreState();
    }

    // called on Start to restore saved state
    // override in subclass to add extra restore logic
    protected virtual void RestoreState()
    {
        // restore position if saved
        if (GameState.Instance.TryGetPosition(uniqueID + "_pos", out Vector3 savedPos))
            transform.position = savedPos;

        // restore active state if saved
        if (GameState.Instance.TryGetState(uniqueID + "_active", out bool savedActive))
            gameObject.SetActive(savedActive);
    }

    // call this whenever the object moves or changes state
    protected void SaveCurrentPosition()
    {
        if (GameState.Instance == null) return;
        GameState.Instance.SavePosition(uniqueID + "_pos", transform.position);
    }

    protected void SaveCurrentState(bool state)
    {
        if (GameState.Instance == null) return;
        GameState.Instance.SaveState(uniqueID + "_active", state);
    }
}
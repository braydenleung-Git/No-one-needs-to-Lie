using UnityEngine;
using UnityEngine.InputSystem;

// base class for anything the player can walk up to and press E on
// NPCs, doors, chests etc all extend this
// just need a trigger collider on the object and the player tagged "Player"
public abstract class Interactable : MonoBehaviour, Hintable
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactPrompt = "Press [E] to interact";
    
    [SerializeField] private bool _isHinted = false;
    [SerializeField] private Sprite _hintIcon = null;
    
    protected bool playerInRange = false;
    protected GameObject player;
    
    public bool IsHinted { get; }
    public Sprite HintIcon
    {
        get => _hintIcon;
        set => _hintIcon = value;
    }

    private bool _previousHintedState = false;
    
    public void Hint(bool isHinted)
    {
        _isHinted = isHinted;
        _previousHintedState = isHinted;
    }

    protected virtual void Start()
    {
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        player = other.gameObject;
        _isHinted = false;
        InteractionPromptUI.Instance?.Show(interactPrompt, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        _isHinted = _previousHintedState;
        InteractionPromptUI.Instance?.Hide();
    }

    protected virtual void Update()
    {
        if (IsHinted)
        {
            
        }
        OnInteractableUpdateEarly();

        if (!playerInRange) return;

        // don't let the player re-trigger while dialogue is already open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;

        OnInteractableUpdateInRange();

        if (!AllowInteractWithE()) return;

        // using new input system here, old Input.GetKeyDown doesn't work with it
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    /// <summary>Runs every frame before range/dialogue checks (e.g. multi-frame input flows).</summary>
    protected virtual void OnInteractableUpdateEarly() { }

    /// <summary>Runs every frame while the player is in range and no dialogue is open.</summary>
    protected virtual void OnInteractableUpdateInRange() { }

    /// <summary>When false, E will not fire Interact() (subclasses can absorb E for other UI).</summary>
    protected virtual bool AllowInteractWithE() => true;

    // subclasses fill this in, e.g. NPCController starts dialogue
    public abstract void Interact();
}

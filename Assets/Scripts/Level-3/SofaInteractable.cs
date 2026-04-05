using UnityEngine;

// Attach this to your Sofa GameObject.
// Requires:
//   - BoxCollider2D (Is Trigger ON)  → for interaction detection
//   - BoxCollider2D (Is Trigger OFF) → for physical blocking
//   - SpriteRenderer
//   - YSortingOrder
public class SofaInteractable : Interactable
{
    [Header("Dialogue")]
    [SerializeField] private string sofaDialogue =
        "Oh! Looks like there is something hidden behind the sofa...";

    [Header("Sofa Movement")]
    [SerializeField] private Vector2 moveDirection = new Vector2(1f, 0f); // direction to slide (1,0 = right)
    [SerializeField] private float   moveDistance   = 2f;                 // how far it slides in world units
    [SerializeField] private float   moveSpeed      = 3f;                 // units per second

    private bool hasBeenInteracted = false; // one-shot: sofa only moves once
    private bool isMoving          = false;
    private Vector3 targetPosition;
    private Collider2D blockingCollider;    // the non-trigger collider that physically blocks the player

    private void Start()
    {
        // overrides the default prompt from Interactable base class
        interactPrompt = "Press [E] to investigate";

        // find the solid (non-trigger) collider specifically
        // the trigger collider is handled by the base Interactable class
        foreach (var col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                blockingCollider = col;
                break;
            }
        }
    }

    public override void Interact()
    {
        if (hasBeenInteracted) return; // sofa already moved, don't re-trigger
        hasBeenInteracted = true;

        // hide the prompt immediately so it doesn't float while dialogue plays
        InteractionPromptUI.Instance?.Hide();

        // show the dialogue first, then move the sofa after player dismisses it
        // onComplete callback fires after the player presses E on the last line
        DialogueManager.Instance?.StartDialogue(
            speakerName:   "",                          // empty = narration, no speaker name shown
            dialogueLines: new[] { sofaDialogue },
            onComplete:    StartSofaMovement
        );
    }

    private void StartSofaMovement()
    {
        isMoving       = true;
        targetPosition = transform.position +
                         (Vector3)(moveDirection.normalized * moveDistance);

        // disable the physical blocker so the player can walk into the revealed area
        if (blockingCollider != null)
            blockingCollider.enabled = false;

        // flag for GameState so SafeInteractable knows sofa has been investigated
        // safe prompt won't appear until this is true
        GameState.SofaInvestigated = true;
    }

    // override Update from Interactable to also handle the sliding movement
    protected override void Update()
    {
        base.Update(); // handles E key, trigger range, dialogue check

        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // snap to exact target once close enough to avoid floating point drift
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving           = false;
        }
    }
}
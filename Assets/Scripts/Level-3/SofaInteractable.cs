using UnityEngine;

// Attach this to your Sofa GameObject.
// Requires: a trigger CircleCollider2D (or BoxCollider2D) on the sofa,
// and the player tagged "Player" with a non-trigger Collider2D.
public class SofaInteractable : Interactable
{
    [Header("Dialogue")]
    [SerializeField] private string sofaDialogue =
        "Oh! Looks like there is something hidden behind the sofa...";

    [Header("Sofa Movement")]
    [SerializeField] private Vector2 moveDirection = new Vector2(1f, 0f); // direction to slide
    [SerializeField] private float   moveDistance   = 2f;                 // how far it slides
    [SerializeField] private float   moveSpeed      = 3f;                 // units per second

    private bool hasBeenInteracted = false; // one-shot: sofa only moves once
    private bool isMoving          = false;
    private Vector3 targetPosition;

    private void Start()
    {
        // set prompt text (inherited field from Interactable)
        interactPrompt = "Press [E] to investigate";
    }

    public override void Interact()
    {
        if (hasBeenInteracted) return; // don't re-trigger after it's already moved
        hasBeenInteracted = true;

        // hide the prompt immediately so it doesn't float while the sofa moves
        InteractionPromptUI.Instance?.Hide();

        // StartDialogue's onComplete callback kicks off the movement after
        // the player finishes reading and dismisses the dialogue
        DialogueManager.Instance?.StartDialogue(
            speakerName:   "",                          // no speaker name — it's a narration box
            dialogueLines: new[] { sofaDialogue },
            onComplete:    StartSofaMovement
        );
    }

    private void StartSofaMovement()
    {
        isMoving       = true;
        targetPosition = transform.position +
                         (Vector3)(moveDirection.normalized * moveDistance);
    }

    private void Update()
    {
        // let the base class handle trigger enter/exit and E key
        base.Update(); // NOTE: see caveat below

        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
}
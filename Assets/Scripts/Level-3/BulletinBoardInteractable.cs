using UnityEngine;
using UnityEngine.UI;

// Attach to BulletinBoard GameObject
// Requires:
//   - BoxCollider2D (Is Trigger ON) for interaction detection
//   - BulletinBoardUI canvas assigned in inspector
public class BulletinBoardInteractable : Interactable
{
    [Header("Bulletin Board UI")]
    [SerializeField] private GameObject bulletinBoardUI; // the full screen canvas
    [SerializeField] private Button closeButton;         // the X button

    private bool boardOpen = false;

    private void Start()
    {
        interactPrompt = "Press [E] to interact";

        if (bulletinBoardUI != null)
            bulletinBoardUI.SetActive(false);

        closeButton?.onClick.AddListener(CloseBoard);
    }

    public override void Interact()
    {
        if (boardOpen) return;
        OpenBoard();
    }

    private void OpenBoard()
    {
        boardOpen = true;
        bulletinBoardUI.SetActive(true);
        InteractionPromptUI.Instance?.Hide();

        // pause game while reading board
        Time.timeScale = 0f;
    }

    private void CloseBoard()
    {
        boardOpen = false;
        bulletinBoardUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // hide prompt when board is open
    protected override void OnInteractableUpdateInRange()
    {
        if (boardOpen)
            InteractionPromptUI.Instance?.Hide();
        else
            InteractionPromptUI.Instance?.Show(interactPrompt, transform);
    }

    // block E while board is open
    protected override bool AllowInteractWithE() => !boardOpen;
}
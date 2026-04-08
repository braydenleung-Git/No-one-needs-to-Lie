using UnityEngine;
using UnityEngine.UI;

public class PhotoFrameInteractable : Interactable
{
    [Header("Photo Frame UI")]
    [SerializeField] private GameObject photoFrameUI;
    [SerializeField] private Button closeButton;

    private bool photoOpen = false;

    private new void Start()
    {
        interactPrompt = "Press [E] to inspect";

        if (photoFrameUI != null)
            photoFrameUI.SetActive(false);

        closeButton?.onClick.AddListener(ClosePhoto);
    }

    public override void Interact()
    {
        if (photoOpen) return;
        OpenPhoto();
    }

    private void OpenPhoto()
    {
        photoOpen = true;
        photoFrameUI.SetActive(true);
        InteractionPromptUI.Instance?.Hide();
        Time.timeScale = 0f;
    }

    private void ClosePhoto()
    {
        photoOpen = false;
        photoFrameUI.SetActive(false);
        Time.timeScale = 1f;
    }

    protected override void OnInteractableUpdateInRange()
    {
        if (photoOpen)
            InteractionPromptUI.Instance?.Hide();
        else
            InteractionPromptUI.Instance?.Show(interactPrompt, transform);
    }

    protected override bool AllowInteractWithE() => !photoOpen;
}
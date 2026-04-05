using UnityEngine;
using UnityEngine.UI;

// Attach this to the SafeOpenUI canvas GameObject
// This handles showing and hiding the safe interior UI after it's been solved
public class SafeOpenUI : MonoBehaviour
{
    [SerializeField] private Button closeButton; // X button

    // lets SafeInteractable check if this UI is currently open
    public bool IsOpen => gameObject.activeSelf;

    private void Start()
    {
        closeButton?.onClick.AddListener(CloseUI);

        // safety check for testing directly without Town scene
        if (GameState.Instance != null)
            gameObject.SetActive(GameState.Instance.SafeSolved);
        else
            gameObject.SetActive(false); // always hide when testing directly
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        InteractionPromptUI.Instance?.Hide();
    }

    private void CloseUI()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
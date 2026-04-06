using UnityEngine;
using UnityEngine.SceneManagement;

// attach to exit trigger in each level
// player presses E to leave and returns to Town in front of the correct building
public class ReturnToTown : MonoBehaviour
{
    [SerializeField] private int levelNumber;                    // which level this exit belongs to
    [SerializeField] private string exitPrompt = "Press [E] to exit"; 

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractionPromptUI.Instance?.Show(exitPrompt, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractionPromptUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (levelNumber == 3)
                GameState.Instance.Level3Complete = true;

            PlayerSpawnManager.ReturnFromLevel = levelNumber;
            SceneManager.LoadScene("Town");
        }
    }
}
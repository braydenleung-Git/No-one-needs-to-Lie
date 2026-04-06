using UnityEngine;
using UnityEngine.SceneManagement;

// attach to exit trigger in each level
// returns to Town in front of the correct building
public class ReturnToTown : MonoBehaviour
{
    [SerializeField] private int levelNumber;                    // which level this exit belongs to
    [SerializeField] private string exitPrompt = "Press [E] to exit";
    [SerializeField] private bool autoExitOnEnter = false;       // if true, touching trigger returns immediately
    [SerializeField] private string townSceneName = "Town";       // scene to return to

    private bool playerInRange = false;
    private bool _exiting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (autoExitOnEnter)
        {
            ExitNow();
            return;
        }

        if (!string.IsNullOrWhiteSpace(exitPrompt))
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
        if (autoExitOnEnter) return;
        if (!playerInRange) return;
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExitNow();
        }
    }

    void ExitNow()
    {
        if (_exiting) return;
        _exiting = true;

        PlayerSpawnManager.ReturnFromLevel = levelNumber;
        SceneManager.LoadScene(townSceneName);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box == null || !box.enabled) return;
        Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.65f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube((Vector3)box.offset, new Vector3(box.size.x, box.size.y, 0.05f));
    }
#endif
}
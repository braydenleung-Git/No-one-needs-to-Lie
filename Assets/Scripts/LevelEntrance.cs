using UnityEngine;
using UnityEngine.SceneManagement;

// attach to each building entrance trigger in Town
// player walks in automatically, checks if level is unlocked
// if locked shows dialogue message, if unlocked loads the scene
public class LevelEntrance : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber;   // 1 through 5
    [SerializeField] private string sceneName;  // exact scene name e.g. "Level3"

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // safety check in case GameState not loaded yet
        if (GameState.Instance == null) return;

        if (GameState.Instance.IsLevelUnlocked(levelNumber))
        {
            // level is unlocked - load it immediately
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // level is locked - show dialogue message
            ShowLockedMessage();
        }
    }

    private void ShowLockedMessage()
    {
        DialogueManager.Instance?.StartDialogue(
            "",
            new[] { $"This area is locked. Complete Level {levelNumber - 1} first." }
        );
    }
}
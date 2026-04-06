using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnerPlayerCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        // reset all level 3 progress
        GameState.Instance.ResetLevel3();

        // tell town to spawn player in front of level 3 house
        PlayerSpawnManager.ReturnFromLevel = 3;

        Time.timeScale = 1f;

        // go back to town
        SceneManager.LoadScene("Town");
    }
}
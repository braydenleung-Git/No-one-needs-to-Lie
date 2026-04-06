using UnityEngine;
using UnityEngine.SceneManagement;

// attach this to the Player in the Town scene
// on start checks if we just returned from a level and spawns player at correct position
public class PlayerSpawnManager : MonoBehaviour
{
    public static int ReturnFromLevel = 0; // 0 means fresh start

    [Header("Spawn Points — match order to level number")]
    [SerializeField] private Transform[] spawnPoints; // 0 = default, 1 = outside Level1, etc.

    private void Start()
    {
        // no scale override here - the Detective prefab is (1, 1, 1) and that's correct for all scenes
        // previously we forced (0.4, 0.4, 1) here which was making the player tiny after returning from Crime Scene

        if (ReturnFromLevel > 0 && ReturnFromLevel < spawnPoints.Length)
        {
            // spawn player in front of the building they just came from
            transform.position = spawnPoints[ReturnFromLevel].position;
            ReturnFromLevel = 0; // reset
        }
    }
}
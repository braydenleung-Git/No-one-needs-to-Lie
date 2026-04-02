using UnityEngine;

// attach this to any sprite so it sorts correctly on the Y axis
// lower on screen = higher sort order = draws in front, gives the 2.5D illusion
// without this the player walks behind the npc which looks super wrong
[RequireComponent(typeof(SpriteRenderer))]
public class YSortingOrder : MonoBehaviour
{
    [Tooltip("how aggressively the sort order changes as Y changes, 10 works fine for most setups")]
    [SerializeField] private int sortingScale = 10;

    // without a base offset, sprites near y=0 end up on order 0 which matches
    // the background default - they'd randomly flicker behind walls every frame
    // keeping base at 100 means the background sitting at -1000 can never win
    [Tooltip("base order added on top so actors never render behind the background")]
    [SerializeField] private int baseOrder = 100;

    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        // negative Y because lower on screen = smaller Y value = should draw in FRONT
        // e.g. player at y=-1 → order 110, player at y=1 → order 90
        // so whoever's standing further south always appears in front, classic 2.5D trick
        sr.sortingOrder = baseOrder + Mathf.RoundToInt(-transform.position.y * sortingScale);
    }
}

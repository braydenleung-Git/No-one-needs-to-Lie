using UnityEngine;

// attach this to any sprite so it sorts correctly on the Y axis
// lower on screen = higher sort order = draws in front, gives the 2.5D illusion
// without this the player walks behind the npc which looks super wrong
[RequireComponent(typeof(SpriteRenderer))]
public class YSortingOrder : MonoBehaviour
{
    [Tooltip("how aggressively the sort order changes as Y changes, 10 works fine for most setups")]
    [SerializeField] private int sortingScale = 10;

    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        // negative Y because lower on screen = smaller Y = should draw in front
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * sortingScale);
    }
}

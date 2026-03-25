using UnityEngine;

/// <summary>
/// Attach to any sprite that should participate in Y-axis depth sorting.
/// Objects lower on screen (smaller Y) get a higher sorting order so they
/// render in front — giving the illusion of depth in a top-down/isometric view.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSortingOrder : MonoBehaviour
{
    [Tooltip("Multiplier for how aggressively sorting order changes with Y position.")]
    [SerializeField] private int sortingScale = 10;

    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * sortingScale);
    }
}

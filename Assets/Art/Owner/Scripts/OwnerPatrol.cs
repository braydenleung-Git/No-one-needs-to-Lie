using UnityEngine;

public class OwnerPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;

    private int currentIndex = 0;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];

        Vector2 direction = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        FlipSprite(direction);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    private void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0.1f)
            spriteRenderer.flipX = false; // moving right
        else if (direction.x < -0.1f)
            spriteRenderer.flipX = true;  // moving left
    }
}
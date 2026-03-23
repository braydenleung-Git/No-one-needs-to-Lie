using System.Collections;
using UnityEngine;

public class NPC_Patrol : MonoBehaviour
{
    public Vector2[] patrolPoints;
    private int currentPatrolIndex;
    public Vector2 target;
    public float speed = 2;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = patrolPoints[0];
    }

    void Update()
    {
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (Vector2.Distance(transform.position, target) < .1f)
            SetPatrolPoint();
    }

    private void SetPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex];
    }
}
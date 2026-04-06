using System.Collections;
using UnityEngine;

public class Owner_Patrol : MonoBehaviour, IPatroller
{
    public Transform[] patrolPoints;
    public float speed = 2;
    public float chaseSpeed = 4f;
    public float pauseDuration = 1.5f;

    private bool isPaused;
    private bool isChasing;
    private int currentPatrolIndex;
    private Vector2 target;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform chaseTarget;

    public Vector2 currentDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        target = patrolPoints[0].position;
    }

    void Update()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isChasing && chaseTarget != null)
            target = chaseTarget.position;

        Vector2 direction = ((Vector3)target - transform.position).normalized;
        currentDirection = direction;

        if (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.x);
        
        rb.linearVelocity = direction * (isChasing ? chaseSpeed : speed);

        UpdateAnimator(direction);

        if (!isChasing && Vector2.Distance(transform.position, target) < .1f)
            StartCoroutine(SetPatrolPoint());
    }

    private void UpdateAnimator(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            anim.Play("Walk");
        else if (direction.y < 0)
            anim.Play("WalkDown");
        else if (direction.y > 0)
            anim.Play("WalkUp");
    }

    IEnumerator SetPatrolPoint()
    {
        isPaused = true;
        anim.Play("Idle");
        currentDirection = Vector2.down;
        yield return new WaitForSeconds(pauseDuration);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex].position;
        isPaused = false;
        anim.Play("Walk");
    }

    public void StartChase(Transform player)
    {
        isChasing = true;
        isPaused = false;
        chaseTarget = player;
        StopAllCoroutines();
    }

    public void StopChase()
    {
        isChasing = false;
        chaseTarget = null;
        currentPatrolIndex = 0;
        target = patrolPoints[0].position;
    }
}
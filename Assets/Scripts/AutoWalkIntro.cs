using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoWalkIntro : MonoBehaviour
{
    public Vector2 targetPosition = new Vector2(0f, -2.14999f);
    public float walkSpeed = 2.5f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Animator animator;
    private PlayerController playerController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        playerInput.enabled = false;
        playerController.enabled = false;

        animator.Play("Walk_Back", 0);

        StartCoroutine(WalkToTarget());
    }

    IEnumerator WalkToTarget()
    {
        yield return new WaitForSeconds(0.3f);

        while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            Vector2 direction = ((Vector2)targetPosition - rb.position).normalized;
            rb.MovePosition(rb.position + direction * walkSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        animator.Play("Idle-Front", 0);

        playerController.enabled = true;
        playerInput.enabled = true;
    }
}
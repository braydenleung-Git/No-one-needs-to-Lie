using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoWalkIntro : MonoBehaviour
{
    public Vector2 targetPosition = new Vector2(0f, -1f);
    public float walkSpeed = 2.5f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        playerInput.enabled = false;
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
        playerInput.enabled = true;
    }
}
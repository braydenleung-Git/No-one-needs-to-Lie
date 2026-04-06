using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// handles all the player movement stuff
// uses the new input system so make sure that's set up in project settings
public class PlayerController : MonoBehaviour
{
    public float movespeed = 1f;
    public float collisionOffSet = 0.05f;

    public ContactFilter2D movementFilter;
    Vector2 movementInput;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    private Vector2 lastMoveDirection = Vector2.down;

    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Drive animator from Update so blend trees / sprite swaps stay in sync with the frame (ACPlayer uses 2D blend trees).
    void Update()
    {
        Vector2 animDirection = movementInput != Vector2.zero ? movementInput : lastMoveDirection;

        if (animator != null && canMove)
        {
            animator.SetFloat("MoveX", animDirection.x);
            animator.SetFloat("MoveY", animDirection.y);
            animator.SetBool("IsMoving", movementInput != Vector2.zero);
        }

        // Facing left/right comes from ACPlayer blend tree (side clip + Mirror on the -X node). Avoid flipX here or it doubles up.
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        if (movementInput != Vector2.zero)
        {
            int count = rb.Cast(
                movementInput,
                movementFilter,
                castCollisions,
                movespeed * Time.fixedDeltaTime + collisionOffSet
            );

            if (count == 0)
            {
                rb.MovePosition(rb.position + movementInput * movespeed * Time.fixedDeltaTime);
            }
        }

        Vector2 animDirection = movementInput != Vector2.zero ? movementInput : lastMoveDirection;

        animator.SetFloat("MoveX", animDirection.x);
        animator.SetFloat("MoveY", animDirection.y);
        animator.SetBool("IsMoving", movementInput != Vector2.zero);

        if (movementInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (movementInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        if (canMove)
        {
            movementInput = movementValue.Get<Vector2>();
        }
        if (movementInput != Vector2.zero)
        {
            lastMoveDirection = movementInput;
        }
    }
}
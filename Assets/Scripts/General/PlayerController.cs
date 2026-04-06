using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    private void FixedUpdate()
    {
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
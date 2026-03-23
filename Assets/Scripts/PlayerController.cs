using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float movespeed = 1f;
    public float collisionOffSet = 0.05f;

    public ContactFilter2D movementFilter;
    Vector2 movementInput;

    Rigidbody2D rb;

    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (movementInput != Vector2.zero)
        {
            int count = rb.Cast(movementInput, movementFilter, castCollisions, movespeed * Time.fixedDeltaTime + collisionOffSet);

            if (count == 0)
            {
                rb.MovePosition(rb.position + movementInput * movespeed * Time.fixedDeltaTime);
            }
        }
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
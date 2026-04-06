using UnityEngine;

public class OwnerPlayerCollision : MonoBehaviour
{
    private bool triggered = false;

    // if owner collider is TRIGGER
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        CaughtUI.Instance?.Show();
    }

    // if owner collider is NOT trigger
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (triggered) return;
        if (!other.gameObject.CompareTag("Player")) return;
        triggered = true;
        CaughtUI.Instance?.Show();
    }
}
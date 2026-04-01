using UnityEngine;

// put this on the cassette tape object sitting on the kitchen floor
// player walks up, presses E, tape gets added to inventory and disappears
public class CassetteTapePickup : Interactable
{
    // drag the CassetteTapeItem ScriptableObject asset here in the inspector
    // (or Level2SceneBuilder assigns it at runtime)
    public GameItem cassetteTapeItem;

    public override void Interact()
    {
        if (PuzzleState.HasCassetteTape) return; // already picked up, shouldn't be reachable

        // add to player inventory
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var inv = player.GetComponent<PlayerInventory>();
            if (inv != null && cassetteTapeItem != null)
                inv.AddItem(cassetteTapeItem);
        }

        PuzzleState.HasCassetteTape = true;

        InteractionPromptUI.Instance?.Hide();

        // show a quick pickup message then vanish
        DialogueManager.Instance?.StartDialogue("", new[] { "You found a cassette tape." }, DisableSelf);
    }

    // hides the tape from the world after the pickup dialogue closes
    private void DisableSelf()
    {
        // the tape visual is a child "Visual" object built by Level2SceneBuilder
        // so GetComponent<SpriteRenderer>() on the parent finds nothing - gotta go through children
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
            sr.enabled = false;

        // kill the trigger so the "press E" prompt doesn't keep popping up on an invisible object
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }
}

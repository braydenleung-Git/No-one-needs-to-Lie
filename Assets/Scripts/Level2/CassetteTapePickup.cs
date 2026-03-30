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

    // hides the tape from the world - call comes back as dialogue onComplete
    private void DisableSelf()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }
}

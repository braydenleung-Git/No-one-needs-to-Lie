using UnityEngine;

public class L1TransitionExitInteractable : Interactable
{
    [SerializeField] private GameObject policeHub;
    [SerializeField] private GameObject introScene;
    public override void Interact()
    {
        introScene.SetActive(false);
        policeHub.SetActive(true);
    }
}

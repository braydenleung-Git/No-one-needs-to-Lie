using UnityEngine;
using UnityEngine.InputSystem;

public class CardSwiperInteractable : Interactable
{
    [SerializeField] private CardSwiperUI cardSwiperUI;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private InputActionAsset inputActionAsset;

    private InputAction _escapeMenuAction;
    private InputActionMap _interactActionMap;
    protected override void Start()
    {
        base.Start();
        _interactActionMap = inputActionAsset.FindActionMap("Player");
        _escapeMenuAction = _interactActionMap.FindAction("EscapeMenu");
    }
    public override void Interact()
    {
        //enable the ui
        cardSwiperUI.gameObject.SetActive(true);
        //lock the player
        playerController.canMove = false;
    }

    protected override void Update()
    {
        base.Update();
        if (_escapeMenuAction.IsPressed())
        {
            cardSwiperUI.gameObject.SetActive(false);
            playerController.canMove = true;
        }
    }
}

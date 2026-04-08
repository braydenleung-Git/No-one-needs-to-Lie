using UnityEngine;
using UnityEngine.InputSystem;

// handles the whole level 1 intro cinematic
// locks the player, shows the customization screen first, then plays the animations
public class IntroController : MonoBehaviour
{
    [Header("Animators")]
    public Animator cameraAnimator;
    public Animator Text1Animator;
    public Animator Text2Animator;
    public Animator TextFinalAnimator;

    [Header("Player")]
    public PlayerController playerController;

    [Header("Customization")]
    // drag the CharacterCustomization object here in the inspector
    public CharacterCustomizationUI customizationUI;

    private int _index;
    private bool _isIntro;

    private void Start()
    {
        // lock movement immediately so the player can't run off during the UI
        playerController.canMove = false;

        if (customizationUI != null)
        {
            // show the hat picker first, then kick off the cinematic once they pick
            customizationUI.Show(StartIntro, playerController != null ? playerController.gameObject : null);
        }
        else
        {
            // no customization UI wired up, just go straight into the intro
            StartIntro();
        }
    }

    // called after the player picks hat or no hat
    void StartIntro()
    {
        _index = 1;
        _isIntro = true;
        Text1Animator.Play("Text1");
        cameraAnimator.Play("Tstart");
    }

    void Update()
    {
        bool isNotAnimating = !IsAnimating();

        // let the player press space to advance the cinematic
        if (_isIntro && Keyboard.current.spaceKey.wasPressedThisFrame && isNotAnimating)
        {
            AdvanceCinematic();
        }

        // once the whole intro is done, hand control back to the player
        if (!_isIntro && isNotAnimating)
        {
            TextFinalAnimator.enabled = false;
            Text1Animator.enabled = false;
            Text2Animator.enabled = false;
            cameraAnimator.Play("TransferControl");
            playerController.canMove = true;
        }
    }

    void AdvanceCinematic()
    {
        float animCheckInterval = 0.1f;
        float lastAnimCheckTime;
        switch (_index)
        {
            case 1:
                cameraAnimator.Play("T1-T2");
                lastAnimCheckTime = 0f;
                // wait for the animation to finish before playing the next text
                while (true)
                {
                    if (Time.time - lastAnimCheckTime >= animCheckInterval)
                    {
                        lastAnimCheckTime = Time.time;
                        if (!IsAnimating())
                        {
                            break;
                        }
                    }
                }
                Text2Animator.Play("Text2");
                break;
            case 2:
                cameraAnimator.Play("T2-Tfinal");
                lastAnimCheckTime = 0f;
                // same deal, wait for it
                while (true)
                {
                    if (Time.time - lastAnimCheckTime >= animCheckInterval)
                    {
                        lastAnimCheckTime = Time.time;
                        if (!IsAnimating())
                        {
                            break;
                        }
                    }
                }
                TextFinalAnimator.Play("TFinal");
                break;
        }
        if (_index == 2)
        {
            _isIntro = false;
        }
        _index++;
    }

    bool IsAnimating()
    {
        if (Text1Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        if (Text2Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        if (TextFinalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        return false;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class IntroController : MonoBehaviour
{
    [Header("Animators")]
    public Animator cameraAnimator;
    public Animator Text1Animator;
    public Animator Text2Animator;
    public Animator TextFinalAnimator;
    
    [Header("Player")]
    public PlayerController playerController;
    
    private int _index;
    private bool _isIntro;

    private void Start()
    {
        _index = 1;
        _isIntro = true;
        //lock player input until animation is over
        Text1Animator.Play("Text1");
        cameraAnimator.Play("Tstart");
        playerController.canMove = false;
    }

    void Update()
    {
        bool isNotAnimating = !IsAnimating();
        if (_isIntro && Keyboard.current.spaceKey.wasPressedThisFrame && isNotAnimating)
        {
            AdvanceCinematic();
        }

        //release the control of the player once the intro is over
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
                //wait for the animation to finish, through constantly asking at a certain interval
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
                //wait for the animation to finish, through constantly asking at a certain interval
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
        {
            return true;
        }

        if (Text2Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }
        if (TextFinalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }
        return false;
    }
}
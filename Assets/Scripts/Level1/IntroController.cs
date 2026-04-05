using UnityEngine;
using UnityEngine.InputSystem;

public class IntroController : MonoBehaviour
{
    [Header("Animators")]
    public Animator cameraAnimator;
    public Animator Text1Animator;
    public Animator Text2Animator;
    public Animator TextFinalAnimator;
    
    // [Header("Player")]
    // public PlayerController playerController;
    
    private int _index = 1;
    private bool _isIntro = true;

    private void Start()
    {
        //lock player input until animation is over
        Text1Animator.Play("Text1");
        // playerController.canMove = false;
    }

    void Update()
    {
        if (_isIntro && Keyboard.current.spaceKey.wasPressedThisFrame && !IsAnimating())
        {
            //lock player input
            AdvanceCinematic();
        }

        if (!_isIntro && !IsAnimating())
        {
            //release player lock input
        }
    }

    void AdvanceCinematic()
    {
        float animCheckInterval = 0.1f;
        float lastAnimCheckTime;
        if (_index == 2)
        {
            _isIntro = false;
        }
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
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroController : MonoBehaviour
{
    [Header("Animators")]
    public Animator Text1Animator;
    public Animator Text2Animator;
    public Animator TextFinalAnimator;
    
    [Header("Player")]
    public PlayerController playerController;
    
    [Header("Police Hub")]
    public GameObject enterPoliceHub;
    
    
    private int _index;
    private bool _isIntro;
    private Animator _cameraAnimator;

    private void Start()
    {
        _cameraAnimator = gameObject.GetComponent<Animator>();
        _index = 1;
        _isIntro = true;
        //lock player input until animation is over
        Text1Animator.Play("Text1");
        _cameraAnimator.Play("Tstart");
        playerController.canMove = false;
        ExperienceManager.Instance.gameObject.SetActive(false);
    }

    void Update() 
    {
        bool isNotAnimating = !IsAnimating();
        if (_isIntro && Keyboard.current.spaceKey.wasPressedThisFrame && isNotAnimating)
        {
            AdvanceCinematic();
        }

        if (_index == 3)
        {
            _isIntro = false;
        }
        //release the control of the player once the intro is over
        if (!_isIntro && isNotAnimating)
        {
            Text1Animator.enabled = false;
            Text2Animator.enabled = false;
            TextFinalAnimator.enabled = false;
            _cameraAnimator.Play("TransferControl");
            playerController.canMove = true;
            playerController.gameObject.transform.parent = null;
            StartDelayedCoroutine(60);
        }
    }

    void AdvanceCinematic()
    {
        float animCheckInterval = 0.1f;
        float lastAnimCheckTime;
        switch (_index)
        {
            case 1:
                _cameraAnimator.Play("T1-T2");
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
                _cameraAnimator.Play("T2-Tfinal");
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
        if (_cameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }
        return false;
    }

    public void StartDelayedCoroutine(float delay)
    {
        StartCoroutine(DelayedCoroutine(delay));
    }

    private IEnumerator DelayedCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Enable the EnterPoliceHub GameObject
        if (enterPoliceHub != null)
        {
            enterPoliceHub.SetActive(true);
            ExperienceManager.Instance.gameObject.SetActive(true);
        }
    }
}
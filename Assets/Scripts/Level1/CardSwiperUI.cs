using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardSwiperUI : MonoBehaviour
{
    private BoxCollider2D[] _checkpoints;
    private BoxCollider2D[] _barriers;
    private SwipeCard _swipeCard;

    private bool _puzzleFailed;
    private TextMeshPro _prompterTextObj;

    private int _checkpointIndex;
    private float _lastCheckPointTime;
    [SerializeField]private float checkPointInterval = 0.8f;
    [SerializeField]private float checkPointSensitivity = 0.2f;
    
    private Vector3 _swipeCardOriginalPosition;
    public Vector3 swipeCardOriginalPosition {get => _swipeCardOriginalPosition;}
    
    private void OnEnable()
    {
        //getting all the trigger pts 
        _checkpoints = gameObject.GetComponentsInChildren<BoxCollider2D>(true)
            .Where(boxCollider2D => !boxCollider2D.gameObject.CompareTag("Barrier"))
            .ToArray();
        
        //getting all the barriers
        _barriers = gameObject.GetComponentsInChildren<BoxCollider2D>(true)
            .Where(boxCollider2D => boxCollider2D.gameObject.CompareTag("Barrier"))
            .ToArray();
        _prompterTextObj = gameObject.GetComponentInChildren<TextMeshPro>();
        _swipeCard = gameObject.GetComponentInChildren<SwipeCard>();
        //teleport the card to relative position
        _swipeCardOriginalPosition = _swipeCard.transform.position;
        _prompterTextObj.text = "SwipeCard";
        _checkpointIndex = 0;
        _lastCheckPointTime = 0f;
    }
    
    public void ReportTrigger(GameObject trigger)
    {
        //exit if it is not equal to the checkpoint that is expected
        if (!_checkpoints[_checkpointIndex].gameObject.Equals(trigger)) return;
        //exit if it is too slow
        var speedTest = IsSwipeSpeedGood();
        if (speedTest[0]==speedTest[1])
        {
            _checkpointIndex++;
        }
        else
        {
            if (!_puzzleFailed)
            {
                _puzzleFailed = true;
                //flicker issue may occur
                if (speedTest[0])
                {
                    _prompterTextObj.text = "Too Fast";
                }

                if (speedTest[1])
                {
                    _prompterTextObj.text = "Too Slow";
                }
            }
        }

        if (!_puzzleFailed && _checkpointIndex == _checkpoints.Length - 2)
        {
            PuzzleCompleted();
        }
    }
    
    //release, return to start 
    public void ReportAreaTrigger(bool isExit)
    {
        var enableStatus = !isExit;
        foreach (var trigger in _checkpoints)
        {
            trigger.gameObject.SetActive(enableStatus);
        }

        foreach (var barrier in _barriers)
        {
            barrier.gameObject.SetActive(enableStatus);
        }
    }
    

    private void PuzzleCompleted()
    {
        //do something
        Debug.Log("Puzzle Completed");
        gameObject.SetActive(false);
    }

    private bool[] IsSwipeSpeedGood()
    {
        var fast = Time.time - _lastCheckPointTime <= checkPointInterval + checkPointSensitivity;
        var slow = Time.time - _lastCheckPointTime >= checkPointInterval - checkPointSensitivity;
        return new[] {fast, slow};
    }
}   


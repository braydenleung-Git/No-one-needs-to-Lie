using System;
using UnityEngine;

public class CardSwiper : Interactable
{
    private BoxCollider2D[] checkpoints;

    private int checkpointIndex = 0;

    private float lastCheckPointTime;
    [SerializeField]private float checkPointInterval = 0.1f;
    
    
    private void OnEnable()
    {
        
    }
    protected override void Start()
    {
        //initalized by getting child 
        checkpoints = gameObject.GetComponentsInChildren<BoxCollider2D>(true);
    }

    protected override void Update()
    {
        base.Update();
    }
    public override void Interact()
    {
        //Show card swiper puzzle
    }

    public void ReportTrigger(GameObject trigger, bool isExit)
    {
        //exit if it is not equal to the checkpoint that is expected
        if (!checkpoints[checkpointIndex].gameObject.Equals(trigger)) return;
        
        if (isExit) checkpointIndex++;
    }
    
    
    //release, return to start 
    //left, disable collider

    private void PuzzleCompleted()
    {
        gameObject.SetActive(false);
    }
}   


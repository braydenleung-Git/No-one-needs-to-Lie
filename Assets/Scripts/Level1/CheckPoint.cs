using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInParent<CardSwiper>().ReportTrigger(gameObject, true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        gameObject.GetComponentInParent<CardSwiper>().ReportTrigger(gameObject, false);
    }
}

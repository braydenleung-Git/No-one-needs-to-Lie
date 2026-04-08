using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<CardSwiperUI>().ReportAreaTrigger(false);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<CardSwiperUI>().ReportAreaTrigger(true);
        }
    }
}

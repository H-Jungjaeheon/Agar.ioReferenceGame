using UnityEngine;

public class Range : MonoBehaviour
{
    [SerializeField]
    [Tooltip("플레이어 판별 콜라이더")]
    private CircleCollider2D rangeCollider;

    [SerializeField]
    [Tooltip("적 컴포넌트")]
    private Enemy eComponent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            eComponent.player = collision.gameObject;

            eComponent.ChaseEvent(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && eComponent.player != null)
        {
            eComponent.player = null;

            eComponent.ChaseEvent(true);
        }
    }
}

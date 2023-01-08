using UnityEngine;

public class Range : MonoBehaviour
{
    [SerializeField]
    [Tooltip("�÷��̾� �Ǻ� �ݶ��̴�")]
    private CircleCollider2D rangeCollider;

    [SerializeField]
    [Tooltip("�� ������Ʈ")]
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

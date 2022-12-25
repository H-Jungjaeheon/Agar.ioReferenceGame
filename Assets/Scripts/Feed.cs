using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{
    [SerializeField]
    [Tooltip("ũ�⸦ Ű���ִ� ����")]
    private float Value;

    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.Size += Value;
            player.speed -= 0.5f;

            Destroy(gameObject); //������Ʈ Ǯ������ ��ü
        }
    }
}

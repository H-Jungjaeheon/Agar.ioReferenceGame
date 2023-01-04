using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{
    [SerializeField]
    [Tooltip("ũ�⸦ Ű���ִ� ����")]
    private float Value;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.Size += Value;
            player.speed -= 0.05f;

            player.cam.orthographicSize += 0.05f;

            Destroy(gameObject); //������Ʈ Ǯ������ ��ü
        }
    }
}

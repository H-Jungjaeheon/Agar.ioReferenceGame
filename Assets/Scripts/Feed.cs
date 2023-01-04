using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{
    [SerializeField]
    [Tooltip("크기를 키워주는 정도")]
    private float Value;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.Size += Value;
            player.speed -= 0.05f;

            player.cam.orthographicSize += 0.05f;

            Destroy(gameObject); //오브젝트 풀링으로 교체
        }
    }
}

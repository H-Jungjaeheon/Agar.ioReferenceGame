using UnityEngine;

public class Feed : MonoBehaviour
{
    [SerializeField]
    [Tooltip("ũ�⸦ Ű���ִ� ����")]
    private float Value;

    private ObjectPool objectPool;

    private SpawnManager spawnManager;

    void Start()
    {
        spawnManager = SpawnManager.instance;
        objectPool = ObjectPool.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            player.Size += Value;

            if (player.speed > 1)
            {
                player.speed -= 0.05f;

                if (player.speed < 1)
                {
                    player.speed = 1;
                }
            }

            player.cam.orthographicSize += 0.04f;

            spawnManager.nowFeedCount--;

            spawnManager.AdditionalFeedSpawn();

            objectPool.ReturnObject(gameObject, ObjectKind.FeedObj);
        }
    }
}

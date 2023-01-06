using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    ObjectPool poolInstance;

    GameObject nowFeedObj;

    Vector2 spawnPos = new Vector2(0, 0);

    Collider2D collider;

    private const string wallTag = "Wall";

    private const string feedTag = "Feed";

    private const string playerTag = "Player";

    void Start()
    {
        poolInstance = ObjectPool.instance;

        for (int nowSpawnIndex = 0; nowSpawnIndex < 200; nowSpawnIndex++)
        {
            nowFeedObj = poolInstance.GetObject(ObjectKind.FeedObj);

            while (true)
            {
                spawnPos.x = Random.Range(-86, 86);
                spawnPos.y = Random.Range(-86, 86);

                collider = Physics2D.OverlapCircle(spawnPos, 0.25f);

                if (collider == null)
                {
                    nowFeedObj.transform.localPosition = spawnPos;
                    break;
                }
            }
        }
    }
}

using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    ObjectPool poolInstance;

    GameObject nowFeedObj;

    Vector2 spawnPos = new Vector2(0, 0);

    Collider2D collider;

    public int nowFeedCount = 200;

    private const string wallTag = "Wall";

    private const string feedTag = "Feed";

    private const string playerTag = "Player";

    void Start()
    {
        StartSetting();
    }

    void StartSetting()
    {
        if (instance == null)
        {
            instance = this;
        }

        poolInstance = ObjectPool.instance;

        BasicFeedSpawn();
    }

    void BasicFeedSpawn()
    {
        for (int nowSpawnIndex = 0; nowSpawnIndex < 200; nowSpawnIndex++)
        {
            nowFeedObj = poolInstance.GetObject(ObjectKind.FeedObj);

            while (true)
            {
                spawnPos.x = Random.Range(-86, 86);
                spawnPos.y = Random.Range(-86, 86);

                collider = Physics2D.OverlapCircle(spawnPos, 1f);

                if (collider == null)
                {
                    nowFeedObj.transform.localPosition = spawnPos;
                    break;
                }
            }
        }
    }

    public void AdditionalFeedSpawn()
    {
        while (nowFeedCount < 200)
        {
            nowFeedObj = poolInstance.GetObject(ObjectKind.FeedObj);

            while (true)
            {
                spawnPos.x = Random.Range(-86, 86);
                spawnPos.y = Random.Range(-86, 86);

                collider = Physics2D.OverlapCircle(spawnPos, 1f);

                if (collider == null)
                {
                    nowFeedObj.transform.localPosition = spawnPos;
                    break;
                }
            }

            //if (nowFeedCount < 200)
            //{
                
            //}

            nowFeedCount++;
        }
    }
}

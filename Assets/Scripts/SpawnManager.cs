using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    ObjectPool poolInstance;

    GameObject nowFeedObj;

    Vector2 spawnPos = new Vector2(0f, 0f);

    private const string wallTag = "Wall";

    private const string feedTag = "Feed";

    private const string playerTag = "Player";

    void Start()
    {
        bool isEmpty;

        poolInstance = ObjectPool.instance;

        for (int nowSpawnIndex = 0; nowSpawnIndex < 100; nowSpawnIndex++)
        {
            isEmpty = false;

            //while (true)
            //{
                spawnPos.x = Random.Range(-86, 86);
                spawnPos.y = Random.Range(-86, 86);

                //foreach (Collider2D collider in Physics2D.OverlapCircleAll(spawnPos, 0.25f))
                //{
                //    if (collider.gameObject.CompareTag(wallTag) || collider.gameObject.CompareTag(feedTag) || collider.gameObject.CompareTag(playerTag))
                //    {
                //        isEmpty = false;
                //    }
                //    else
                //    {
                //        isEmpty = true;
                //    }
                //}

                //if (isEmpty)
                //{
                //    break;
                //}
            //}

            nowFeedObj = poolInstance.GetObject(ObjectKind.FeedObj);
            nowFeedObj.transform.localPosition = spawnPos;
        }
    }
}

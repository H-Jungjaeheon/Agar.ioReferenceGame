using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectKind
{
    FeedObj,
    EnemyObj
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField]
    [Tooltip("������Ʈ Ǯ ����� ���� ������ ������Ʈ��")]
    private GameObject[] usePrefabObjs;

    Dictionary<int, Queue<GameObject>> objectPool = new Dictionary<int, Queue<GameObject>>();

    void Start()
    {
        StartSetting();
    }

    private void StartSetting()
    {
        if (instance == null)
        {
            instance = this;
        }

        BasicSpawn();
    }

    private void BasicSpawn()
    {
        int spawnCount = 0;

        for (int nowIndex = 0; nowIndex < usePrefabObjs.Length; nowIndex++)
        {
            switch ((ObjectKind)spawnCount)
            {
                case ObjectKind.FeedObj:
                    spawnCount = 100;
                    break;
                case ObjectKind.EnemyObj:
                    spawnCount = 50;
                    break;
            }

            for (int nowSpawnCount = 0; nowSpawnCount < spawnCount; nowSpawnCount++)
            {
                
            }
        }
    }
}

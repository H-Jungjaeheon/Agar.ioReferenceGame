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

    void Awake()
    {
        StartSetting();
    }

    private void StartSetting()
    {
        instance = this;
        if (instance == null)
        {
        }

        BasicSpawn();
    }

    /// <summary>
    /// ���� �� ��� ������Ʈ�� �⺻������ �����ϴ� �Լ�(������ �ּ� ������ŭ ����)
    /// </summary>
    private void BasicSpawn()
    {
        ObjectKind nowObjKind;
        int spawnCount = 0;

        for (int nowIndex = 0; nowIndex < usePrefabObjs.Length; nowIndex++)
        {
            switch ((ObjectKind)nowIndex)
            {
                case ObjectKind.FeedObj:
                    spawnCount = 200;
                    break;
                case ObjectKind.EnemyObj:
                    spawnCount = 20;
                    break;
            }

            nowObjKind = (ObjectKind)nowIndex;

            objectPool.Add(nowIndex, new Queue<GameObject>());

            for (int nowPoolIndex = 0; nowPoolIndex < spawnCount; nowPoolIndex++)
            {
                objectPool[nowIndex].Enqueue(CreateNewObjs(nowObjKind));
            }
        }
    }

    /// <summary>
    /// ������Ʈ ���� �Լ�
    /// </summary>
    /// <param name="objKind"> ������ ������Ʈ ���� </param>
    /// <returns></returns>
    private GameObject CreateNewObjs(ObjectKind objKind) 
    {
        GameObject newCreateObj = Instantiate(usePrefabObjs[(int)objKind], transform);
        newCreateObj.gameObject.SetActive(false);

        return newCreateObj;
    }

    /// <summary>
    /// ������Ʈ�� ������ �������� ���ٸ� ���� �����ؼ� �������� �Լ�
    /// </summary>
    /// <param name="objKind"> ������ ������Ʈ ���� </param>
    /// <returns></returns>
    public GameObject GetObject(ObjectKind objKind)
    {
        int objIndex = (int)objKind;

        if (objectPool[objIndex].Count > 0)
        {
            GameObject obj = objectPool[objIndex].Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);

            return obj;
        }
        else
        {
            GameObject newObj = CreateNewObjs(objKind);
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);

            return newObj;
        }
    }

    /// <summary>
    /// ����� ���� ������Ʈ Ǯ�� ��ȯ
    /// </summary>
    /// <param name="usedObj"> ���� ��� �Ϸ�� �ش� ������Ʈ </param>
    /// <param name="objIndex"> �ش� ������Ʈ�� ���� </param>
    public void ReturnObject(GameObject usedObj, ObjectKind objKind)
    {
        usedObj.SetActive(false);
        usedObj.transform.SetParent(transform);
        objectPool[(int)objKind].Enqueue(usedObj);
    }
}

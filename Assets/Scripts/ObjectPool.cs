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
    [Tooltip("오브젝트 풀 사용을 위한 프리펩 오브젝트들")]
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
    /// 시작 시 모든 오브젝트를 기본적으로 생성하는 함수(설정한 최소 수량만큼 생성)
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
    /// 오브젝트 생성 함수
    /// </summary>
    /// <param name="objKind"> 생성할 오브젝트 종류 </param>
    /// <returns></returns>
    private GameObject CreateNewObjs(ObjectKind objKind) 
    {
        GameObject newCreateObj = Instantiate(usePrefabObjs[(int)objKind], transform);
        newCreateObj.gameObject.SetActive(false);

        return newCreateObj;
    }

    /// <summary>
    /// 오브젝트가 있으면 꺼내가고 없다면 새로 생성해서 가져가는 함수
    /// </summary>
    /// <param name="objKind"> 꺼내갈 오브젝트 종류 </param>
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
    /// 사용이 끝난 오브젝트 풀로 반환
    /// </summary>
    /// <param name="usedObj"> 현재 사용 완료된 해당 오브젝트 </param>
    /// <param name="objIndex"> 해당 오브젝트의 종류 </param>
    public void ReturnObject(GameObject usedObj, ObjectKind objKind)
    {
        usedObj.SetActive(false);
        usedObj.transform.SetParent(transform);
        objectPool[(int)objKind].Enqueue(usedObj);
    }
}

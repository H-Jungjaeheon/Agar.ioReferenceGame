using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Node
{
    public Node(bool _isWall, int nodeXPos, int nodeYPos) //��� ������ (�� �Ǻ�, x��ġ��, y��ġ�� �Ű������� �ʱ�ȭ)
    {
        isWall = _isWall;
        xPos = nodeXPos;
        yPos = nodeYPos;
    }

    [Tooltip("���� ��尡(��� ��ġ��) ������ �Ǻ�")]
    public bool isWall;

    [Tooltip("�θ�(�� ��ġ ���) ��� ����")]
    public Node ParentNode;

    [Tooltip("�ش� ��� X��ǥ")]
    public int xPos;

    [Tooltip("�ش� ��� Y��ǥ")]
    public int yPos;

    [Tooltip("�������κ��� �̵��ߴ� �Ÿ�(��ĭor�밢�� ��ĭ ��ŭ ���밪���� ����)")]
    public int sDistance;

    [Tooltip("��ֹ� �����Ͽ� ��ǥ������ �Ÿ�(���� + ����)")]
    public int tDistance;
    
    public int priorityScore //�̵� �켱���� ���� (priorityScore : sDistance + tDistance)
    {
        get
        {
            return sDistance + tDistance;
        }
    } 
}

public class Player : MonoBehaviour
{
    public static Player instance = null;

    [Tooltip("�̵��ӵ�")]
    public float speed;

    float size; //���� ũ��

    public float Size
    {
        get { return size; }
        set
        {
            if (value < 1.55f)
            {
                size = value;

                sizeVector.x = size;
                sizeVector.y = size;
            }
            else
            {
                sizeVector.x = 1.55f;
                sizeVector.y = 1.55f;
            }

            transform.localScale = sizeVector;
        }
    }

    Vector3 sizeVector = new Vector3(0f, 0f, 1f); //ũ�� ������ ����

    #region ��ã�� �˰��� ���� ����
    [Header("��ã�� �˰��� ���� ����")]

    [Tooltip("���� ������ �ִ� ��ǥ��(���� x, y)")]
    public Vector2Int bottomLeft;

    [Tooltip("���� ������ �ִ� ��ǥ��(��� x, y)")]
    public Vector2Int topRight;

    [Tooltip("���� ������(���� �÷��̾� ������)")]
    public Vector2Int startPos;

    [Tooltip("��ǥ ������")]
    public Vector3 targetPos;

    [Tooltip("���� �� ��� ����Ʈ")]
    public List<Node> FinalNodeList;

    int sizeX; //���� ��ü Ž�� ������ X��

    int sizeY; //���� ��ü Ž�� ������ Y��

    Node[,] NodeArray; //���� ��ü ��� Ž�� ����(2���� ��� �迭)

    Node StartNode; //����(�����) ���

    Node TargetNode; //��ǥ(������) ���

    Node CurNode; //���� ��ġ ���

    List<Node> OpenList; //���� ���� ���� ����� ����

    List<Node> ClosedList; //���� �Ϸ�� ����� ����
    #endregion

    IEnumerator movingCoroutine;

    [Tooltip("���� ī�޶� ������Ʈ")]
    public Camera cam;

    #region �÷��̾� ���� ���� ���� ����
    [Header("�÷��̾� ���� ���� ���� ����")]

    [SerializeField]
    [Tooltip("���� ������ �ݶ��̴� ����")]
    private BoxCollider2D[] spawnColliders;

    [Tooltip("���� ���� ���� �ε���(���� �̱�)")]
    private int nowSpawnIndex;
    #endregion

    private void Start()
    {
        Init();
        RandSpawn();
    }

    private void OnEnable()
    {
        RandSpawn();
    }

    private void Update()
    {
        MouseInput();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
        }

        size = 0.15f;
    }

    private void RandSpawn()
    {
        Vector3 nowSpawnPos; //���� ���� ���� ���� ��� ������Ʈ ��ġ
        BoxCollider2D nowSpawnCollider; //���� ���� ���� ���� ��� �ݶ��̴�

        nowSpawnIndex = Random.Range(0, 4);

        nowSpawnCollider = spawnColliders[nowSpawnIndex];
        nowSpawnPos = nowSpawnCollider.transform.position;

        //���� ���� ���� ���� ��� �ݶ��̴� ����(X, Y)
        float range_X = nowSpawnCollider.bounds.size.x; 
        float range_Y = nowSpawnCollider.bounds.size.y;

        range_X = Random.Range((range_X / 2) * -1, range_X / 2);
        range_Y = Random.Range((range_Y / 2) * -1, range_Y / 2);

        transform.position = nowSpawnPos + new Vector3(range_X, range_Y, 0);
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }

            startPos = Vector2Int.RoundToInt(transform.position);
            targetPos = Camera.main.ScreenToWorldPoint(new Vector3Int((int)Input.mousePosition.x, (int)Input.mousePosition.y, 10));
            
            PathFinding();
        }
    }

    public void PathFinding() //��ã�� ���� �Լ�
    {
        #region ���� ��ġ �������� �ִ� �̵� ���� �缳��(ī�޶� ���� ����, ī�޶� ��ġ�� ũ�⿡ ����ؼ� �����ϵ��� �����ϱ�)
        bottomLeft.x = (int)(transform.position.x - (9 + cam.orthographicSize));
        bottomLeft.y = (int)(transform.position.y - (5 + cam.orthographicSize));

        topRight.x = (int)(transform.position.x + (9 + cam.orthographicSize));
        topRight.y = (int)(transform.position.y + (5 + cam.orthographicSize));
        #endregion

        // NodeArray�� ũ�� �����ֱ�(���� ��ã�� ��ü ����, 1 ���ϴ� ������ 0��ǥ�� �����ϱ� ����)
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;

        NodeArray = new Node[sizeX, sizeY]; //���� Ž�� ���� ����

        for (int i = 0; i < sizeX; i++) //��ü ������ Ž���ؼ� �� ã�� (0,0 ~ sizeX-1, sizeY-1)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), (transform.localScale.x / 2))) //��ǥ�� ���ư��� �� �Ǻ��� ���� �׸���(�� ĭ���� ���� ������ŭ �� ����)
                {
                    if (collider.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y); //�� ��ǥ�� ��� ���� : isWall, x, y ���� (ȭ���� ���� �Ʒ� ��� ~ ȭ���� ������ �� ������)
            }
        }

        // ���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];

        TargetNode = NodeArray[(int)targetPos.x - bottomLeft.x, (int)targetPos.y - bottomLeft.y];

        //��ã�� ������ �� ��������Ʈ�� �������� ��� �ֱ�, �� ���� �ߺ� ���� �ʱ�ȭ ����
        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();

        while (OpenList.Count > 0)
        {
            //��������Ʈ ��� �� F�� ���� ������ �ش� ��带 ���� ���� ���� ����
            //F�� ���ٸ� H�� ���� �� ���� ���� ���� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��(H�� ���ٸ� 0��° ��带 ���� ���� ���� ����)
            CurNode = OpenList[0];

            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].priorityScore <= CurNode.priorityScore && OpenList[i].tDistance < CurNode.tDistance)
                {
                    CurNode = OpenList[i];
                }
            }

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);


            // ������
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;

                while (TargetCurNode != StartNode)
                {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }

                FinalNodeList.Add(StartNode);
                FinalNodeList.Reverse(); //��������, 0��°���� ���������� �� ã�ư����� �ϱ�

                movingCoroutine = Moving();
                StartCoroutine(movingCoroutine);

                break;
            }

            // �֢آע�
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos - 1);
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos - 1);

            // �� �� �� ��
            OpenListAdd(CurNode.xPos, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos);
            OpenListAdd(CurNode.xPos, CurNode.yPos - 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // �����¿� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !ClosedList.Contains(NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
            {
                return;
            }

            if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
            {
                return;
            }

            // �̿���忡 �ְ�, ������ 10, �밢���� 14��ŭ�� �Ÿ� ����
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.sDistance + (CurNode.xPos - checkX == 0 || CurNode.yPos - checkY == 0 ? 10 : 14);

            // �̵������ �̿���� sDistance���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� sDistance, tDistance, ParentNode�� ���� �� ��������Ʈ�� �߰�
            if (MoveCost < NeighborNode.sDistance || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.sDistance = MoveCost;
                NeighborNode.tDistance = (Mathf.Abs(NeighborNode.xPos - TargetNode.xPos) + Mathf.Abs(NeighborNode.yPos - TargetNode.yPos)) * 10;
                NeighborNode.ParentNode = CurNode;

                OpenList.Add(NeighborNode);
            }
        }
    }

    IEnumerator Moving()
    {
        Vector2 targetPos;

        for (int nowIndex = 0; nowIndex < FinalNodeList.Count; nowIndex++)
        {
            targetPos.x = FinalNodeList[nowIndex].xPos;
            targetPos.y = FinalNodeList[nowIndex].yPos;

            while (true)
            {
                if (transform.position.x == targetPos.x && transform.position.y == targetPos.y)
                {
                    break;
                }

                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);

                yield return null;
            }
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene(1);
    }

    void OnDrawGizmos()
    {
        if (FinalNodeList.Count != 0)
        {
            for (int i = 0; i < FinalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].xPos, FinalNodeList[i].yPos), new Vector2(FinalNodeList[i + 1].xPos, FinalNodeList[i + 1].yPos));
            }
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.gameObject.GetComponent<IInteraction>().Interaction();
    //}
}

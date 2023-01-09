using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Node
{
    public Node(bool _isWall, int nodeXPos, int nodeYPos) //노드 생성자 (벽 판별, x위치값, y위치값 매개변수로 초기화)
    {
        isWall = _isWall;
        xPos = nodeXPos;
        yPos = nodeYPos;
    }

    [Tooltip("현재 노드가(노드 위치가) 벽인지 판별")]
    public bool isWall;

    [Tooltip("부모(전 위치 노드) 노드 저장")]
    public Node ParentNode;

    [Tooltip("해당 노드 X좌표")]
    public int xPos;

    [Tooltip("해당 노드 Y좌표")]
    public int yPos;

    [Tooltip("시작으로부터 이동했던 거리(한칸or대각선 한칸 만큼 절대값으로 더함)")]
    public int sDistance;

    [Tooltip("장애물 무시하여 목표까지의 거리(가로 + 세로)")]
    public int tDistance;
    
    public int priorityScore //이동 우선순위 점수 (priorityScore : sDistance + tDistance)
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

    [Tooltip("이동속도")]
    public float speed;

    float size; //현재 크기

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

    Vector3 sizeVector = new Vector3(0f, 0f, 1f); //크기 조정용 벡터

    #region 길찾기 알고리즘 변수 모음
    [Header("길찾기 알고리즘 변수 모음")]

    [Tooltip("측정 가능한 최대 좌표값(음수 x, y)")]
    public Vector2Int bottomLeft;

    [Tooltip("측정 가능한 최대 좌표값(양수 x, y)")]
    public Vector2Int topRight;

    [Tooltip("시작 포지션(현재 플레이어 포지션)")]
    public Vector2Int startPos;

    [Tooltip("목표 포지션")]
    public Vector3 targetPos;

    [Tooltip("최종 길 노드 리스트")]
    public List<Node> FinalNodeList;

    int sizeX; //현재 전체 탐색 범위의 X값

    int sizeY; //현재 전체 탐색 범위의 Y값

    Node[,] NodeArray; //현재 전체 경로 탐색 범위(2차원 노드 배열)

    Node StartNode; //시작(출발점) 노드

    Node TargetNode; //목표(도착점) 노드

    Node CurNode; //현재 위치 노드

    List<Node> OpenList; //현재 진행 가능 경로의 노드들

    List<Node> ClosedList; //진행 완료된 경로의 노드들
    #endregion

    IEnumerator movingCoroutine;

    [Tooltip("메인 카메라 컴포넌트")]
    public Camera cam;

    #region 플레이어 스폰 관련 변수 모음
    [Header("플레이어 스폰 관련 변수 모음")]

    [SerializeField]
    [Tooltip("스폰 지점들 콜라이더 모음")]
    private BoxCollider2D[] spawnColliders;

    [Tooltip("현재 스폰 지점 인덱스(랜덤 뽑기)")]
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
        Vector3 nowSpawnPos; //현재 뽑힌 랜덤 스폰 장소 오브젝트 위치
        BoxCollider2D nowSpawnCollider; //현재 뽑힌 랜덤 스폰 장소 콜라이더

        nowSpawnIndex = Random.Range(0, 4);

        nowSpawnCollider = spawnColliders[nowSpawnIndex];
        nowSpawnPos = nowSpawnCollider.transform.position;

        //현재 뽑힌 랜덤 스폰 장소 콜라이더 범위(X, Y)
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

    public void PathFinding() //길찾기 시작 함수
    {
        #region 현재 위치 기준으로 최대 이동 범위 재설정(카메라 범위 기준, 카메라 비치는 크기에 비례해서 변경하도록 수정하기)
        bottomLeft.x = (int)(transform.position.x - (9 + cam.orthographicSize));
        bottomLeft.y = (int)(transform.position.y - (5 + cam.orthographicSize));

        topRight.x = (int)(transform.position.x + (9 + cam.orthographicSize));
        topRight.y = (int)(transform.position.y + (5 + cam.orthographicSize));
        #endregion

        // NodeArray의 크기 정해주기(현재 길찾기 전체 범위, 1 더하는 이유는 0좌표를 포함하기 위함)
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;

        NodeArray = new Node[sizeX, sizeY]; //현재 탐색 범위 설정

        for (int i = 0; i < sizeX; i++) //전체 범위를 탐색해서 벽 찾기 (0,0 ~ sizeX-1, sizeY-1)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), (transform.localScale.x / 2))) //좌표를 돌아가며 벽 판별용 원을 그린다(한 칸으로 잡은 범위만큼 원 생성)
                {
                    if (collider.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y); //각 좌표의 노드 설정 : isWall, x, y 대입 (화면의 왼쪽 아래 노드 ~ 화면의 오른쪽 위 노드까지)
            }
        }

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];

        TargetNode = NodeArray[(int)targetPos.x - bottomLeft.x, (int)targetPos.y - bottomLeft.y];

        //길찾기 시작할 때 열린리스트에 시작지점 노드 넣기, 각 노드들 중복 방지 초기화 선언
        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();

        while (OpenList.Count > 0)
        {
            //열린리스트 노드 중 F가 가장 작으면 해당 노드를 현재 방향 노드로 지정
            //F가 같다면 H가 작은 걸 현재 방향 노드로 하고 열린리스트에서 닫힌리스트로 옮기기(H도 같다면 0번째 노드를 현재 방향 노드로 지정)
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


            // 마지막
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;

                while (TargetCurNode != StartNode)
                {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }

                FinalNodeList.Add(StartNode);
                FinalNodeList.Reverse(); //반전시켜, 0번째부터 순차적으로 길 찾아가도록 하기

                movingCoroutine = Moving();
                StartCoroutine(movingCoroutine);

                break;
            }

            // ↗↖↙↘
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos - 1);
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos - 1);

            // ↑ → ↓ ←
            OpenListAdd(CurNode.xPos, CurNode.yPos + 1);
            OpenListAdd(CurNode.xPos + 1, CurNode.yPos);
            OpenListAdd(CurNode.xPos, CurNode.yPos - 1);
            OpenListAdd(CurNode.xPos - 1, CurNode.yPos);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
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

            // 이웃노드에 넣고, 직선은 10, 대각선은 14만큼의 거리 지정
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.sDistance + (CurNode.xPos - checkX == 0 || CurNode.yPos - checkY == 0 ? 10 : 14);

            // 이동비용이 이웃노드 sDistance보다 작거나 또는 열린리스트에 이웃노드가 없다면 sDistance, tDistance, ParentNode를 설정 후 열린리스트에 추가
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

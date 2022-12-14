using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
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

    [Tooltip("사거리 내 플레이어")]
    public GameObject player;

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

    IEnumerator moving;

    IEnumerator targetReSetting;

    WaitForSeconds reSettingDelay = new WaitForSeconds(3f);

    private void Start()
    {
        Init();
    }

    private void Init()
    { 
        size = 0.15f;
    }

    public void ChaseEvent(bool isEscapeRange)
    {
        if(isEscapeRange == false)
        {
            targetReSetting = TargetReSetting();
            StartCoroutine(targetReSetting);
        }
        else if (isEscapeRange)
        {
            if (moving != null)
            {
                StopCoroutine(moving);
            }

            if (targetReSetting != null)
            {
                StopCoroutine(targetReSetting);
            }
        }
    }

    private IEnumerator TargetReSetting()
    {
        while (true)
        {
            startPos = Vector2Int.RoundToInt(transform.position);
            targetPos = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 10);

            PathFinding();

            yield return reSettingDelay;
        }
    }

    public void PathFinding() //길찾기 시작 함수
    {  
        bottomLeft.x = (int)transform.position.x - 20;
        bottomLeft.y = (int)transform.position.y - 15;

        topRight.x = (int)transform.position.x + 20;
        topRight.y = (int)transform.position.y + 15;

        // NodeArray의 크기 정해주기(현재 길찾기 전체 범위, 1 더하는 이유는 0좌표를 포함하기 위함)
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;

        NodeArray = new Node[sizeX, sizeY]; //현재 탐색 범위 설정

        for (int i = 0; i < sizeX; i++) //전체 범위를 탐색해서 벽 찾기 (0,0 ~ sizeX-1, sizeY-1)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), transform.localScale.x / 2)) //좌표를 돌아가며 벽 판별용 원을 그린다(한 칸으로 잡은 범위만큼 원 생성)
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

                moving = Moving();
                StartCoroutine(moving);

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerComponent = player.GetComponent<Player>();

            if (collision.gameObject.transform.localScale.x > gameObject.transform.localScale.x)
            {
                playerComponent.Size += 0.1f;

                if (playerComponent.speed > 1)
                {
                    playerComponent.speed -= 0.5f;

                    if (playerComponent.speed < 1)
                    {
                        playerComponent.speed = 1;
                    }
                }

                playerComponent.cam.orthographicSize += 0.4f;

                Destroy(gameObject); //오브젝트 풀로 변경
            }
            else
            {
                playerComponent.GameOver();
            }
        }

    }
}

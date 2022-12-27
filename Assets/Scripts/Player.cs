using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; } //노드 생성자 (벽 판별, x위치값, y위치값 매개변수로 초기화)

    public bool isWall; //현재 노드가 벽인지 판별

    public Node ParentNode; //부모 노드 저장(전 노드)

    //x, y : 노드 좌표, G : 시작으로부터 이동했던 거리, H : 가로 + 세로 장애물 무시하여 목표까지의 거리, F : G + H
    public int x, y, G, H;
    public int F { get { return G + H; } }
}

public class Player : MonoBehaviour
{
    public static Player instance = null;

    [Tooltip("이동속도")]
    public float speed;

    [Tooltip("현재 크기")]
    private float size;

    public float Size
    {
        get { return size; }
        set
        {
            size = value;

            sizeVector.x = size;
            sizeVector.y = size;
            sizeVector.z = 1;

            transform.localScale = sizeVector;
        }
    }

    private Vector3 sizeVector = new Vector3(0f, 0f, 1); //크기 조정용 벡터

    private Vector2 targetPos; //현재 마우스로 찍은 목표 포지션

    [Tooltip("측정 가능한 최대 좌표값(음수 x, y)")]
    public Vector2Int bottomLeft;

    [Tooltip("측정 가능한 최대 좌표값(양수 x, y)")]
    public Vector2Int topRight;

    [Tooltip("시작 포지션(현재 플레이어 포지션)")]
    public Vector2Int startPos;

    [Tooltip("목표 포지션")]
    public Vector3 targetpos;

    public List<Node> FinalNodeList;

    [Tooltip("대각선 움직임 허용 여부")]
    public bool allowDiagonal;

    [Tooltip("코너 가로질러 가기 불가능 허용 여부")]
    public bool dontCrossCorner;

    int sizeX, sizeY;

    Node[,] NodeArray;

    Node StartNode, TargetNode, CurNode;

    List<Node> OpenList;

    List<Node> ClosedList;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MouseInput();
        Moving();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
        }

        size = 0.7f;
        sizeVector.x = size;
        sizeVector.y = size;
        targetPos = transform.position;
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            targetPos = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            targetpos = Camera.main.ScreenToWorldPoint(new Vector3Int((int)Input.mousePosition.x, (int)Input.mousePosition.y, 0));
            PathFinding();
        }
    }

    private void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);
    }

    public void PathFinding() //길찾기 시작 함수
    {
        #region 현재 위치 기준으로 최대 이동 범위 재설정(카메라 범위 기준, 카메라 비치는 크기에 비례해서 변경하도록 수정하기)
        bottomLeft.x = (int)(transform.position.x - 10);
        bottomLeft.y = (int)(transform.position.y - 5);

        topRight.x = (int)(transform.position.x + 10);
        topRight.y = (int)(transform.position.y + 5);
        #endregion

        startPos = Vector2Int.RoundToInt(transform.position); //만약 시작 지점 좌표가 소숫점이 포함된다면 int로 변형해서 가져오기(길찾기 시작 위치 현재 플레이어 위치로 초기화)

        // NodeArray의 크기 정해주기(현재 길찾기 전체 범위)
        sizeX = topRight.x - bottomLeft.x + 1; //1 더하는 이유는 0 포함
        sizeY = topRight.y - bottomLeft.y + 1;

        NodeArray = new Node[sizeX, sizeY]; //현재 탐색 범위 설정

        for (int i = 0; i < sizeX; i++) //전체 범위를 탐색해서 벽 찾기
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f)) //좌표를 돌아가며 벽 판별용 원을 그린다(한 칸으로 잡은 범위만큼 원 생성)
                {
                    if (col.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    }
                }

                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y); //isWall, x, y 대입
            }
        }


        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];

        TargetNode = NodeArray[(int)targetpos.x - bottomLeft.x, (int)targetpos.y - bottomLeft.y];

        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();


        while (OpenList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            CurNode = OpenList[0];

            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H)
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
                FinalNodeList.Reverse();

                for (int i = 0; i < FinalNodeList.Count; i++)
                {
                    print(i + "번째는 " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
                }
                return;
            }


            // ↗↖↙↘
            if (allowDiagonal)
            {
                OpenListAdd(CurNode.x + 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y - 1);
                OpenListAdd(CurNode.x + 1, CurNode.y - 1);
            }

            // ↑ → ↓ ←
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !ClosedList.Contains(NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal)
            {
                if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall)
                {
                    return;
                }
            }

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner)
            {
                if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall)
                {
                    return;
                }
            }

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);


            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                NeighborNode.ParentNode = CurNode;

                OpenList.Add(NeighborNode);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (FinalNodeList.Count != 0)
        {
            for (int i = 0; i < FinalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].x, FinalNodeList[i].y), new Vector2(FinalNodeList[i + 1].x, FinalNodeList[i + 1].y));
            }
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.gameObject.GetComponent<IInteraction>().Interaction();
    //}
}

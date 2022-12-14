using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [Tooltip("측정 가능한 최대 좌표값(음수 x, y)")]
    public Vector2Int bottomLeft;

    [Tooltip("측정 가능한 최대 좌표값(양수 x, y)")]
    public Vector2Int topRight;
    
    [Tooltip("시작 포지션(현재 플레이어 포지션)")]
    public Vector2Int startPos;

    [Tooltip("목표 포지션")]
    public Vector2Int targetPos; 

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
        PathFinding();
    }

    public void PathFinding() //길찾기 시작 함수
    {
        startPos = Vector2Int.RoundToInt(new Vector2(0, 0)); //만약 시작 지점 좌표가 소숫점이 포함된다면 int로 변형해서 가져오기

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

        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();


        while (OpenList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
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
                FinalNodeList.Reverse();

                for (int i = 0; i < FinalNodeList.Count; i++)
                {
                    print(i + "번째는 " + FinalNodeList[i].xPos + ", " + FinalNodeList[i].yPos);
                }
                return;
            }


            // ↗↖↙↘
            if (allowDiagonal)
            {
                OpenListAdd(CurNode.xPos + 1, CurNode.yPos + 1);
                OpenListAdd(CurNode.xPos - 1, CurNode.yPos + 1);
                OpenListAdd(CurNode.xPos - 1, CurNode.yPos - 1);
                OpenListAdd(CurNode.xPos + 1, CurNode.yPos - 1);
            }

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
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal)
            {
                if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
                {
                    return;
                }  
            }

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner)
            {
                if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
                {
                    return;
                }
            } 

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.sDistance + (CurNode.xPos - checkX == 0 || CurNode.yPos - checkY == 0 ? 10 : 14);


            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.sDistance || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.sDistance = MoveCost;
                NeighborNode.tDistance = (Mathf.Abs(NeighborNode.xPos - TargetNode.xPos) + Mathf.Abs(NeighborNode.yPos - TargetNode.yPos)) * 10;
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
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].xPos, FinalNodeList[i].yPos), new Vector2(FinalNodeList[i + 1].xPos, FinalNodeList[i + 1].yPos));
            }
        } 
    }
}
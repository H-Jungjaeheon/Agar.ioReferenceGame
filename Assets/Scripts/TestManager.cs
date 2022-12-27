using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [Tooltip("���� ������ �ִ� ��ǥ��(���� x, y)")]
    public Vector2Int bottomLeft;

    [Tooltip("���� ������ �ִ� ��ǥ��(��� x, y)")]
    public Vector2Int topRight;
    
    [Tooltip("���� ������(���� �÷��̾� ������)")]
    public Vector2Int startPos;

    [Tooltip("��ǥ ������")]
    public Vector2Int targetPos; 

    public List<Node> FinalNodeList;

    [Tooltip("�밢�� ������ ��� ����")]
    public bool allowDiagonal;

    [Tooltip("�ڳ� �������� ���� �Ұ��� ��� ����")]
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

    public void PathFinding() //��ã�� ���� �Լ�
    {
        startPos = Vector2Int.RoundToInt(new Vector2(0, 0)); //���� ���� ���� ��ǥ�� �Ҽ����� ���Եȴٸ� int�� �����ؼ� ��������

        // NodeArray�� ũ�� �����ֱ�(���� ��ã�� ��ü ����)
        sizeX = topRight.x - bottomLeft.x + 1; //1 ���ϴ� ������ 0 ����
        sizeY = topRight.y - bottomLeft.y + 1;

        NodeArray = new Node[sizeX, sizeY]; //���� Ž�� ���� ����

        for (int i = 0; i < sizeX; i++) //��ü ������ Ž���ؼ� �� ã��
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f)) //��ǥ�� ���ư��� �� �Ǻ��� ���� �׸���(�� ĭ���� ���� ������ŭ �� ����)
                {
                    if (col.gameObject.CompareTag("Wall"))
                    {
                        isWall = true;
                    } 
                }

                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y); //isWall, x, y ����
            }
        }


        // ���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];

        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();


        while (OpenList.Count > 0)
        {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
            CurNode = OpenList[0];

            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].F <= CurNode.F && OpenList[i].tDistance < CurNode.tDistance)
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
                FinalNodeList.Reverse();

                for (int i = 0; i < FinalNodeList.Count; i++)
                {
                    print(i + "��°�� " + FinalNodeList[i].xPos + ", " + FinalNodeList[i].yPos);
                }
                return;
            }


            // �֢آע�
            if (allowDiagonal)
            {
                OpenListAdd(CurNode.xPos + 1, CurNode.yPos + 1);
                OpenListAdd(CurNode.xPos - 1, CurNode.yPos + 1);
                OpenListAdd(CurNode.xPos - 1, CurNode.yPos - 1);
                OpenListAdd(CurNode.xPos + 1, CurNode.yPos - 1);
            }

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
            // �밢�� ����, �� ���̷� ��� �ȵ�
            if (allowDiagonal)
            {
                if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
                {
                    return;
                }  
            }

            // �ڳʸ� �������� ���� ������, �̵� �߿� �������� ��ֹ��� ������ �ȵ�
            if (dontCrossCorner)
            {
                if (NodeArray[CurNode.xPos - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.yPos - bottomLeft.y].isWall)
                {
                    return;
                }
            } 

            // �̿���忡 �ְ�, ������ 10, �밢���� 14���
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.sDistance + (CurNode.xPos - checkX == 0 || CurNode.yPos - checkY == 0 ? 10 : 14);


            // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
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
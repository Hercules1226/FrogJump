using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* (에이스타) 길찾기 알고리즘 본체입니다.
///
/// 한 줄 요약 : "지금까지 온 거리(G) + 앞으로 남은 예상 거리(H)" 가 가장 작은 칸부터 살펴본다.
///
///  - G 만 본다            -> 다익스트라 (사방으로 고르게 퍼짐, 느림)
///  - H 만 본다            -> 탐욕적 최선 탐색 (빠르지만 최단이 아닐 수 있음)
///  - G + H 를 같이 본다   -> A*  (빠르면서도 최단 경로를 보장)
/// </summary>
public class AStarPathfinder
{
    // 직선 이동 비용 10, 대각선 이동 비용 14 (루트2 = 1.414... 를 10배 해서 정수로)
    public const int STRAIGHT_COST = 10;
    public const int DIAGONAL_COST = 14;

    public int width;
    public int height;
    public PathNode[,] nodes;

    public bool allowDiagonal = false;   // 대각선 이동을 허용할까?
    public bool useHeuristic = true;     // false 로 두면 H=0 이 되어 '다익스트라'가 된다

    // 탐색 과정을 보여주기 위한 값들
    public List<PathNode> openList = new List<PathNode>();     // 열린 목록 : 앞으로 살펴볼 후보
    public HashSet<PathNode> closedSet = new HashSet<PathNode>(); // 닫힌 목록 : 이미 살펴본 칸

    public List<PathNode> resultPath = new List<PathNode>();
    public int visitedCount;    // 몇 칸이나 조사했는가 (알고리즘 효율 비교용)
    public bool found;


    public AStarPathfinder(int width, int height)
    {
        this.width = width;
        this.height = height;
        nodes = new PathNode[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                nodes[x, y] = new PathNode(x, y);
    }

    /// <summary> 탐색 기록만 지운다 (벽은 유지) </summary>
    public void ResetSearch()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                nodes[x, y].Reset();

        openList.Clear();
        closedSet.Clear();
        resultPath.Clear();
        visitedCount = 0;
        found = false;
    }


    // ============================================================
    //  A* 본체 - 한 칸씩 멈춰가며 보여주기 위해 코루틴(IEnumerator)으로 작성
    // ============================================================
    public IEnumerator Search(PathNode start, PathNode goal, float stepDelay)
    {
        ResetSearch();

        // [1단계] 출발점을 열린 목록에 넣는다
        start.gCost = 0;
        start.hCost = GetHeuristic(start, goal);
        start.state = NodeState.Open;
        openList.Add(start);

        while (openList.Count > 0)
        {
            // [2단계] 열린 목록에서 F 값이 가장 작은 칸을 고른다
            PathNode current = GetLowestFCostNode();

            // [3단계] 도착점에 닿았다면 끝! 부모를 거꾸로 따라가 경로를 만든다
            if (current == goal)
            {
                RetracePath(start, goal);
                found = true;
                yield break;
            }

            // [4단계] 고른 칸을 열린 목록에서 빼고 닫힌 목록에 넣는다
            openList.Remove(current);
            closedSet.Add(current);
            current.state = NodeState.Closed;
            visitedCount++;

            // [5단계] 이웃한 칸들을 하나씩 살펴본다
            foreach (PathNode neighbour in GetNeighbours(current))
            {
                // 벽이거나 이미 조사를 끝낸 칸이면 건너뛴다
                if (neighbour.isWall || closedSet.Contains(neighbour)) continue;

                // current 를 거쳐서 neighbour 로 갈 때의 비용
                int newG = current.gCost + GetDistance(current, neighbour);

                // 더 싼 길을 찾았다면 기록을 갱신한다
                if (newG < neighbour.gCost)
                {
                    neighbour.gCost = newG;
                    neighbour.hCost = GetHeuristic(neighbour, goal);
                    neighbour.parent = current;    // "나는 current 에서 왔다" 고 기록

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                        neighbour.state = NodeState.Open;
                    }
                }
            }

            // 한 칸 조사할 때마다 잠깐 멈춰서 화면을 보여준다
            if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
            else yield return null;
        }

        // 열린 목록이 비었는데도 도착하지 못했다 = 길이 없다
        found = false;
    }


    /// <summary>
    /// 열린 목록에서 F 가 가장 작은 칸을 찾는다.
    /// (F 가 같으면 H 가 작은 쪽 = 도착점에 더 가까운 쪽을 고른다)
    ///
    /// [참고] 여기서는 이해하기 쉽도록 전부 훑어보는 방식을 썼다.
    ///        칸이 아주 많아지면 '우선순위 큐(힙)' 를 쓰면 훨씬 빨라진다.
    /// </summary>
    PathNode GetLowestFCostNode()
    {
        PathNode best = openList[0];
        for (int i = 1; i < openList.Count; i++)
        {
            if (openList[i].FCost < best.FCost ||
               (openList[i].FCost == best.FCost && openList[i].hCost < best.hCost))
            {
                best = openList[i];
            }
        }
        return best;
    }


    /// <summary> 도착점에서 부모를 거꾸로 따라가며 경로를 복원한다 </summary>
    void RetracePath(PathNode start, PathNode goal)
    {
        resultPath.Clear();
        PathNode current = goal;

        while (current != start)
        {
            resultPath.Add(current);
            current = current.parent;   // 한 칸씩 거슬러 올라간다
        }
        resultPath.Add(start);
        resultPath.Reverse();           // 거꾸로 담았으니 뒤집어서 출발 -> 도착 순서로

        foreach (var n in resultPath) n.state = NodeState.Path;
    }


    /// <summary> 상하좌우(+대각선) 이웃 칸들을 돌려준다 </summary>
    public List<PathNode> GetNeighbours(PathNode node)
    {
        var list = new List<PathNode>();

        // 상하좌우 4방향
        int[] dx4 = { 0, 0, -1, 1 };
        int[] dy4 = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            int nx = node.x + dx4[i];
            int ny = node.y + dy4[i];
            if (IsInside(nx, ny)) list.Add(nodes[nx, ny]);
        }

        if (allowDiagonal)
        {
            int[] dx8 = { -1, -1, 1, 1 };
            int[] dy8 = { -1, 1, -1, 1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = node.x + dx8[i];
                int ny = node.y + dy8[i];
                if (!IsInside(nx, ny)) continue;

                // 벽 모서리를 뚫고 지나가지 못하게 막는다
                if (nodes[node.x, ny].isWall || nodes[nx, node.y].isWall) continue;

                list.Add(nodes[nx, ny]);
            }
        }
        return list;
    }

    bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    /// <summary> 붙어있는 두 칸 사이의 실제 이동 비용 </summary>
    int GetDistance(PathNode a, PathNode b)
    {
        bool diagonal = (a.x != b.x) && (a.y != b.y);
        return diagonal ? DIAGONAL_COST : STRAIGHT_COST;
    }


    /// <summary>
    /// 휴리스틱(H) : 도착점까지 '대충 이 정도 걸릴 것 같다' 는 예상값.
    ///
    /// 중요한 규칙 : 실제 거리보다 크게 예상하면 안 된다.
    ///              크게 잡으면 빨라지는 대신 최단 경로를 놓칠 수 있다.
    /// </summary>
    int GetHeuristic(PathNode a, PathNode b)
    {
        if (!useHeuristic) return 0;   // H = 0 으로 만들면 다익스트라와 같아진다

        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        if (allowDiagonal)
        {
            // 대각선을 쓸 수 있을 때 : 옥타일 거리
            int min = Mathf.Min(dx, dy);
            int max = Mathf.Max(dx, dy);
            return DIAGONAL_COST * min + STRAIGHT_COST * (max - min);
        }

        // 상하좌우만 쓸 때 : 맨해튼 거리
        return STRAIGHT_COST * (dx + dy);
    }
}

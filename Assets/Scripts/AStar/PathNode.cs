using UnityEngine;

/// <summary>
/// 길찾기 격자의 칸 하나를 나타내는 '데이터' 클래스입니다.
/// MonoBehaviour 가 아니므로 게임오브젝트에 붙이지 않습니다. (순수한 데이터 덩어리)
/// </summary>
public class PathNode
{
    public int x;          // 격자에서의 가로 위치
    public int y;          // 격자에서의 세로 위치
    public bool isWall;    // 벽이라서 지나갈 수 없는 칸인가?

    // ---- A* 의 핵심이 되는 세 가지 비용 ----
    public int gCost;      // G : 출발점에서 이 칸까지 '실제로' 걸린 비용
    public int hCost;      // H : 이 칸에서 도착점까지 '예상되는' 비용 (휴리스틱)
    public int FCost { get { return gCost + hCost; } }   // F = G + H (총 예상 비용)

    // 이 칸에 '어디서 왔는지'를 기억한다. 도착 후 거꾸로 따라가면 경로가 된다.
    public PathNode parent;

    // 화면에 색을 칠하기 위한 상태값
    public NodeState state;

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        Reset();
    }

    /// <summary> 탐색 기록만 초기화한다 (벽 정보는 그대로 둔다) </summary>
    public void Reset()
    {
        gCost = int.MaxValue;
        hCost = 0;
        parent = null;
        state = NodeState.None;
    }
}

/// <summary> 칸의 현재 상태 (색칠용) </summary>
public enum NodeState
{
    None,     // 아직 살펴보지 않음
    Open,     // 열린 목록에 들어있음 (후보)
    Closed,   // 닫힌 목록에 들어있음 (조사 완료)
    Path      // 최종 경로
}

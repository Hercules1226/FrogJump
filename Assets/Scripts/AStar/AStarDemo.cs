using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A* 길찾기를 눈으로 보여주는 데모입니다.
/// 격자를 만들고, 마우스 입력을 받고, 칸마다 색을 칠합니다.
///
/// 조작법
///   마우스 왼쪽 드래그 : 벽 그리기 / 지우기
///   S + 클릭           : 출발점 옮기기
///   G + 클릭           : 도착점 옮기기
///   Space              : 한 칸씩 천천히 탐색
///   Enter              : 즉시 탐색
///   C                  : 탐색 결과만 지우기
///   R                  : 벽까지 전부 지우기
///   D                  : 대각선 이동 켜기 / 끄기
///   H                  : 휴리스틱 끄기 -> 다익스트라와 비교해보기
///   위/아래 화살표     : 탐색 속도 조절
/// </summary>
public class AStarDemo : MonoBehaviour
{
    [Header("격자 크기")]
    public int gridWidth = 25;
    public int gridHeight = 15;

    [Header("칸에 사용할 스프라이트")]
    public Sprite cellSprite;            // Assets/Sprites/WhiteSquare.png

    [Header("탐색 속도 (작을수록 빠름)")]
    public float stepDelay = 0.02f;

    [Header("색상")]
    public Color emptyColor  = new Color(0.90f, 0.91f, 0.94f);
    public Color wallColor   = new Color(0.18f, 0.20f, 0.28f);
    public Color startColor  = new Color(0.20f, 0.80f, 0.40f);
    public Color goalColor   = new Color(0.95f, 0.30f, 0.35f);
    public Color openColor   = new Color(0.55f, 0.80f, 0.98f);
    public Color closedColor = new Color(0.98f, 0.72f, 0.45f);
    public Color pathColor   = new Color(1.00f, 0.90f, 0.25f);

    // ---- 내부 상태 ----
    private AStarPathfinder pathfinder;
    private SpriteRenderer[,] cells;     // 칸마다 하나씩 있는 그림 조각
    private PathNode startNode;
    private PathNode goalNode;

    private Camera cam;
    private Coroutine searchRoutine;
    private bool isSearching;
    private bool paintWallMode;          // 드래그하는 동안 '그리기' 인지 '지우기' 인지

    private Text infoText;


    void Start()
    {
        cam = Camera.main;
        pathfinder = new AStarPathfinder(gridWidth, gridHeight);

        BuildGrid();
        SetupCamera();
        BuildUI();

        // 출발점은 왼쪽 가운데, 도착점은 오른쪽 가운데
        startNode = pathfinder.nodes[1, gridHeight / 2];
        goalNode  = pathfinder.nodes[gridWidth - 2, gridHeight / 2];

        MakeSampleWalls();
        Redraw();
    }


    // ============================================================
    //  격자 만들기
    // ============================================================
    void BuildGrid()
    {
        // 이미 만들어둔 격자(에디터 미리보기)가 있으면 지우고 새로 만든다
        Transform old = transform.Find("Grid");
        if (old != null) DestroyImmediate(old.gameObject);

        Sprite square = cellSprite != null ? cellSprite : MakeWhiteSprite();

        var root = new GameObject("Grid").transform;
        root.SetParent(transform);
        root.localPosition = Vector3.zero;

        cells = new SpriteRenderer[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var go = new GameObject("Cell_" + x + "_" + y);
                go.transform.SetParent(root);
                go.transform.position = GridToWorld(x, y);
                go.transform.localScale = Vector3.one * 0.92f;   // 살짝 줄여서 격자선처럼 보이게

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.color = emptyColor;
                cells[x, y] = sr;
            }
        }
    }

    /// <summary> 스프라이트가 지정되지 않았을 때 쓸 흰색 사각형을 코드로 만든다 </summary>
    Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary> 격자 좌표 -> 월드 좌표 (격자를 화면 가운데로 옮긴다) </summary>
    Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x - (gridWidth - 1) * 0.5f, y - (gridHeight - 1) * 0.5f, 0f);
    }

    /// <summary> 월드 좌표 -> 격자 좌표 </summary>
    bool WorldToGrid(Vector3 world, out int gx, out int gy)
    {
        gx = Mathf.RoundToInt(world.x + (gridWidth - 1) * 0.5f);
        gy = Mathf.RoundToInt(world.y + (gridHeight - 1) * 0.5f);
        return gx >= 0 && gx < gridWidth && gy >= 0 && gy < gridHeight;
    }


    void SetupCamera()
    {
        // 에디터 미리보기에서도 동작하도록, 아직 못 찾았으면 여기서 찾는다
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.12f, 0.13f, 0.18f);

        // 격자가 세로로도 가로로도 잘리지 않도록 크기를 맞춘다
        float sizeByHeight = gridHeight * 0.5f + 1.5f;
        float sizeByWidth  = (gridWidth * 0.5f + 1f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }


    // ============================================================
    //  예시용 벽 (수업에서 바로 실행해볼 수 있도록 미리 놓아둔다)
    // ============================================================
    void MakeSampleWalls()
    {
        MakeWallColumn(7,  0, gridHeight - 5);
        MakeWallColumn(13, 4, gridHeight - 1);
        MakeWallColumn(18, 0, gridHeight - 6);
    }

    void MakeWallColumn(int x, int fromY, int toY)
    {
        if (x < 0 || x >= gridWidth) return;
        for (int y = fromY; y <= toY && y < gridHeight; y++)
        {
            if (y < 0) continue;
            pathfinder.nodes[x, y].isWall = true;
        }
    }


    // ============================================================
    //  입력 처리
    // ============================================================
    void Update()
    {
        HandleMouse();
        HandleKeyboard();
    }

    void HandleMouse()
    {
        if (isSearching || cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        int gx, gy;
        if (!WorldToGrid(world, out gx, out gy)) return;

        PathNode node = pathfinder.nodes[gx, gy];

        // S / G 키를 누른 채 클릭하면 출발점 / 도착점을 옮긴다
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.S))
        {
            if (!node.isWall && node != goalNode) { startNode = node; ClearSearchOnly(); Redraw(); }
            return;
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.G))
        {
            if (!node.isWall && node != startNode) { goalNode = node; ClearSearchOnly(); Redraw(); }
            return;
        }

        // 그냥 클릭하면 벽을 그리거나 지운다
        if (Input.GetMouseButtonDown(0))
        {
            paintWallMode = !node.isWall;   // 빈 칸을 눌렀으면 '그리기', 벽을 눌렀으면 '지우기'
        }
        if (Input.GetMouseButton(0))
        {
            if (node == startNode || node == goalNode) return;
            if (node.isWall != paintWallMode)
            {
                node.isWall = paintWallMode;
                ClearSearchOnly();
                Redraw();
            }
        }
    }

    void HandleKeyboard()
    {
        // Space : 천천히 탐색 / Enter : 즉시 탐색
        if (Input.GetKeyDown(KeyCode.Space)) StartSearch(stepDelay);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) StartSearch(0f);

        if (Input.GetKeyDown(KeyCode.C)) { ClearSearchOnly(); Redraw(); }

        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    pathfinder.nodes[x, y].isWall = false;
            ClearSearchOnly();
            Redraw();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            pathfinder.allowDiagonal = !pathfinder.allowDiagonal;
            ClearSearchOnly(); Redraw();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            pathfinder.useHeuristic = !pathfinder.useHeuristic;
            ClearSearchOnly(); Redraw();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))   { stepDelay = Mathf.Max(0f, stepDelay - 0.01f); Redraw(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { stepDelay = Mathf.Min(0.3f, stepDelay + 0.01f); Redraw(); }
    }


    // ============================================================
    //  탐색 실행
    // ============================================================
    void StartSearch(float delay)
    {
        if (searchRoutine != null) StopCoroutine(searchRoutine);
        searchRoutine = StartCoroutine(SearchRoutine(delay));
    }

    IEnumerator SearchRoutine(float delay)
    {
        isSearching = true;

        // AStarPathfinder 의 코루틴을 돌리면서, 매 단계마다 화면을 다시 칠한다
        IEnumerator search = pathfinder.Search(startNode, goalNode, delay);
        while (search.MoveNext())
        {
            Redraw();
            yield return search.Current;
        }

        isSearching = false;
        Redraw();
    }

    void ClearSearchOnly()
    {
        if (isSearching) return;
        pathfinder.ResetSearch();
    }


    // ============================================================
    //  화면 칠하기
    // ============================================================
    void Redraw()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                PathNode n = pathfinder.nodes[x, y];
                Color c;

                if (n.isWall)                         c = wallColor;
                else if (n == startNode)              c = startColor;
                else if (n == goalNode)               c = goalColor;
                else if (n.state == NodeState.Path)   c = pathColor;
                else if (n.state == NodeState.Closed) c = closedColor;
                else if (n.state == NodeState.Open)   c = openColor;
                else                                  c = emptyColor;

                cells[x, y].color = c;
            }
        }
        UpdateInfoText();
    }


    // ============================================================
    //  화면 글자
    // ============================================================
    void BuildUI()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGO.AddComponent<GraphicRaycaster>();

        Text helpText = CreateText(canvasGO.transform, "HelpText", 26,
            new Vector2(0f, 1f), new Vector2(20f, -15f), new Vector2(1100f, 260f), TextAnchor.UpperLeft, font);
        helpText.text =
            "[마우스 드래그] 벽 그리기/지우기    [S+클릭] 출발점    [G+클릭] 도착점\n" +
            "[Space] 천천히 탐색    [Enter] 즉시 탐색    [C] 결과 지우기    [R] 벽 전체 지우기\n" +
            "[D] 대각선 이동    [H] 휴리스틱 끄기(다익스트라 비교)    [상/하 화살표] 속도 조절";

        infoText = CreateText(canvasGO.transform, "InfoText", 30,
            new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(1300f, 130f), TextAnchor.LowerLeft, font);
    }

    Text CreateText(Transform parent, string name, int size, Vector2 anchor,
                    Vector2 pos, Vector2 sizeDelta, TextAnchor align, Font font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;

        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    void UpdateInfoText()
    {
        if (infoText == null) return;

        string mode = pathfinder.useHeuristic ? "A* (F = G + H)" : "다익스트라 (H = 0)";
        string diag = pathfinder.allowDiagonal ? "8방향" : "4방향";

        string status;
        if (isSearching)                      status = "탐색 중...";
        else if (pathfinder.found)            status = "경로 발견!  길이 " + pathfinder.resultPath.Count + "칸";
        else if (pathfinder.visitedCount > 0) status = "경로가 없습니다";
        else                                  status = "Space 또는 Enter 를 눌러 시작";

        infoText.text =
            "모드: " + mode + "    이동: " + diag + "    속도: " + stepDelay.ToString("0.00") + "초\n" +
            "조사한 칸: " + pathfinder.visitedCount + "    열린 목록: " + pathfinder.openList.Count + "\n" +
            status;
    }


    // ============================================================
    //  에디터 전용 : 플레이하지 않아도 격자를 미리 볼 수 있게 해준다
    //  (인스펙터의 컴포넌트 우클릭 메뉴에서 실행)
    // ============================================================
    [ContextMenu("격자 미리보기 만들기")]
    public void BuildPreview()
    {
        pathfinder = new AStarPathfinder(gridWidth, gridHeight);
        BuildGrid();
        SetupCamera();

        startNode = pathfinder.nodes[1, gridHeight / 2];
        goalNode  = pathfinder.nodes[gridWidth - 2, gridHeight / 2];
        MakeSampleWalls();

        // 미리보기에서는 UI 글자가 없으므로 색만 칠한다
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                PathNode n = pathfinder.nodes[x, y];
                Color c = emptyColor;
                if (n.isWall)            c = wallColor;
                else if (n == startNode) c = startColor;
                else if (n == goalNode)  c = goalColor;
                cells[x, y].color = c;
            }
        }
    }

    [ContextMenu("격자 미리보기 지우기")]
    public void ClearPreview()
    {
        Transform old = transform.Find("Grid");
        if (old != null) DestroyImmediate(old.gameObject);
    }
}

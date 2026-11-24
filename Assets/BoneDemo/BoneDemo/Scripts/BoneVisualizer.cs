using System.Collections.Generic;
using UnityEngine;

// 본(뼈대)을 선으로 그려서 보여주는 스크립트
// - Scene 뷰: Gizmo로 항상 표시
// - Game 뷰: showInGame 이 켜져 있으면 LineRenderer로 표시
[System.Serializable]
public class BonePair
{
    public Transform start; // 관절(본의 시작점)
    public Transform end;   // 다음 관절 또는 끝점
}

public class BoneVisualizer : MonoBehaviour
{
    public List<BonePair> bones = new List<BonePair>();
    public bool showInGame = true;
    public Color boneColor = new Color(1f, 0.85f, 0.1f, 0.95f);
    public float width = 0.1f;

    readonly List<LineRenderer> lines = new List<LineRenderer>();

    void Start()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); // URP 2D 프로젝트
        if (shader == null) shader = Shader.Find("Sprites/Default");                         // Built-in 프로젝트
        Material mat = new Material(shader);

        foreach (BonePair pair in bones)
        {
            GameObject go = new GameObject("BoneLine");
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.material = mat;
            lr.startWidth = width;         // 관절 쪽은 두껍게
            lr.endWidth = width * 0.25f;   // 끝 쪽은 가늘게 → 뼈 방향이 보임
            lr.startColor = lr.endColor = boneColor;
            lr.numCapVertices = 4;
            lr.sortingOrder = 100;         // 캐릭터보다 앞에 그리기
            lines.Add(lr);
        }
    }

    void LateUpdate() // 애니메이션이 적용된 뒤에 선 위치 갱신
    {
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].enabled = showInGame;
            if (!showInGame) continue;
            lines[i].SetPosition(0, bones[i].start.position);
            lines[i].SetPosition(1, bones[i].end.position);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = boneColor;
        foreach (BonePair pair in bones)
        {
            if (pair.start == null || pair.end == null) continue;
            Gizmos.DrawLine(pair.start.position, pair.end.position);
            Gizmos.DrawWireSphere(pair.start.position, 0.06f);
        }
    }
}

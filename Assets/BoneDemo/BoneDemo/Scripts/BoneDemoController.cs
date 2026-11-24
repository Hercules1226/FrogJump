using UnityEngine;

// 화면 왼쪽 위 패널로 애니메이션을 바꾸고, 본을 직접 돌려볼 수 있는 스크립트
// (OnGUI를 사용하므로 Input System 설정과 관계없이 동작합니다)
public class BoneDemoController : MonoBehaviour
{
    public Animator animator;
    public BoneVisualizer visualizer;
    public Transform[] poseBones;   // 직접 조작 모드에서 돌릴 본
    public string[] poseLabels;

    readonly string[] states = { "Idle", "Walk", "Wave" };
    readonly string[] stateLabels = { "대기 (Idle)", "걷기 (Walk)", "손 흔들기 (Wave)" };

    float speed = 1f;
    bool manual;
    float[] angles;

    void Start()
    {
        angles = new float[poseBones.Length];
    }

    void OnGUI()
    {
        float s = Screen.height / 720f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

        GUILayout.BeginArea(new Rect(10, 10, 290, 700), GUI.skin.box);
        GUILayout.Label("2D 본 애니메이션 데모");
        GUILayout.Space(6);

        if (!manual)
        {
            for (int i = 0; i < states.Length; i++)
                if (GUILayout.Button(stateLabels[i], GUILayout.Height(32)))
                    animator.CrossFade(states[i], 0.2f);

            GUILayout.Label($"재생 속도: {speed:0.0}x");
            speed = GUILayout.HorizontalSlider(speed, 0f, 2f);
            animator.speed = speed;
            GUILayout.Space(10);
        }

        visualizer.showInGame = GUILayout.Toggle(visualizer.showInGame, " 본(뼈대) 보이기");

        bool newManual = GUILayout.Toggle(manual, " 직접 조작 모드 (애니메이션 끄기)");
        if (newManual != manual)
        {
            manual = newManual;
            animator.enabled = !manual;
            if (manual) ResetPose();
        }

        if (manual)
        {
            GUILayout.Space(6);
            GUILayout.Label("부모 본을 돌리면 자식 본이 따라 움직여요!");
            for (int i = 0; i < poseBones.Length; i++)
            {
                GUILayout.Label($"{poseLabels[i]}: {angles[i]:0}°");
                angles[i] = GUILayout.HorizontalSlider(angles[i], -180f, 180f);
                poseBones[i].localRotation = Quaternion.Euler(0f, 0f, angles[i]);
            }
            if (GUILayout.Button("자세 초기화", GUILayout.Height(28))) ResetPose();
        }

        GUILayout.EndArea();
    }

    void ResetPose()
    {
        for (int i = 0; i < poseBones.Length; i++)
        {
            angles[i] = 0f;
            poseBones[i].localRotation = Quaternion.identity;
        }
    }
}

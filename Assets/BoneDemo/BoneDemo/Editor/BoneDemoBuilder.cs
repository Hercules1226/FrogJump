using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// 메뉴: Tools > Bone Demo > 캐릭터 만들기
// 1) 스프라이트 임포트 설정(피벗 = 관절 위치)
// 2) 본 계층 구조 생성
// 3) Idle / Walk / Wave 애니메이션 클립 + Animator Controller 생성
// 4) 프리팹 저장 후 현재 씬에 배치
public static class BoneDemoBuilder
{
    const string RootDir = "Assets/BoneDemo";
    const string SpriteDir = RootDir + "/Sprites";
    const string GenDir = RootDir + "/Generated";

    // 본 길이 (단위: Unity unit, 100px = 1unit)
    const float HipY = 1.7f;
    const float ShoulderY = 1.10f;
    const float NeckY = 1.30f;
    const float UpperArmLen = 0.66f;
    const float LowerArmLen = 0.75f;
    const float UpperLegLen = 0.66f;
    const float LowerLegLen = 0.63f;

    static readonly Color BackTint = new Color(0.75f, 0.75f, 0.8f);

    [MenuItem("Tools/Bone Demo/캐릭터 만들기")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(GenDir)) AssetDatabase.CreateFolder(RootDir, "Generated");

        // ---------- 1. 스프라이트 (피벗 = 회전 중심 관절) ----------
        Sprite head     = LoadSprite("head",      new Vector2(0.5f, 0.114f));
        Sprite torso    = LoadSprite("torso",     new Vector2(0.5f, 0.125f));
        Sprite upperArm = LoadSprite("upper_arm", new Vector2(0.5f, 0.8f));
        Sprite lowerArm = LoadSprite("lower_arm", new Vector2(0.5f, 0.833f));
        Sprite upperLeg = LoadSprite("upper_leg", new Vector2(0.5f, 0.775f));
        Sprite lowerLeg = LoadSprite("lower_leg", new Vector2(0.29f, 0.807f));

        GameObject old = GameObject.Find("BoneCharacter");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        // ---------- 2. 본 계층 구조 ----------
        // BoneCharacter
        //  └ Hip ─ Torso(스프라이트)
        //     ├ Neck ─ Head(스프라이트), Neck_End
        //     ├ UpperArm_L/R ─ Forearm_L/R ─ Forearm_End
        //     └ Thigh_L/R ─ Shin_L/R ─ Shin_End
        GameObject root = new GameObject("BoneCharacter");
        Transform hip = Bone("Hip", root.transform, new Vector3(0, HipY, 0));
        AddSprite(hip, "Torso_Sprite", torso, 5, Color.white);

        Transform neck = Bone("Neck", hip, new Vector3(0, NeckY, 0));
        AddSprite(neck, "Head_Sprite", head, 8, Color.white);
        Transform neckEnd = Bone("Neck_End", neck, new Vector3(0, 1.1f, 0));

        // _R = 뒤쪽(어둡게, 몸통 뒤), _L = 앞쪽(몸통 앞)
        Transform uaR = Bone("UpperArm_R", hip, new Vector3(0.05f, ShoulderY, 0));
        AddSprite(uaR, "UpperArm_R_Sprite", upperArm, 1, BackTint);
        Transform faR = Bone("Forearm_R", uaR, new Vector3(0, -UpperArmLen, 0));
        AddSprite(faR, "Forearm_R_Sprite", lowerArm, 0, BackTint);
        Transform faREnd = Bone("Forearm_R_End", faR, new Vector3(0, -LowerArmLen, 0));

        Transform thR = Bone("Thigh_R", hip, new Vector3(0.05f, 0, 0));
        AddSprite(thR, "Thigh_R_Sprite", upperLeg, 3, BackTint);
        Transform shR = Bone("Shin_R", thR, new Vector3(0, -UpperLegLen, 0));
        AddSprite(shR, "Shin_R_Sprite", lowerLeg, 2, BackTint);
        Transform shREnd = Bone("Shin_R_End", shR, new Vector3(0, -LowerLegLen, 0));

        Transform thL = Bone("Thigh_L", hip, new Vector3(0, 0, 0));
        AddSprite(thL, "Thigh_L_Sprite", upperLeg, 7, Color.white);
        Transform shL = Bone("Shin_L", thL, new Vector3(0, -UpperLegLen, 0));
        AddSprite(shL, "Shin_L_Sprite", lowerLeg, 6, Color.white);
        Transform shLEnd = Bone("Shin_L_End", shL, new Vector3(0, -LowerLegLen, 0));

        Transform uaL = Bone("UpperArm_L", hip, new Vector3(0, ShoulderY, 0));
        AddSprite(uaL, "UpperArm_L_Sprite", upperArm, 10, Color.white);
        Transform faL = Bone("Forearm_L", uaL, new Vector3(0, -UpperArmLen, 0));
        AddSprite(faL, "Forearm_L_Sprite", lowerArm, 9, Color.white);
        Transform faLEnd = Bone("Forearm_L_End", faL, new Vector3(0, -LowerArmLen, 0));

        // ---------- 3. 애니메이션 ----------
        AnimationClip idle = CreateClip("Idle", 2f, IdlePose);
        AnimationClip walk = CreateClip("Walk", 1f, WalkPose);
        AnimationClip wave = CreateClip("Wave", 1f, WavePose);

        string ctrlPath = GenDir + "/BoneCharacter.controller";
        AssetDatabase.DeleteAsset(ctrlPath);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("Idle"); idleState.motion = idle;
        AnimatorState walkState = sm.AddState("Walk"); walkState.motion = walk;
        AnimatorState waveState = sm.AddState("Wave"); waveState.motion = wave;
        sm.defaultState = idleState;

        Animator animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;

        // ---------- 4. 본 표시 + 컨트롤 패널 ----------
        BoneVisualizer vis = root.AddComponent<BoneVisualizer>();
        vis.bones = new List<BonePair>
        {
            Pair(hip, neck), Pair(neck, neckEnd),
            Pair(uaR, faR), Pair(faR, faREnd), Pair(thR, shR), Pair(shR, shREnd),
            Pair(thL, shL), Pair(shL, shLEnd), Pair(uaL, faL), Pair(faL, faLEnd),
        };

        BoneDemoController controller = root.AddComponent<BoneDemoController>();
        controller.animator = animator;
        controller.visualizer = vis;
        controller.poseBones = new[] { neck, uaL, faL, thL, shL };
        controller.poseLabels = new[] { "목 (Neck)", "윗팔 (UpperArm_L)", "아래팔 (Forearm_L)", "허벅지 (Thigh_L)", "정강이 (Shin_L)" };

        PrefabUtility.SaveAsPrefabAssetAndConnect(root, GenDir + "/BoneCharacter.prefab", InteractionMode.UserAction);
        Undo.RegisterCreatedObjectUndo(root, "Create Bone Character");

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 3f;
            cam.transform.position = new Vector3(0, 2.1f, -10f);
        }

        Selection.activeGameObject = root;
        AssetDatabase.SaveAssets();
        Debug.Log("[Bone Demo] 캐릭터 생성 완료! ▶ Play 버튼을 눌러 확인하세요.");
    }

    // ===================== 포즈 정의 =====================
    // p = 0~1 (한 주기), 반환값: 본 경로 → Z 회전각(도)
    // 오른쪽을 보는 캐릭터 기준: + 각도 = 앞으로, - 각도 = 뒤로
    static readonly string[] BonePaths =
    {
        "Hip/Neck",
        "Hip/UpperArm_L", "Hip/UpperArm_L/Forearm_L",
        "Hip/UpperArm_R", "Hip/UpperArm_R/Forearm_R",
        "Hip/Thigh_L", "Hip/Thigh_L/Shin_L",
        "Hip/Thigh_R", "Hip/Thigh_R/Shin_R",
    };

    struct Pose
    {
        public float hipY;
        public Dictionary<string, float> rot;
    }

    static float Cos(float p) => Mathf.Cos(p * Mathf.PI * 2f);
    static float Sin(float p) => Mathf.Sin(p * Mathf.PI * 2f);

    static Pose IdlePose(float p) => new Pose
    {
        hipY = HipY + 0.03f * Sin(p),   // 숨쉬기
        rot = new Dictionary<string, float>
        {
            { "Hip/Neck", 3f * Sin(p) },
            { "Hip/UpperArm_L", 4f * Sin(p) }, { "Hip/UpperArm_L/Forearm_L", 8f },
            { "Hip/UpperArm_R", -4f * Sin(p) }, { "Hip/UpperArm_R/Forearm_R", 8f },
        }
    };

    static Pose WalkPose(float p)
    {
        // 다리: 앞뒤로 흔들기, 무릎은 뒤로만 굽힘
        float thighL = 25f * Cos(p);
        float thighR = -thighL;
        float shinL = -5f - 45f * Mathf.Max(0f, Sin(p + 0.5f));
        float shinR = -5f - 45f * Mathf.Max(0f, Sin(p));
        return new Pose
        {
            hipY = HipY - 0.06f + 0.06f * Mathf.Abs(Cos(p)), // 걸음마다 위아래로 출렁
            rot = new Dictionary<string, float>
            {
                { "Hip/Neck", 3f * Cos(p * 2f) },
                { "Hip/Thigh_L", thighL }, { "Hip/Thigh_L/Shin_L", shinL },
                { "Hip/Thigh_R", thighR }, { "Hip/Thigh_R/Shin_R", shinR },
                // 팔은 같은 쪽 다리와 반대로
                { "Hip/UpperArm_L", -20f * Cos(p) }, { "Hip/UpperArm_L/Forearm_L", 20f + 10f * Cos(p) },
                { "Hip/UpperArm_R", 20f * Cos(p) },  { "Hip/UpperArm_R/Forearm_R", 20f - 10f * Cos(p) },
            }
        };
    }

    static Pose WavePose(float p) => new Pose
    {
        hipY = HipY,
        rot = new Dictionary<string, float>
        {
            { "Hip/Neck", 6f },
            { "Hip/UpperArm_L", 140f },                          // 팔을 위로 들기
            { "Hip/UpperArm_L/Forearm_L", 35f * Sin(p) + 10f },   // 아래팔만 흔들기
            { "Hip/UpperArm_R", -3f }, { "Hip/UpperArm_R/Forearm_R", 8f },
        }
    };

    // ===================== 헬퍼 =====================
    static AnimationClip CreateClip(string name, float duration, Func<float, Pose> poseFn)
    {
        const int samples = 16;
        AnimationClip clip = new AnimationClip { name = name, frameRate = 30 };

        var hipKeys = new Keyframe[samples + 1];
        var rotKeys = new Dictionary<string, Keyframe[]>();
        foreach (string path in BonePaths) rotKeys[path] = new Keyframe[samples + 1];

        for (int i = 0; i <= samples; i++)
        {
            float p = (float)i / samples;
            float t = duration * p;
            Pose pose = poseFn(p);
            hipKeys[i] = new Keyframe(t, pose.hipY);
            foreach (string path in BonePaths)
            {
                pose.rot.TryGetValue(path, out float angle); // 없으면 0도
                rotKeys[path][i] = new Keyframe(t, angle);
            }
        }

        SetCurve(clip, "Hip", "m_LocalPosition.x", Constant(duration, 0f));
        SetCurve(clip, "Hip", "m_LocalPosition.y", Smooth(hipKeys));
        SetCurve(clip, "Hip", "m_LocalPosition.z", Constant(duration, 0f));
        foreach (string path in BonePaths)
        {
            SetCurve(clip, path, "localEulerAnglesRaw.x", Constant(duration, 0f));
            SetCurve(clip, path, "localEulerAnglesRaw.y", Constant(duration, 0f));
            SetCurve(clip, path, "localEulerAnglesRaw.z", Smooth(rotKeys[path]));
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string assetPath = $"{GenDir}/{name}.anim";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(clip, assetPath);
        return clip;
    }

    static void SetCurve(AnimationClip clip, string path, string prop, AnimationCurve curve)
    {
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), prop), curve);
    }

    static AnimationCurve Smooth(Keyframe[] keys)
    {
        AnimationCurve c = new AnimationCurve(keys);
        for (int i = 0; i < c.length; i++) c.SmoothTangents(i, 0f);
        return c;
    }

    static AnimationCurve Constant(float duration, float v) => AnimationCurve.Constant(0f, duration, v);

    static BonePair Pair(Transform a, Transform b) => new BonePair { start = a, end = b };

    static Transform Bone(string name, Transform parent, Vector3 localPos)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = localPos;
        return t;
    }

    static void AddSprite(Transform bone, string name, Sprite sprite, int order, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(bone, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.color = color;
    }

    static Sprite LoadSprite(string file, Vector2 pivot)
    {
        string path = $"{SpriteDir}/{file}.png";
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) throw new Exception("스프라이트를 찾을 수 없습니다: " + path + " (BoneDemo 폴더를 Assets 바로 아래에 두세요)");

        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 100;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;

        TextureImporterSettings s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spritePivot = pivot;
        s.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(s);
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}

# 2D 본 애니메이션 데모 (Unity 6.x)

## 사용법
1. `BoneDemo` 폴더를 프로젝트의 **Assets 바로 아래**에 넣습니다. (`Assets/BoneDemo/...`)
2. 메뉴 **Tools > Bone Demo > 캐릭터 만들기** 클릭
3. ▶ Play → 왼쪽 위 패널에서 조작
   - 대기 / 걷기 / 손 흔들기 전환, 재생 속도 조절
   - **본(뼈대) 보이기**: 노란 선 = 본 (굵은 쪽이 관절)
   - **직접 조작 모드**: 슬라이더로 본을 돌려 "부모가 돌면 자식이 따라온다"를 확인

## 수업 포인트
- Hierarchy에서 `Hip > UpperArm_L > Forearm_L` 구조 = 본의 부모-자식 관계
- 스프라이트 **피벗 = 관절 위치** (Sprite Editor에서 확인 가능)
- `Generated/Walk.anim`을 Animation 창에서 열면 각 본의 Rotation.z 키프레임이 보입니다
- 학생 실습: 새 클립을 만들어 녹화(●) 버튼으로 점프·춤 동작 만들기

## 확장: 2D Animation 패키지 (스킨 메시 본)
`Sprites/character_full.png`는 한 장짜리 전신 이미지입니다.
1. Package Manager에서 **2D Animation** 설치 (2D 템플릿이면 이미 설치됨)
2. 이미지 선택 → Sprite Editor → 왼쪽 위 드롭다운 **Skinning Editor**
3. Create Bone으로 본 그리기 → Auto Geometry → Auto Weights
4. 씬에 놓고 **Sprite Skin** 컴포넌트 → Create Bones
5. 팔·다리에 **IK Manager 2D + Limb Solver**를 달면 손끝/발끝을 끌어서 포즈 잡기 가능

※ 이 데모(파츠 분리형)는 Transform 계층만 사용하므로 패키지 없이도 동작합니다.

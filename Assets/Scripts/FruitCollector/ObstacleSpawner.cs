using UnityEngine;

/// <summary>
/// 일정한 시간마다 장애물 프리팹을 복제해서 떨어뜨리는 '생성기' 입니다.
///
/// [이번주 신규] Instantiate        - 프리팹을 복제해서 씬에 실제로 만들어내기
/// [이번주 신규] InvokeRepeating    - 일정 시간마다 같은 함수를 반복 실행하기
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("무엇을 만들까")]
    public GameObject obstaclePrefab;   // Project 창의 프리팹을 여기에 드래그한다

    [Header("언제 만들까")]
    public float firstDelay = 1.5f;     // 게임 시작 후 몇 초 뒤에 첫 번째를 만들까
    public float interval = 2.5f;       // 그 다음부터 몇 초마다 만들까

    [Header("어디에 만들까")]
    public float randomRangeX = 0f;     // 0 이면 항상 같은 자리, 1 이면 좌우로 1만큼 흔들린다

    void Start()
    {
        // InvokeRepeating("함수이름", 처음 기다릴 시간, 반복 간격)
        //
        // 주의! 함수 이름을 글자(문자열)로 적기 때문에 오타가 나도 빨간 줄이 안 생긴다.
        //       아무 일도 일어나지 않는다면 철자부터 확인하자.
        InvokeRepeating("SpawnObstacle", firstDelay, interval);
    }

    /// <summary> 장애물 하나를 실제로 만들어낸다 </summary>
    void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;   // 프리팹을 안 넣었으면 그냥 넘어간다

        // 생성 위치를 정한다 (randomRangeX 만큼 좌우로 흔들어서 매번 조금씩 다르게)
        float offsetX = Random.Range(-randomRangeX, randomRangeX);
        Vector3 spawnPosition = transform.position + new Vector3(offsetX, 0f, 0f);

        // Instantiate(복제할 것, 위치, 회전)
        // = "이 프리팹을 똑같이 하나 찍어서 저 자리에 놓아줘"
        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
    }

    // 씬 화면에서 생성 위치가 어디인지 보이도록 표시해준다 (게임 화면에는 안 나온다)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 3f);
    }
}

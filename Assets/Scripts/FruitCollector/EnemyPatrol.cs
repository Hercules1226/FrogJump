using UnityEngine;

/// <summary>
/// [도전 과제] 좌우로 왔다갔다 순찰하는 장애물입니다.
///
/// 시간이 남는 친구들이 도전해보세요!
/// - 6주차에 배운 transform.Translate 로 스스로 움직인다
/// - 주인공과 닿으면 과일/가시와 똑같이 충돌을 감지한다
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("순찰 설정")]
    public float moveSpeed = 3f;   // 움직이는 속도
    public float moveRange = 3f;   // 시작 위치에서 좌우로 얼마나 멀리 갈지

    private Vector3 startPosition;   // 순찰의 기준이 되는 시작 위치
    private int direction = 1;       // 진행 방향 (1: 오른쪽, -1: 왼쪽)

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 지금 방향으로 계속 이동한다 (Time.deltaTime 을 곱해 속도를 일정하게!)
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        // 오른쪽 끝에 닿으면 왼쪽으로 방향을 바꾼다
        if (transform.position.x > startPosition.x + moveRange)
        {
            direction = -1;
        }
        // 왼쪽 끝에 닿으면 오른쪽으로 방향을 바꾼다
        else if (transform.position.x < startPosition.x - moveRange)
        {
            direction = 1;
        }
    }

    // 주인공이 내 영역에 들어오면 (Is Trigger 가 켜져 있음)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<PlayerController>().Respawn();
        GameManager.instance.LoseHeart();
    }
}

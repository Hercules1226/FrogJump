using UnityEngine;

/// <summary>
/// 하늘에서 떨어지는 장애물(쇠공)에 붙는 스크립트입니다.
///
/// 이 오브젝트는 씬에 미리 놓여있지 않습니다.
/// '프리팹(Prefab)' 으로 저장해두고, ObstacleSpawner 가 Instantiate 로 복제해서 만듭니다.
///
/// [이번주 신규] 프리팹 - 미리 만들어 둔 '붕어빵 틀'
/// </summary>
public class FallingObstacle : MonoBehaviour
{
    [Header("설정")]
    public float lifeTime = 8f;      // 몇 초 뒤에 저절로 사라질까 (혹시 안 없어졌을 때의 안전장치)
    public float rotateSpeed = 180f; // 떨어지면서 도는 속도 (보기 좋으라고)

    void Start()
    {
        // Destroy 에 시간을 같이 넘기면 '몇 초 뒤에 없애라' 는 뜻이 된다.
        // 화면 밖으로 나간 공이 계속 남아있으면 게임이 점점 느려지기 때문에 꼭 정리해줘야 한다.
        Destroy(gameObject, lifeTime);

        // 떨어지면서 빙글빙글 돌게 만든다.
        // Rigidbody2D 가 붙어 있으면 transform.Rotate 로 돌려도 물리 엔진이 덮어써 버린다.
        // 그래서 '회전 속도(angularVelocity)' 를 물리 엔진에게 직접 알려준다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = -rotateSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // (1) 주인공에게 맞았다면 - 가시와 똑같이 처리한다
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Respawn();
            GameManager.instance.LoseHeart();
            Destroy(gameObject);      // 맞은 공은 바로 없앤다
            return;
        }

        // (2) 바닥이나 발판에 닿았다면 - 부서진 것처럼 사라진다
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}

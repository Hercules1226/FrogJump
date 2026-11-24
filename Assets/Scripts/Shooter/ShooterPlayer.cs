using UnityEngine;

/// <summary>
/// 플레이어 비행기를 조종하는 스크립트입니다.
/// - 방향키(WASD)로 상하좌우 이동
/// - 스페이스바를 누르고 있으면 총알 자동 발사
/// - 적과 부딪히면 목숨이 줄고 잠깐 무적(깜빡임)
/// </summary>
public class ShooterPlayer : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 8f;        // 이동 속도

    [Header("발사 설정")]
    public GameObject bulletPrefab;     // 발사할 총알 프리팹
    public Transform firePoint;         // 총알이 나오는 위치
    public float fireInterval = 0.15f;  // 발사 간격(초)

    [Header("폭탄 설정 (C 키)")]
    public float bombInterval = 0.8f;   // 폭탄 재사용 대기시간(초)
    public int bombDamage = 3;          // 폭발 데미지
    public float bombRadius = 2.5f;     // 스플래시 반경

    [Header("피격 설정")]
    public float invincibleTime = 1.5f; // 맞은 뒤 무적 시간
    public GameObject explosionPrefab;  // 폭발 효과

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float fireTimer;
    private float bombTimer;
    private float invincibleTimer;
    private Vector2 minPos, maxPos;     // 화면 밖으로 못 나가게 하는 경계

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 카메라가 보여주는 영역을 계산해서 이동 가능 범위로 사용
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;
        minPos = new Vector2(c.x - halfW + 0.5f, c.y - halfH + 0.5f);
        maxPos = new Vector2(c.x + halfW - 0.5f, c.y + halfH - 0.5f);
    }

    void Update()
    {
        if (ShooterGameManager.Instance.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 1) 이동 : 입력 방향 * 속도
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // 2) 발사 : 타이머가 0 이하가 되면 한 발 쏘고 타이머 초기화
        fireTimer -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            fireTimer = fireInterval;
        }

        // 2-1) 폭탄 : C 키를 누르면 폭탄 발사 (적에 맞으면 범위 폭발)
        bombTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.C) && bombTimer <= 0f)
        {
            ShooterBomb bomb = ShooterBomb.Create(firePoint.position, explosionPrefab);
            bomb.damage = bombDamage;
            bomb.radius = bombRadius;
            bombTimer = bombInterval;
        }

        // 3) 무적 상태라면 깜빡깜빡
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
            sr.enabled = Mathf.FloorToInt(invincibleTimer * 10f) % 2 == 0;
            if (invincibleTimer <= 0f) sr.enabled = true;
        }
    }

    void FixedUpdate()
    {
        // 화면 경계 안으로 위치 제한
        Vector2 p = rb.position;
        p.x = Mathf.Clamp(p.x, minPos.x, maxPos.x);
        p.y = Mathf.Clamp(p.y, minPos.y, maxPos.y);
        rb.position = p;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ShooterEnemy enemy = other.GetComponent<ShooterEnemy>();
        if (enemy == null) return;

        enemy.Explode();      // 부딪힌 적은 터짐 (점수 없음)
        if (invincibleTimer > 0f) return;

        ShooterGameManager.Instance.LoseLife();
        invincibleTimer = invincibleTime;

        if (ShooterGameManager.Instance.IsGameOver)
        {
            if (explosionPrefab != null)
            {
                GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                fx.transform.localScale = Vector3.one * 2f;
            }
            gameObject.SetActive(false);
        }
    }
}

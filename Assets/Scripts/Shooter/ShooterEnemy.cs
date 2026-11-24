using System.Collections;
using UnityEngine;

/// <summary>
/// 위에서 아래로 내려오는 적. 체력이 0이 되면 점수를 주고 폭발합니다.
/// </summary>
public class ShooterEnemy : MonoBehaviour
{
    public float speed = 3f;         // 내려오는 속도
    public int hp = 1;               // 체력
    public int score = 10;           // 처치 시 점수
    public float swayAmount = 0f;    // 좌우로 흔들리는 폭 (0이면 직진)
    public GameObject explosionPrefab;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color baseColor;
    private float startX;
    private float time;
    private float bottomY;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        startX = transform.position.x;

        Camera cam = Camera.main;
        bottomY = cam.transform.position.y - cam.orthographicSize - 1f;
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        float x = startX + Mathf.Sin(time * 2f) * swayAmount;   // 사인 함수로 좌우 흔들기
        float y = rb.position.y - speed * Time.fixedDeltaTime;
        rb.MovePosition(new Vector2(x, y));

        if (y < bottomY) Destroy(gameObject);   // 화면 아래로 지나가면 삭제
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        hp -= damage;
        if (hp <= 0)
        {
            ShooterGameManager.Instance.AddScore(score);
            Explode();
        }
        else
        {
            StartCoroutine(HitFlash());
        }
    }

    public void Explode()
    {
        if (isDead) return;
        isDead = true;
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = transform.localScale;
        }
        Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.07f);
        sr.color = baseColor;
    }
}

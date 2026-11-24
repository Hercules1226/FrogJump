using UnityEngine;

/// <summary>
/// 위로 날아가는 폭탄. 적에 닿으면 터지면서 주변 반경 안의 모든 적에게 스플래시 데미지를 줍니다.
/// 프리팹 없이 ShooterBomb.Create() 로 코드에서 바로 만들어집니다.
/// </summary>
public class ShooterBomb : MonoBehaviour
{
    public float speed = 7f;          // 날아가는 속도 (총알보다 느림)
    public int damage = 3;            // 폭발 데미지
    public float radius = 2.5f;       // 스플래시 반경
    public float lifeTime = 3f;       // 아무것도 못 맞추면 자동 삭제
    public GameObject explosionPrefab;

    private bool exploded;
    private static Sprite cachedSprite;

    /// <summary>폭탄 오브젝트를 코드로 생성합니다.</summary>
    public static ShooterBomb Create(Vector3 position, GameObject explosionPrefab)
    {
        GameObject go = new GameObject("Bomb");
        go.transform.position = position;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite();
        sr.sortingOrder = 5;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        ShooterBomb bomb = go.AddComponent<ShooterBomb>();
        bomb.explosionPrefab = explosionPrefab;
        return bomb;
    }

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 빙글빙글 돌면서 날아가기
        transform.Rotate(0f, 0f, 360f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;
        if (other.GetComponent<ShooterEnemy>() != null) Explode();
    }

    void Explode()
    {
        exploded = true;

        // 반경 안의 모든 적에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            ShooterEnemy enemy = hit.GetComponent<ShooterEnemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        // 폭발 효과: 최종 크기가 대략 스플래시 범위와 같도록 키움
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * (radius * 2f / (0.75f * 2.5f));
            SpriteRenderer fxSr = fx.GetComponent<SpriteRenderer>();
            if (fxSr != null) fxSr.color = new Color(1f, 0.6f, 0.2f);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    // 동그란 폭탄 모양 스프라이트를 코드로 만들기 (한 번만 만들고 재사용)
    static Sprite GetSprite()
    {
        if (cachedSprite != null) return cachedSprite;

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                Color col;
                if (d > 1f) col = Color.clear;
                else if (d > 0.8f) col = new Color(1f, 0.85f, 0.2f);          // 노란 테두리
                else if (x > c && y > c && d < 0.45f && d > 0.2f) col = new Color(0.6f, 0.6f, 0.7f); // 하이라이트
                else col = new Color(0.2f, 0.2f, 0.25f);                        // 검은 몸통
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        cachedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        return cachedSprite;
    }
}

using UnityEngine;

/// <summary>
/// 위로 날아가는 총알. 적에 닿으면 데미지를 주고 사라집니다.
/// </summary>
public class ShooterBullet : MonoBehaviour
{
    public float speed = 14f;     // 날아가는 속도
    public int damage = 1;        // 적에게 주는 데미지
    public float lifeTime = 2f;   // 이 시간이 지나면 자동 삭제 (화면 밖 총알 정리)

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ShooterEnemy enemy = other.GetComponent<ShooterEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

/// <summary>
/// 간단한 폭발 효과: 커지면서 투명해지다가 사라집니다.
/// </summary>
public class ShooterExplosion : MonoBehaviour
{
    public float duration = 0.35f;
    public float growScale = 2.5f;

    private SpriteRenderer sr;
    private Vector3 startScale;
    private float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float p = t / duration;
        transform.localScale = startScale * Mathf.Lerp(0.5f, growScale, p);
        Color c = sr.color;
        c.a = 1f - p;
        sr.color = c;
        if (p >= 1f) Destroy(gameObject);
    }
}

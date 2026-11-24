using UnityEngine;

/// <summary>
/// 배경의 별들이 아래로 흘러가서 우주를 나는 느낌을 줍니다.
/// </summary>
public class ShooterStarfield : MonoBehaviour
{
    public Sprite starSprite;
    public int starCount = 70;
    public float minSpeed = 0.5f;
    public float maxSpeed = 4f;

    private Transform[] stars;
    private float[] speeds;
    private float halfW, halfH;
    private Vector3 center;

    void Start()
    {
        Camera cam = Camera.main;
        halfH = cam.orthographicSize + 0.3f;
        halfW = halfH * cam.aspect;
        center = cam.transform.position;

        stars = new Transform[starCount];
        speeds = new float[starCount];
        for (int i = 0; i < starCount; i++)
        {
            GameObject s = new GameObject("Star");
            s.transform.SetParent(transform);
            s.transform.position = new Vector3(center.x + Random.Range(-halfW, halfW), center.y + Random.Range(-halfH, halfH), 0f);

            SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = starSprite;
            sr.sortingOrder = -10;

            speeds[i] = Random.Range(minSpeed, maxSpeed);
            float k = Mathf.InverseLerp(minSpeed, maxSpeed, speeds[i]);   // 빠른 별일수록 크고 밝게
            s.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.3f, k);
            sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.3f, 1f, k));
            stars[i] = s.transform;
        }
    }

    void Update()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].position += Vector3.down * speeds[i] * Time.deltaTime;
            if (stars[i].position.y < center.y - halfH)
            {
                stars[i].position = new Vector3(center.x + Random.Range(-halfW, halfW), center.y + halfH, 0f);
            }
        }
    }
}

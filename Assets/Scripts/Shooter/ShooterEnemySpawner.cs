using UnityEngine;

/// <summary>
/// 화면 위쪽 랜덤 위치에서 적을 계속 만들어 냅니다.
/// 시간이 지날수록 생성 간격이 짧아져서 점점 어려워집니다.
/// </summary>
public class ShooterEnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float startInterval = 1.2f;     // 처음 생성 간격
    public float minInterval = 0.35f;      // 가장 짧은 생성 간격
    public float intervalDecrease = 0.02f; // 한 번 생성할 때마다 줄어드는 시간
    [Range(0f, 1f)] public float bigEnemyChance = 0.2f;  // 큰 적이 나올 확률

    private float timer;
    private float interval;
    private float halfWidth;
    private float topY;

    void Start()
    {
        interval = startInterval;
        timer = 1f;   // 시작 후 1초 뒤 첫 적 등장

        Camera cam = Camera.main;
        halfWidth = cam.orthographicSize * cam.aspect - 0.7f;
        topY = cam.transform.position.y + cam.orthographicSize + 1f;
    }

    void Update()
    {
        if (ShooterGameManager.Instance.IsGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            interval = Mathf.Max(minInterval, interval - intervalDecrease);
            timer = interval;
        }
    }

    void Spawn()
    {
        float x = Random.Range(-halfWidth, halfWidth);
        GameObject go = Instantiate(enemyPrefab, new Vector3(x, topY, 0f), Quaternion.identity);
        ShooterEnemy enemy = go.GetComponent<ShooterEnemy>();

        enemy.speed = Random.Range(2f, 4f);
        if (Random.value < 0.4f) enemy.swayAmount = Random.Range(0.5f, 1.5f);

        // 가끔 크고 단단한 적 등장!
        if (Random.value < bigEnemyChance)
        {
            enemy.hp = 4;
            enemy.score = 50;
            enemy.speed *= 0.6f;
            go.transform.localScale = Vector3.one * 1.7f;
            go.GetComponent<SpriteRenderer>().color = new Color(1f, 0.7f, 0.3f);
        }
    }
}

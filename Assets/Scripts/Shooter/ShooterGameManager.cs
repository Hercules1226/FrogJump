using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 점수, 목숨, 게임 오버, 재시작을 관리합니다.
/// 다른 스크립트에서 ShooterGameManager.Instance 로 접근합니다. (싱글톤)
/// </summary>
public class ShooterGameManager : MonoBehaviour
{
    public static ShooterGameManager Instance;

    [Header("게임 설정")]
    public int lives = 3;

    [Header("UI 연결")]
    public Text scoreText;
    public Text livesText;
    public Text messageText;

    private int score;
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        messageText.text = "방향키: 이동   스페이스: 발사   C: 폭탄";
        Invoke(nameof(HideMessage), 3f);
    }

    void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        score += amount;
        UpdateUI();
    }

    public void LoseLife()
    {
        if (IsGameOver) return;
        lives--;
        UpdateUI();
        if (lives <= 0) GameOver();
    }

    void GameOver()
    {
        IsGameOver = true;
        CancelInvoke();

        int best = PlayerPrefs.GetInt("ShooterBest", 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt("ShooterBest", best);
        }
        messageText.text = "GAME OVER\n점수: " + score + "   최고: " + best + "\nR 키로 다시 시작";
    }

    void HideMessage()
    {
        messageText.text = "";
    }

    void UpdateUI()
    {
        scoreText.text = "SCORE  " + score;
        livesText.text = "♥ x " + Mathf.Max(0, lives);
    }
}

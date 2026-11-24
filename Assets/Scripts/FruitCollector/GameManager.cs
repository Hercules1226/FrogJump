using System.Collections;   // [스테이지 신규] 코루틴(IEnumerator)을 쓰려면 꼭 필요하다
using UnityEngine;
using UnityEngine.UI;        // UI.Text 를 쓰려면 꼭 필요하다
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체를 관리하는 스크립트입니다.
/// 점수(과일 개수), 하트(목숨), 화면 글자를 담당합니다.
///
/// [이번주 신규] 라이프(하트) 시스템
/// [5주차 복습] 변수와 조건문으로 게임 규칙 만들기
/// [스테이지 신규] 코루틴 - 클리어 후 잠깐 기다렸다가 다음 스테이지로 넘어가기
/// [사운드 신규] AudioSource + AudioClip 으로 효과음 재생하기
/// </summary>
public class GameManager : MonoBehaviour
{
    // static = 게임 전체에 딱 하나만 존재하는 변수.
    // 덕분에 다른 스크립트에서 GameManager.instance 라고 부를 수 있다.
    public static GameManager instance;

    [Header("게임 규칙")]
    public int totalFruits = 5;   // 모아야 하는 과일 개수 (스테이지마다 인스펙터에서 바꾼다!)
    public int startHearts = 3;   // 시작할 때 주어지는 하트 개수

    [Header("스테이지 이동")]
    public float nextStageDelay = 2f;              // 클리어 후 몇 초 기다렸다가 넘어갈까?
    public bool isLastStage = false;               // 마지막 스테이지라면 체크!
    public string mainMenuSceneName = "MainMenu";  // 마지막 스테이지를 깨면 돌아갈 씬

    [Header("화면에 표시할 글자")]
    public Text scoreText;     // 좌측 상단 - 과일 개수
    public Text heartText;     // 좌측 상단 - 하트
    public Text messageText;   // 화면 중앙 - CLEAR / GAME OVER

    [Header("사운드 (Project 창의 소리 파일을 드래그)")]
    public AudioClip jumpSound;    // 점프할 때   - DM-CGS-07
    public AudioClip fruitSound;   // 과일 먹을 때 - DM-CGS-21
    public AudioClip clearSound;   // 클리어할 때 - DM-CGS-26
    public AudioClip failSound;    // 게임 오버   - DM-CGS-25
    public AudioClip hitSound;     // 장애물에 부딪혔을 때 - DM-CGS-35

    private int score;          // 지금까지 모은 과일 개수
    private int hearts;         // 남은 하트 개수
    private bool isGameOver;    // 게임 오버 상태인가? (R 키 재시작은 게임 오버일 때만)
    private AudioSource audioSource;   // 소리를 실제로 재생해주는 '스피커' 부품


    // Awake() : Start() 보다도 먼저 실행된다
    void Awake()
    {
        instance = this;   // "게임 매니저는 나야!" 하고 등록한다
        audioSource = GetComponent<AudioSource>();   // 나에게 붙어있는 스피커를 찾아둔다
    }


    /// <summary>
    /// [사운드 신규] 소리를 한 번 재생한다. 다른 스크립트에서도 불러서 쓸 수 있도록 public!
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;   // 소리 파일을 안 넣었으면 그냥 넘어간다

        // PlayOneShot = 소리를 '한 번' 재생. 이전 소리가 끝나지 않았어도 겹쳐서 재생된다
        audioSource.PlayOneShot(clip);
    }

    void Start()
    {
        Time.timeScale = 1f;   // 이전 판에서 멈춰 있었을 수 있으니 시간을 되돌려 놓는다
        score = 0;
        hearts = startHearts;
        isGameOver = false;
        messageText.text = "";   // 시작할 땐 중앙 메시지를 비워둔다
        UpdateUI();
    }

    void Update()
    {
        // 게임 오버 상태에서 R 키를 누르면 이 스테이지를 다시 시작
        // (클리어했을 때는 자동으로 다음 스테이지로 넘어가므로 R 키가 필요 없다)
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    /// <summary> 과일을 먹었을 때 Fruit 스크립트가 불러주는 함수 </summary>
    public void AddScore()
    {
        score = score + 1;
        UpdateUI();
        PlaySound(fruitSound);   // 과일 먹는 소리

        // 과일을 다 모았으면 클리어!
        if (score >= totalFruits)
        {
            Clear();
        }
    }


    /// <summary> 가시에 닿았을 때 Spike 스크립트가 불러주는 함수 </summary>
    public void LoseHeart()
    {
        hearts = hearts - 1;
        UpdateUI();

        // 하트를 다 잃으면 게임 오버
        if (hearts <= 0)
        {
            GameOver();   // 마지막 하트일 때는 실패 소리만 난다 (부딪힌 소리와 겹치지 않게)
        }
        else
        {
            PlaySound(hitSound);   // 아직 하트가 남아 있으면 부딪힌 소리
        }
    }


    /// <summary> [추가 활동] 과일을 전부 모았는지 알려준다 (StageExit 가 물어본다) </summary>
    public bool IsAllFruitsCollected()
    {
        return score >= totalFruits;
    }


    /// <summary> 화면의 글자를 현재 상태에 맞게 새로 쓴다 </summary>
    void UpdateUI()
    {
        scoreText.text = "과일: " + score + " / " + totalFruits;

        // 남은 하트 개수만큼 하트 기호를 이어붙인다 (반복문 복습!)
        string heartMark = "";
        for (int i = 0; i < hearts; i++)
        {
            heartMark = heartMark + "♥";

        }
        heartText.text = "목숨: " + heartMark;
    }


    void Clear()
    {
        if (isLastStage)
        {
            messageText.text = "ALL CLEAR!\n잠시 후 메인 화면으로";
        }
        else
        {
            messageText.text = "STAGE CLEAR!\n잠시 후 다음 스테이지로";
        }

        Time.timeScale = 0f;   // 게임 시간을 0배속으로 = 완전히 멈춤
        PlaySound(clearSound); // 클리어 소리 (소리는 timeScale 이 0 이어도 재생된다)

        // 코루틴 시작! "기다렸다가 다음 스테이지로 가기" 를 따로 진행시킨다
        StartCoroutine(GoNextStage());
    }


    /// <summary>
    /// [스테이지 신규] 코루틴 함수
    /// 반환형이 IEnumerator 이고, 안에 yield return 이 있으면 코루틴이다.
    /// </summary>
    IEnumerator GoNextStage()
    {
        // 여기서 nextStageDelay 초 동안 '잠깐 쉬었다가' 다음 줄로 넘어간다.
        // WaitForSeconds 는 게임 시간(timeScale) 기준이라 멈춘 상태에서는 영원히 기다린다.
        // 그래서 실제 시계 기준인 WaitForSecondsRealtime 을 쓴다!
        yield return new WaitForSecondsRealtime(nextStageDelay);

        Time.timeScale = 1f;   // 다음 씬에서 게임이 멈춰 있지 않도록 시간을 되돌린다

        if (isLastStage)
        {
            // 마지막 스테이지였다면 메인 화면으로
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            // 지금 씬의 번호(Build Profiles 목록 순서) + 1 = 다음 스테이지
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextIndex);
        }
    }


    void GameOver()
    {
        isGameOver = true;
        messageText.text = "GAME OVER\nR 키로 다시 시작";
        Time.timeScale = 0f;
        PlaySound(failSound);   // 실패 소리
    }
}

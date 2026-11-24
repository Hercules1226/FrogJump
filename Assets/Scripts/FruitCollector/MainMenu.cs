using System.Collections;   // 코루틴(IEnumerator)을 쓰려면 필요하다
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 화면(MainMenu 씬)에 붙는 스크립트입니다.
/// '게임 시작' 버튼을 누르면 첫 번째 스테이지로 이동합니다.
///
/// [스테이지 신규] 버튼의 On Click() 에 public 함수를 연결하기
/// [복습] SceneManager.LoadScene - 다른 씬 불러오기
/// [사운드 신규] 버튼 소리를 '다 듣고 나서' 씬 넘기기 (코루틴 복습!)
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("첫 스테이지 씬 이름 (철자를 정확히!)")]
    public string firstStageName = "Stage1";

    [Header("사운드")]
    public AudioClip buttonSound;   // 버튼 누를 때 - DM-CGS-21

    private AudioSource audioSource;   // 소리를 재생해주는 '스피커' 부품
    private bool isStarting = false;   // 이미 버튼을 눌렀는가? (여러 번 눌러도 한 번만 시작되게)

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 버튼이 불러줄 함수. 버튼의 On Click() 목록에 보이려면 반드시 public 이어야 한다!
    /// </summary>
    public void StartGame()
    {
        if (isStarting) return;   // 이미 시작 중이면 무시
        isStarting = true;

        StartCoroutine(StartAfterSound());
    }

    /// <summary>
    /// 버튼 소리를 재생하고, 소리가 끝날 때까지 기다렸다가 첫 스테이지로 넘어간다.
    ///
    /// 왜 기다릴까? 씬이 바뀌면 지금 씬의 오브젝트(스피커 포함)가 모두 사라져서
    /// 소리가 재생되자마자 뚝 끊기기 때문이다.
    /// </summary>
    IEnumerator StartAfterSound()
    {
        Time.timeScale = 1f;   // 혹시 멈춰 있었다면 시간을 되돌리고

        if (buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);

            // clip.length = 소리 파일의 길이(초). 딱 그만큼만 기다린다
            yield return new WaitForSecondsRealtime(buttonSound.length);
        }

        SceneManager.LoadScene(firstStageName); // 첫 스테이지로!
    }
}

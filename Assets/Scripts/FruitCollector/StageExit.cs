using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [추가 활동] 과일을 다 모은 뒤 이 문에 닿으면 다음 씬으로 넘어갑니다.
///
/// [신규] SceneManager.LoadScene - 다른 씬(스테이지)을 불러오기
///
/// 사용하려면 두 가지 준비가 필요합니다.
///   1. 다음 스테이지 씬을 만들어서 저장하기
///   2. File > Build Profiles 에서 그 씬을 목록에 추가하기
///      (목록에 없는 씬은 LoadScene 으로 불러올 수 없다)
/// </summary>
public class StageExit : MonoBehaviour
{
    [Header("다음 씬 이름 (철자를 정확히!)")]
    public string nextSceneName = "";

    [Header("과일을 다 모아야만 열릴까?")]
    public bool requireAllFruits = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 과일을 다 모아야 하는데 아직 안 모았다면 문이 열리지 않는다
        if (requireAllFruits && !GameManager.instance.IsAllFruitsCollected())
        {
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("다음 씬 이름이 비어 있습니다. 인스펙터에서 입력해주세요.");
            return;
        }

        Time.timeScale = 1f;                    // 혹시 멈춰 있었다면 시간을 되돌리고
        SceneManager.LoadScene(nextSceneName);  // 다음 스테이지로!
    }
}

using UnityEngine;

/// <summary>
/// 과일 1개에 붙는 스크립트입니다.
///
/// [이번주 신규] 콜라이더의 'Is Trigger' 를 켜면
///               물리적으로 부딪히지 않고 '통과하면서 감지만' 하게 된다.
///               이때 호출되는 함수가 OnTriggerEnter2D 이다.
/// </summary>
public class Fruit : MonoBehaviour
{
    // 무언가가 내 트리거 영역 안으로 '들어온 순간' 한 번 실행된다
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 것이 주인공이 아니면 아무 일도 하지 않는다
        if (!other.CompareTag("Player")) return;

        // 점수를 1 올려달라고 게임 매니저에게 부탁한다
        GameManager.instance.AddScore();

        // 나 자신(과일)을 화면에서 없앤다
        Destroy(gameObject);
    }
}

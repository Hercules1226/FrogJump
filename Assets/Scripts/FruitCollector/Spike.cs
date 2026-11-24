using UnityEngine;

/// <summary>
/// 가시에 붙는 스크립트입니다.
///
/// [이번주 신규] 콜라이더의 'Is Trigger' 가 꺼져 있으면
///               실제로 '쿵' 하고 부딪힌다. 이때 OnCollisionEnter2D 가 실행된다.
///
/// 과일(Trigger)과 가시(Collision)의 차이를 비교해보자!
/// </summary>
public class Spike : MonoBehaviour
{
    // 무언가와 '부딪힌 순간' 한 번 실행된다
    void OnCollisionEnter2D(Collision2D collision)
    {
        
        // 부딪힌 것이 주인공이 아니면 아무 일도 하지 않는다
        if (!collision.gameObject.CompareTag("Player")) return;

        // 부딪힌 상대(주인공)의 PlayerController 부품을 찾아서
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        // 시작 위치로 되돌려 보낸다
        player.Respawn();

        // 하트를 1개 깎는다
        GameManager.instance.LoseHeart();
    }
}

using UnityEngine;

/// <summary>
/// 주인공 캐릭터를 조종하는 스크립트입니다.
///
/// [5주차 복습] Update() 안에서 매 프레임 입력을 감지한다
/// [6주차 복습] Input.GetAxisRaw("Horizontal") 로 좌우 입력값을 얻는다
/// [이번주 신규] Rigidbody2D.AddForce() 로 점프한다 (힘을 가한다)
/// [이번주 신규] Animator 파라미터로 애니메이션을 바꾼다
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("움직임 설정")]
    public float moveSpeed = 7f;    // 좌우로 움직이는 속도
    public float jumpPower = 15f;   // 점프할 때 위로 가하는 힘의 크기
    public float fallLimitY = -5f;  // 이 높이보다 아래로 떨어지면 낙사 처리

    // ---------- 나에게 붙어있는 부품(컴포넌트)을 담아둘 변수 ----------
    private Rigidbody2D rb;                  // 물리(중력, 힘) 담당
    private SpriteRenderer spriteRenderer;   // 그림 담당 (좌우 뒤집기에 사용)
    private Animator animator;               // 애니메이션 담당

    // ---------- 현재 상태를 기억해두는 변수 ----------
    private float moveInput;         // 이번 프레임의 좌우 입력값 (-1 왼쪽 / 0 정지 / 1 오른쪽)
    private bool isGrounded;         // 지금 땅을 밟고 있는가?
    private Vector3 startPosition;   // 처음 시작한 위치 (가시에 닿으면 이 자리로 돌아온다)


    // Start() : 게임이 시작될 때 딱 한 번만 실행된다
    void Start()
    {
        // GetComponent<타입>() = "나에게 붙어있는 그 부품을 찾아줘" 라는 뜻
        rb = this.GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;   // 시작 위치를 기억해 둔다
    }


    // Update() : 매 프레임마다 실행된다. 키보드 입력은 여기서 감지한다!
    void Update()
    {
        // (1) 좌우 입력 읽기 - 왼쪽이면 -1, 오른쪽이면 1, 안 누르면 0
        moveInput = Input.GetAxisRaw("Horizontal");

        // (2) 땅을 밟고 있을 때만 스페이스바로 점프할 수 있다
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // (3) 애니메이션과 바라보는 방향을 갱신한다
        UpdateLook();

        // (4) 발판 밖으로 떨어졌다면 다시 시작 위치로 (낙사 처리)
        if (transform.position.y < fallLimitY)
        {
            Respawn();
            GameManager.instance.LoseHeart();
        }
    }


    // FixedUpdate() : 물리 계산 전용. 일정한 시간 간격으로 실행된다.
    void FixedUpdate()
    {
        // 좌우 속도만 내가 정해주고, 위아래 속도(y)는 중력에게 맡긴다.
        //
        // [참고] 6주차에는 transform.Translate 로 움직였지만,
        //        지금은 Rigidbody2D(물리 몸체)가 생겼기 때문에
        //        속도(linearVelocity)를 직접 정해주는 방식이 더 자연스럽다.
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        //transform.Translate(Vector2.right * moveInput * moveSpeed * Time.deltaTime);
    }


    /// <summary> 위쪽 방향으로 '순간적인 힘'을 가해서 점프한다 </summary>
    void Jump()
    {
        isGrounded = false;   // 발이 땅에서 떨어졌다고 표시

        // AddForce = 힘을 가하다
        // Vector2.up = 위쪽 방향 (0, 1)
        // ForceMode2D.Impulse = 한 번에 '툭' 치는 순간적인 힘
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        // [사운드 신규] 점프 소리는 게임 매니저에게 재생을 부탁한다
        GameManager.instance.PlaySound(GameManager.instance.jumpSound);
    }


    /// <summary> 애니메이션 파라미터와 좌우 방향을 갱신한다 </summary>
    void UpdateLook()
    {
        // Animator 에게 현재 상태를 알려주면, Animator 가 알아서 애니메이션을 바꿔준다.
        // Mathf.Abs() = 절댓값. -1 도 1 로 바꿔서 '움직이는 중'을 표현한다.
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);

        // 왼쪽으로 갈 때는 그림을 좌우로 뒤집는다
        if (moveInput < 0) 
            spriteRenderer.flipX = true;
        else if (moveInput > 0) 
            spriteRenderer.flipX = false;
    }


    // ================= 충돌 처리 =================

    /// <summary> 무언가와 '부딪혀 있는 동안' 계속 실행된다 </summary>
    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsStandingOn(collision))
        {
            isGrounded = true;
        }
    }

    /// <summary> 부딪혀 있던 것에서 '떨어지는 순간' 한 번 실행된다 </summary>
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    /// <summary> 내가 그 물체 '위에 올라서 있는지' 확인하는 함수 </summary>
    bool IsStandingOn(Collision2D collision)
    {
        // 발판(Ground) 이 아니면 서 있는 게 아니다
        if (!collision.gameObject.CompareTag("Ground")) return false;

        // 부딪힌 지점의 '법선(normal)' 이 위를 향하면 = 내가 그 위에 서 있다는 뜻 (1 이면 위, -1 이면 밑, 0 이면 옆)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) return true;
        }
        return false;
    }


    /// <summary> 시작 위치로 되돌아간다 (Spike, Enemy 가 이 함수를 불러준다) </summary>
    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;   // 속도를 0으로 초기화하고
        transform.position = startPosition; // 처음 위치로 이동
    }
}

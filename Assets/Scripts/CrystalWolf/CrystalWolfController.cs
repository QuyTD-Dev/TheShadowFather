using UnityEngine;

public class CrystalWolfController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Cài đặt Di Chuyển")]
    public float moveSpeed = 2f;        // Tốc độ chạy
    public float moveDistance = 3f;     // Quãng đường chạy

    [Header("Cài đặt Demo")]
    public float waitBeforeAttack = 1f; // Chạy xong đứng thở 1 giây rồi mới đánh
    public float waitAfterAttack = 1f;  // Đánh xong đứng nghỉ 1 giây rồi mới quay đầu

    // Các biến nội bộ để xử lý logic
    private Vector3 startPos;
    private bool movingRight = true;    // Đang đi sang phải hay trái
    private bool isWaiting = false;     // Có đang đứng yên không
    private bool hasAttacked = false;   // Đã thực hiện cú đánh trong lần đứng yên này chưa
    private float timer = 0f;           // Đồng hồ đếm giờ

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    void Update()
    {
        // LOGIC TRẠNG THÁI: ĐANG ĐỨNG YÊN (WAITING)
        if (isWaiting)
        {
            HandleWaitingSequence();
            return;
        }

        // LOGIC TRẠNG THÁI: ĐANG DI CHUYỂN (MOVING)
        HandleMovement();
    }

    // Xử lý việc chạy qua lại
    void HandleMovement()
    {
        animator.SetBool("IsRunning", true); // Bật animation chạy

        float currentDist = transform.position.x - startPos.x;

        if (movingRight)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            spriteRenderer.flipX = false; // Mặt hướng phải

            // Nếu đi quá quãng đường -> Chuyển sang chế độ Chờ
            if (currentDist > moveDistance)
            {
                StartWaiting();
            }
        }
        else
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            spriteRenderer.flipX = true; // Lật mặt sang trái (nếu sprite gốc hướng phải)

            // Nếu đi lùi quá quãng đường -> Chuyển sang chế độ Chờ
            if (currentDist < -moveDistance)
            {
                StartWaiting();
            }
        }
    }

    // Bắt đầu vào trạng thái đứng yên
    void StartWaiting()
    {
        isWaiting = true;
        hasAttacked = false; // Reset trạng thái chưa đánh
        timer = 0f;          // Reset đồng hồ
        animator.SetBool("IsRunning", false); // Chuyển về Idle
    }

    // Xử lý chuỗi hành động: Đứng -> Đánh -> Đứng -> Quay đầu
    void HandleWaitingSequence()
    {
        timer += Time.deltaTime; // Đếm giờ

        // GIAI ĐOẠN 1: Đợi một chút rồi Tấn Công
        if (!hasAttacked && timer >= waitBeforeAttack)
        {
            PerformAttack();
            hasAttacked = true; // Đánh dấu là đã đánh rồi, không đánh lại liên tục
        }

        // GIAI ĐOẠN 2: Đánh xong, đợi thêm chút nữa rồi Quay Đầu
        // Tổng thời gian chờ = (thời gian trước đánh) + (thời gian sau đánh)
        if (hasAttacked && timer >= (waitBeforeAttack + waitAfterAttack))
        {
            FlipAndMove();
        }
    }

    void PerformAttack()
    {
        // Kích hoạt Trigger tấn công trong Animator
        animator.SetTrigger("Attack");
    }

    void FlipAndMove()
    {
        movingRight = !movingRight; // Đổi hướng
        isWaiting = false;          // Hết chờ, đi tiếp thôi!
    }
}
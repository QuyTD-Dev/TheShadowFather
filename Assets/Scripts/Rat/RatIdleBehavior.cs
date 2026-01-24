using UnityEngine;

public class RatIdleBehavior : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Tấn công")]
    public float attackCooldown = 2f; // Cứ 2 giây cắn 1 phát
    private float lastAttackTime = 0f; // Thời điểm cắn lần cuối

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. Tính khoảng cách
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            // --- XA QUÁ THÌ CHẠY LẠI ---
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            anim.SetBool("isRunning", true);
        }
        else
        {
            // --- GẦN RỒI THÌ ĐỨNG LẠI VÀ ĐÁNH ---
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);

            // Logic Tấn Công:
            // Nếu thời gian hiện tại > thời gian được phép đánh tiếp theo
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        // 2. Quay mặt (Giữ nguyên logic sửa lỗi Moonwalk của bạn)
        FlipTowardsPlayer();
    }

    void Attack()
    {
        // Gửi lệnh "Bấm nút attack" sang Animator
        anim.SetTrigger("attack");

        // Cập nhật lại thời điểm vừa đánh xong
        lastAttackTime = Time.time;
    }

    void FlipTowardsPlayer()
    {
        if (transform.position.x < player.position.x)
        {
            sr.flipX = false; // Player bên phải -> False (vì ảnh gốc nhìn trái)
        }
        else
        {
            sr.flipX = true;  // Player bên trái -> True
        }
    }
}
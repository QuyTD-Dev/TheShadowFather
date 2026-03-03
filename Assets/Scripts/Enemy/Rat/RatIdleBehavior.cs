using UnityEngine;

public class RatIdleBehavior : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public Transform player; // Kéo Elias vào đây, hoặc để trống để nó tự tìm
    public float moveSpeed = 3f;
    public float chaseSpeed = 4f; // Lúc rượt thì chạy nhanh hơn đi tuần
    public float stopDistance = 1.5f;

    [Header("Tuần tra (Scout)")]
    public float patrolRange = 5f; // Đi xa tối đa 5m từ điểm xuất phát
    private Vector2 startPos;      // Vị trí ban đầu
    private bool movingRight = true; // Đang đi sang phải hay trái?

    [Header("Tấn công & Phát hiện")]
    public float detectionRange = 8f; // Khoảng cách nhìn thấy người chơi
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Lưu lại vị trí ban đầu để làm tâm điểm đi tuần
        startPos = transform.position;

        // Nếu quên kéo Player vào, tự tìm Elias bằng Tag (Nhớ đặt Tag "Player" cho Elias)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // 1. KIỂM TRA MỤC TIÊU
        if (player == null)
        {
            // Nếu không có player hoặc player đã chết -> Đi tuần
            Patrol();
            return;
        }

        // Tính khoảng cách tới người chơi
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. LOGIC CHUYỂN TRẠNG THÁI
        if (distToPlayer <= detectionRange)
        {
            // --- TRONG TẦM NHÌN -> RƯỢT ĐUỔI ---
            ChaseAndAttack(distToPlayer);
        }
        else
        {
            // --- XA QUÁ KHÔNG THẤY -> ĐI TUẦN TRA TIẾP ---
            Patrol();
        }
    }

    // --- HÀNH VI 1: ĐI TUẦN TRA (SCOUT) ---
    void Patrol()
    {
        anim.SetBool("isRunning", true); // Chuyển animation chạy

        // Logic di chuyển qua lại
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            sr.flipX = false; // Nhìn phải

            // Nếu đi quá xa về bên phải -> Quay đầu
            if (transform.position.x > startPos.x + patrolRange)
                movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            sr.flipX = true; // Nhìn trái (lật ảnh)

            // Nếu đi quá xa về bên trái -> Quay đầu
            if (transform.position.x < startPos.x - patrolRange)
                movingRight = true;
        }
    }

    // --- HÀNH VI 2: RƯỢT ĐUỔI & TẤN CÔNG ---
    void ChaseAndAttack(float dist)
    {
        // Quay mặt về phía người chơi
        FlipTowardsPlayer();

        if (dist > stopDistance)
        {
            // --- CHẠY LẠI GẦN ---
            Vector2 direction = (player.position - transform.position).normalized;

            // Chỉ di chuyển trục X (để tránh chuột bay lên trời nếu player nhảy)
            rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

            anim.SetBool("isRunning", true);
        }
        else
        {
            // --- ĐỨNG LẠI VÀ CẮN ---
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Dừng lại
            anim.SetBool("isRunning", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        anim.SetTrigger("attack");
        lastAttackTime = Time.time;
    }

    void FlipTowardsPlayer()
    {
        if (transform.position.x < player.position.x)
            sr.flipX = false; // Player bên phải
        else
            sr.flipX = true;  // Player bên trái
    }

    // Vẽ vòng tròn tầm nhìn trong Scene để dễ chỉnh (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Vòng phát hiện

        Gizmos.color = Color.green;
        // Vẽ đường đi tuần tra dự kiến
        if (Application.isPlaying)
            Gizmos.DrawLine(new Vector3(startPos.x - patrolRange, startPos.y, 0), new Vector3(startPos.x + patrolRange, startPos.y, 0));
        else
            Gizmos.DrawLine(new Vector3(transform.position.x - patrolRange, transform.position.y, 0), new Vector3(transform.position.x + patrolRange, transform.position.y, 0));
    }
}
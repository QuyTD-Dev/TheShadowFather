using UnityEngine;

public class CrystalWolfController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    [Header("Cảm Biến Môi Trường")]
    public float detectRange = 6f;      // Tầm nhìn: Thấy Player từ xa 6m
    public float attackRange = 1.5f;    // Tầm đánh: Đứng cách 1.5m là cắn được
    public float heightTolerance = 1f;  // Độ cao chênh lệch cho phép (Sói chỉ đuổi nếu Player không đứng quá cao/thấp hơn nó 1m)

    [Header("Chỉ Số Chiến Đấu")]
    public float moveSpeed = 3f;        // Tốc độ đuổi bắt
    public float attackCooldown = 2f;   // Đánh xong nghỉ 2 giây
    private float lastAttackTime = -999f; // Thời điểm đánh lần cuối (khởi tạo số âm để vào game đánh được ngay)

    [Header("Tuần Tra (Idle Mode)")]
    public Transform[] patrolPoints;    // (Nâng cao) Các điểm đi tuần, tạm thời để trống cũng được
    private Vector3 startPos;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;

        // Tìm Player tự động
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Tính khoảng cách theo các trục
        float distanceX = Mathf.Abs(transform.position.x - player.position.x); // Khoảng cách ngang
        float distanceY = Mathf.Abs(transform.position.y - player.position.y); // Khoảng cách dọc (độ cao)
        float totalDistance = Vector2.Distance(transform.position, player.position);

        // LOGIC TRÍ TUỆ NHÂN TẠO (AI)
        // Điều kiện 1: Player phải ở trong tầm nhìn
        // Điều kiện 2: Player phải đứng cùng mặt đất với Sói (chênh lệch độ cao < heightTolerance)
        bool canSeePlayer = totalDistance < detectRange;
        bool sameGroundLevel = distanceY < heightTolerance;

        if (canSeePlayer && sameGroundLevel)
        {
            EngageTarget(distanceX);
        }
        else
        {
            ReturnToPatrol();
        }
    }

    void EngageTarget(float xDist)
    {
        // Luôn quay mặt về phía Player
        FaceTarget();

        // Nếu khoảng cách X lớn hơn tầm đánh -> CHẠY LẠI GẦN
        // (Trừ đi 0.1f để sói dừng lại sát tầm đánh chứ không đi xuyên qua người)
        if (xDist > attackRange - 0.2f)
        {
            animator.SetBool("IsRunning", true);

            // Di chuyển chỉ trên trục X (Không bay lên trời)
            // Mathf.Sign lấy dấu: nếu Player bên phải trả về 1, bên trái trả về -1
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            // Đã vào tầm đánh -> ĐỨNG LẠI VÀ TẤN CÔNG
            animator.SetBool("IsRunning", false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void ReturnToPatrol()
    {
        // Tạm thời cho đứng yên thở nếu mất dấu Player
        animator.SetBool("IsRunning", false);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time; // Ghi lại giờ vừa đánh xong
    }

    void FaceTarget()
    {
        if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // Player bên phải -> Mặt quay phải
        }
        else
        {
            spriteRenderer.flipX = true;  // Player bên trái -> Lật mặt sang trái
        }
    }

    // Vẽ vùng nhìn thấy trong Scene để dễ chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ giới hạn độ cao
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * heightTolerance, transform.position + Vector3.down * heightTolerance);
    }
}
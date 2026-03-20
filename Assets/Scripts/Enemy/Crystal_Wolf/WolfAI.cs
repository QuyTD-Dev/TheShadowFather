using UnityEngine;
using System.Collections;

public class WolfAI : MonoBehaviour
{
    [Header("Cài Đặt Tuần Tra")]
    public float moveSpeed = 3f;
    public float patrolDistance = 6f;
    public float waitTime = 2f;

    [Header("Cài Đặt Chiến Đấu")]
    public float attackRange = 5f;
    public float stopDistance = 2.5f;
    public float attackCooldown = 1.5f;

    private Animator anim;
    private Transform player;
    private Vector3 startPoint;
    private Vector3 targetPoint;

    private bool isMovingRight = true;
    private bool isWaiting = false;
    private bool isDead = false;
    private float nextAttackTime = 0f;

    // Biến mới giúp quản lý trạng thái mượt mà hơn
    private bool isChasing = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        startPoint = transform.position;
        CalculateTargetPoint();
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            // BƯỚC ĐIỀU TRA 1: In ra màn hình xem Sói đang nhắm vào ai?
            Debug.Log("⚠️ Sói đã tìm thấy mục tiêu mang Tag Player tên là: " + p.name);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        // BƯỚC ĐIỀU TRA 2: Tách biệt hẳn trục X và Y để Sói không bị mù khi Player nhảy/đứng trên dốc
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);

        // NẾU PLAYER TRONG TẦM NGANG VÀ KHÔNG BỊ BAY QUÁ CAO (Sai số chiều cao 3.0f)
        if (distanceX <= attackRange && distanceY <= 3.0f)
        {
            if (!isChasing)
            {
                isChasing = true;
                isWaiting = false;
                StopAllCoroutines();
                Debug.Log("🐺 BẮT ĐẦU RƯỢT ĐUỔI: " + player.name); // Báo hiệu đã nhìn thấy
            }
            EngagePlayer(distanceX);
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                CalculateTargetPoint(); // Mất dấu -> làm lại điểm tuần tra
                Debug.Log("🐺 MẤT DẤU MỤC TIÊU -> QUAY VỀ TUẦN TRA");
            }
            PatrolBehavior();
        }
    }

    void EngagePlayer(float distanceX)
    {
        FacePlayer();

        // Nếu sói rượt mà không cắn, hãy BỎ DẤU // ở dòng dưới để xem Sói tính khoảng cách bị sai số bao nhiêu:
        // Debug.Log($"Đang áp sát... Khoảng cách X hiện tại: {distanceX} | Yêu cầu cắn <= {stopDistance}");

        if (distanceX > stopDistance)
        {
            // Vẫn còn xa -> CHẠY ĐẾN
            anim.SetBool("IsPatrolling", true);
            Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            // ÁP SÁT -> CẮN NGAY
            anim.SetBool("IsPatrolling", false);

            if (Time.time > nextAttackTime)
            {
                anim.SetTrigger("Attack");
                Debug.Log("CHÓP! Sói đã cắn thành công: " + player.name);

                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void PatrolBehavior()
    {
        if (isWaiting) return;

        anim.SetBool("IsPatrolling", true);
        Vector2 patrolTarget = new Vector2(targetPoint.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            StartCoroutine(WaitAndFlip());
        }
    }

    IEnumerator WaitAndFlip()
    {
        isWaiting = true;
        anim.SetBool("IsPatrolling", false);

        yield return new WaitForSeconds(waitTime);

        if (!isDead && !isChasing)
        {
            Flip();
            CalculateTargetPoint();
        }
        isWaiting = false;
    }

    void CalculateTargetPoint()
    {
        if (isMovingRight) targetPoint = startPoint + Vector3.right * patrolDistance;
        else targetPoint = startPoint + Vector3.left * patrolDistance;
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x && !isMovingRight) Flip();
        else if (player.position.x < transform.position.x && isMovingRight) Flip();
    }

    void Flip()
    {
        isMovingRight = !isMovingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void OnDie()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        anim.SetTrigger("Die");
        anim.SetBool("IsPatrolling", false);

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
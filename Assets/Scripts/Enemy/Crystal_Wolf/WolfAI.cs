using UnityEngine;
using System.Collections;

public class WolfAI : MonoBehaviour
{
    [Header("Cài Đặt Tuần Tra")]
    public float moveSpeed = 3f;
    public float patrolDistance = 6f;
    public float waitTime = 2f;

    [Header("Cài Đặt Chiến Đấu")]
    public float attackRange = 2.5f;
    public float stopDistance = 1.8f;
    public float attackCooldown = 1.5f;

    private Animator anim;
    private Transform player;
    private Vector3 startPoint;
    private Vector3 targetPoint;

    private bool isMovingRight = true;
    private bool isWaitingAtPatrolPoint = false; // Đổi tên biến cho rõ nghĩa
    private float lastAttackTime = -999f;

    void Start()
    {
        anim = GetComponent<Animator>();
        startPoint = transform.position;
        CalculateTargetPoint();
        FindPlayer();
    }

    void Update()
    {
        if (player == null) FindPlayer();

        float distanceToPlayer = 9999f;
        if (player != null) distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- MÁY TRẠNG THÁI ---

        // 1. TRẠNG THÁI TẤN CÔNG
        if (player != null && distanceToPlayer <= attackRange)
        {
            // Reset trạng thái tuần tra để tránh lỗi kẹt khi quay lại
            isWaitingAtPatrolPoint = false;
            AttackBehavior(distanceToPlayer);
        }
        // 2. TRẠNG THÁI TUẦN TRA
        else
        {
            PatrolBehavior();
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void PatrolBehavior()
    {
        // Nếu đang nghỉ tại điểm tuần tra thì không đi
        if (isWaitingAtPatrolPoint)
        {
            anim.SetBool("IsPatrolling", false);
            return;
        }

        anim.SetBool("IsPatrolling", true);

        // Di chuyển
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        // Kiểm tra đến đích
        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            StartCoroutine(WaitAndFlip());
        }
    }

    void AttackBehavior(float distance)
    {
        // Khi đánh thì tắt animation chạy
        anim.SetBool("IsPatrolling", false);

        FacePlayer();

        // Chỉ di chuyển tiếp cận nếu còn xa, nếu gần rồi (stopDistance) thì đứng yên cắn
        if (distance > stopDistance)
        {
            anim.SetBool("IsPatrolling", true); // Bật chạy để tiếp cận
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }

        // Logic đòn đánh
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack");
        }
    }

    IEnumerator WaitAndFlip()
    {
        isWaitingAtPatrolPoint = true; // Bắt đầu nghỉ
        anim.SetBool("IsPatrolling", false);

        yield return new WaitForSeconds(waitTime);

        // Chỉ lật và đi tiếp nếu KHÔNG đang đánh nhau
        // (Nếu đang đánh nhau thì biến này đã bị set false ở Update rồi)
        Flip();
        CalculateTargetPoint();
        isWaitingAtPatrolPoint = false; // Hết nghỉ
    }

    void CalculateTargetPoint()
    {
        if (isMovingRight) targetPoint = startPoint + Vector3.right * patrolDistance;
        else targetPoint = startPoint + Vector3.left * patrolDistance;
    }

    void FacePlayer()
    {
        if (player == null) return;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
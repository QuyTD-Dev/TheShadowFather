using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Cài đặt Debug")]
    public bool showLogs = true; // Tích vào đây để bật log, bỏ tích để tắt cho đỡ rối

    [Header("Cài đặt chung")]
    public Transform player;
    public float moveSpeed = 2f;

    [Header("Phạm vi cảm biến")]
    public float chaseRange = 5f;
    public float attackRange = 1.5f;

    [Header("Cài đặt Tuần Tra")]
    public float patrolDistance = 3f;
    private Vector3 startPosition;
    private bool isMovingRight = true;

    [Header("Thời gian hồi chiêu")]
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private Animator anim;
    private float distanceToPlayer;
    private bool isAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        startPosition = transform.position;

        if (showLogs) Debug.Log($"[START] Quái sinh ra tại: {startPosition}. Phạm vi tuần tra: {patrolDistance}m");

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (showLogs) Debug.Log("[START] Đã tự động tìm thấy Player!");
            }
            else
            {
                if (showLogs) Debug.LogError("[ERROR] Không tìm thấy Player! Hãy kiểm tra Tag 'Player'.");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Log khoảng cách mỗi 60 frame (để đỡ spam console)
        if (showLogs && Time.frameCount % 60 == 0)
            Debug.Log($"[UPDATE] Khoảng cách tới Player: {distanceToPlayer:F2}m");

        if (isAttacking) return;

        // --- LOGIC HÀNH VI ---

        if (distanceToPlayer <= attackRange)
        {
            if (showLogs && Time.frameCount % 60 == 0) Debug.Log("-> Trạng thái: TẤN CÔNG");
            AttackPlayer();
        }
        else if (distanceToPlayer <= chaseRange)
        {
            if (showLogs && Time.frameCount % 60 == 0) Debug.Log("-> Trạng thái: ĐUỔI THEO");
            ChasePlayer();
        }
        else
        {
            // Chỉ log Patrol khi chuyển hướng để đỡ spam
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetFloat("Speed", 1f);

        float targetX;

        if (isMovingRight)
        {
            targetX = startPosition.x + patrolDistance;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(targetX, transform.position.y), moveSpeed * Time.deltaTime);
            transform.localScale = new Vector3(1, 1, 1);

            // Log vị trí khi đi tuần
            // if (showLogs) Debug.Log($"[PATROL] Đang đi sang PHẢI. Vị trí: {transform.position.x:F2} / Mục tiêu: {targetX}");

            if (transform.position.x >= targetX - 0.1f)
            {
                isMovingRight = false;
                if (showLogs) Debug.Log($"[PATROL] Đã chạm biên PHẢI ({transform.position.x:F2}). Quay đầu sang TRÁI.");
            }
        }
        else
        {
            targetX = startPosition.x - patrolDistance;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(targetX, transform.position.y), moveSpeed * Time.deltaTime);
            transform.localScale = new Vector3(-1, 1, 1);

            // if (showLogs) Debug.Log($"[PATROL] Đang đi sang TRÁI. Vị trí: {transform.position.x:F2} / Mục tiêu: {targetX}");

            if (transform.position.x <= targetX + 0.1f)
            {
                isMovingRight = true;
                if (showLogs) Debug.Log($"[PATROL] Đã chạm biên TRÁI ({transform.position.x:F2}). Quay đầu sang PHẢI.");
            }
        }
    }

    void ChasePlayer()
    {
        // Quay mặt
        if (transform.position.x < player.position.x) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);

        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        anim.SetFloat("Speed", 1f);
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            // Log lý do chưa đánh
            // if (showLogs) Debug.Log($"[ATTACK] Đang hồi chiêu... Còn {attackCooldown - (Time.time - lastAttackTime):F1}s");
            anim.SetFloat("Speed", 0f);
            return;
        }

        if (showLogs) Debug.LogWarning("[ACTION] BÙM! Kích hoạt Attack!");
        anim.SetTrigger("Attack");
        lastAttackTime = Time.time;
        StartCoroutine(FreezeMovement(1f));
    }

    System.Collections.IEnumerator FreezeMovement(float seconds)
    {
        if (showLogs) Debug.Log($"[WAIT] Đứng yên {seconds}s để diễn hoạt cảnh đánh.");
        isAttacking = true;
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
        if (showLogs) Debug.Log("[WAIT] Đã diễn xong. Tiếp tục hành động.");
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Vẽ phạm vi đánh
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ phạm vi TUẦN TRA (Màu xanh lá) - Chỉ vẽ khi game đang chạy (vì cần startPosition)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(startPosition.x - patrolDistance, startPosition.y, 0),
                            new Vector3(startPosition.x + patrolDistance, startPosition.y, 0));
        }
    }
}

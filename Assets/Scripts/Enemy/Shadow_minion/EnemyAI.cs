using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Cài đặt Debug")]
    public bool showLogs = true;

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

    [Header("Sát thương cận chiến")]
    [Tooltip("Damage mỗi đòn đánh")]
    public int meleeDamage = 15;
    [Tooltip("Kích thước hitbox tấn công")]
    public Vector2 hitboxSize = new Vector2(0.8f, 0.8f);
    [Tooltip("Offset hitbox (phía trước quái)")]
    public float hitboxOffsetX = 0.6f;

    private Animator anim;
    private float distanceToPlayer;
    private bool isAttacking = false;
    private GameObject attackHitbox;

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

        // === TỰ ĐỘNG TẠO ATTACK HITBOX ===
        CreateAttackHitbox();
    }

    /// <summary>
    /// Tự tạo child object AttackHitbox nếu chưa có.
    /// </summary>
    private void CreateAttackHitbox()
    {
        // Kiểm tra nếu đã có sẵn
        Transform existing = transform.Find("AttackHitbox");
        if (existing != null)
        {
            attackHitbox = existing.gameObject;
            attackHitbox.SetActive(false);
            return;
        }

        attackHitbox = new GameObject("AttackHitbox");
        attackHitbox.transform.SetParent(transform, false);
        attackHitbox.transform.localPosition = new Vector3(hitboxOffsetX, 0f, 0f);

        BoxCollider2D col = attackHitbox.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = hitboxSize;

        Rigidbody2D rb = attackHitbox.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        EnemyMeleeAttack melee = attackHitbox.AddComponent<EnemyMeleeAttack>();
        melee.damage = meleeDamage;

        attackHitbox.SetActive(false); // Mặc định tắt
        if (showLogs) Debug.Log($"[EnemyAI] Tự tạo AttackHitbox cho {gameObject.name}");
    }

    void Update()
    {
        if (player == null) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAttacking) return;

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
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

            if (transform.position.x <= targetX + 0.1f)
            {
                isMovingRight = true;
                if (showLogs) Debug.Log($"[PATROL] Đã chạm biên TRÁI ({transform.position.x:F2}). Quay đầu sang PHẢI.");
            }
        }
    }

    void ChasePlayer()
    {
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
            anim.SetFloat("Speed", 0f);
            return;
        }

        if (showLogs) Debug.LogWarning("[ACTION] BÙM! Kích hoạt Attack!");
        anim.SetTrigger("Attack");
        lastAttackTime = Time.time;
        StartCoroutine(AttackSequence(0.8f));
    }

    System.Collections.IEnumerator AttackSequence(float seconds)
    {
        isAttacking = true;

        // Bật hitbox khi animation đánh
        if (attackHitbox != null)
            attackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.3f); // Hitbox bật trong 0.3s (frame đánh trúng)

        // Tắt hitbox
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        yield return new WaitForSeconds(seconds - 0.3f); // Chờ phần còn lại

        isAttacking = false;
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

using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("---- Cài đặt chung ----")]
    public Transform player;
    private Animator anim;
    private bool isFlipped = false;
    private Vector2 startPosition; // Điểm gốc để tuần tra

    [Header("---- Chỉ số Boss ----")]
    public float maxHealth = 200f;
    [SerializeField] private float currentHealth;
    public float moveSpeed = 2.5f;

    [Header("---- PHẠM VI (RANGES) ----")]
    [Tooltip("Boss đi qua lại trái phải bao nhiêu mét tính từ điểm xuất phát")]
    public float patrolRange = 4.0f;

    [Tooltip("Tầm nhìn phát hiện người chơi")]
    public float detectionRange = 8.0f;

    [Tooltip("Tầm xa để dùng Skill")]
    public float skillRange = 5.0f;

    [Tooltip("Tầm đánh gần (Cận chiến)")]
    public float attackRange = 1.5f; // Đã tăng nhẹ lên 1.5 để dễ trúng hơn

    // Biến nội bộ tuần tra
    private float patrolTargetX;
    private float roamTimer = 0;
    private bool movingRight = true;

    // --- LOGIC COMBO ---
    private int attackCounter = 0;
    private int hitsToSkill = 3;

    // Trạng thái
    private bool isEnraged = false;
    private bool isDead = false;
    private float cooldownTimer = 0;

    [Header("---- Hiệu Ứng Skill ----")]
    public GameObject skill1Prefab;
    public Transform firePoint1;
    public GameObject skill2Prefab;
    public Transform firePoint2;

    [Header("---- DEBUG ----")]
    [Tooltip("Tích vào để xem khoảng cách trong Console")]
    public bool showDebugLogs = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) Debug.LogError("LỖI: Thiếu Animator!");

        // Lưu vị trí nhà & Setup tuần tra
        startPosition = transform.position;
        patrolTargetX = startPosition.x + patrolRange;

        currentHealth = maxHealth;

        // Random combo đầu tiên
        hitsToSkill = Random.Range(3, 6);

        // Tự tìm Player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. TÍNH KHOẢNG CÁCH (FIX LỖI KẸT ĐỘ CAO) ---
        float distanceX = 1000f;      // Khoảng cách theo trục Ngang (Quan trọng nhất)
        float realDistance = 1000f;   // Khoảng cách thực tế (Để tham khảo)

        if (player != null)
        {
            // Mathf.Abs: Trị tuyệt đối, giúp tính khoảng cách ngang dù Player ở bên trái hay phải
            distanceX = Mathf.Abs(transform.position.x - player.position.x);
            realDistance = Vector2.Distance(transform.position, player.position);
        }

        // --- 2. LOG DEBUG ---
        if (showDebugLogs)
        {
            string comboInfo = $"Combo: {attackCounter}/{hitsToSkill}";
            string status = "Đi tuần";
            if (distanceX <= attackRange) status = "<color=red>CẬN CHIẾN</color>";
            else if (distanceX <= detectionRange) status = "<color=yellow>ĐUỔI THEO</color>";

            // In ra cả X-Dist và Real-Dist để so sánh
            Debug.Log($"[BOSS] X-Dist: {distanceX:F2} (Real: {realDistance:F2}) | {status} | {comboInfo}");
        }

        // --- 3. XỬ LÝ HỒI CHIÊU ---
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            anim.SetFloat("Speed", 0f); // Đứng nghỉ
        }
        else
        {
            // --- 4. LOGIC AI ---

            // TRƯỜNG HỢP A: CÓ NGƯỜI TRONG TẦM NHÌN (Tính theo chiều ngang)
            if (distanceX <= detectionRange)
            {
                LookAtX(player.position.x);

                // A1. TẦM ĐÁNH (CẬN CHIẾN)
                if (distanceX <= attackRange)
                {
                    // Logic Combo: Đánh thường đủ số lần -> Tung Skill
                    if (attackCounter >= hitsToSkill)
                    {
                        if (showDebugLogs) Debug.Log(">> COMBO FULL: Tung Skill!");
                        anim.SetTrigger("Skill");
                        cooldownTimer = 4.0f;

                        // Reset Combo
                        attackCounter = 0;
                        hitsToSkill = Random.Range(3, 6);
                    }
                    else
                    {
                        if (showDebugLogs) Debug.Log($">> Đánh thường ({attackCounter + 1})");
                        anim.SetTrigger("Attack");
                        cooldownTimer = 2.0f;
                        attackCounter++;
                    }
                }
                // A2. TẦM XA (SKILL RANGE)
                else if (distanceX <= skillRange)
                {
                    // Có thể tắt dòng này nếu muốn Boss luôn chạy lại gần mới đánh
                    anim.SetTrigger("Skill");
                    cooldownTimer = 3.5f;
                }
                // A3. CHƯA TỚI TẦM -> CHẠY LẠI GẦN
                else
                {
                    MoveToX(player.position.x, moveSpeed * (isEnraged ? 1.5f : 1f));
                    anim.SetFloat("Speed", 1f);
                }
            }
            // TRƯỜNG HỢP B: KHÔNG THẤY AI -> ĐI TUẦN TRA
            else
            {
                PatrolLeftRight();
            }
        }
    }

    // --- CÁC HÀM DI CHUYỂN & HỖ TRỢ ---

    void PatrolLeftRight()
    {
        float dist = Mathf.Abs(transform.position.x - patrolTargetX);

        if (dist > 0.2f)
        {
            MoveToX(patrolTargetX, moveSpeed * 0.5f);
            LookAtX(patrolTargetX);
            anim.SetFloat("Speed", 1f);
        }
        else
        {
            anim.SetFloat("Speed", 0f);
            roamTimer -= Time.deltaTime;

            if (roamTimer <= 0)
            {
                if (movingRight)
                {
                    patrolTargetX = startPosition.x - patrolRange;
                    movingRight = false;
                }
                else
                {
                    patrolTargetX = startPosition.x + patrolRange;
                    movingRight = true;
                }
                roamTimer = Random.Range(2f, 4f);
            }
        }
    }

    // Chỉ di chuyển trục X, giữ nguyên Y
    void MoveToX(float targetX, float speed)
    {
        float newX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void LookAtX(float targetX)
    {
        if (transform.position.x > targetX && !isFlipped) Flip();
        else if (transform.position.x < targetX && isFlipped) Flip();
    }

    void Flip()
    {
        isFlipped = !isFlipped;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- LOGIC NHẬN DAMAGE & SKILL ---

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (currentHealth <= 0 && !isEnraged) StartPhase2();
        else if (currentHealth <= 0 && isEnraged) Die();
    }

    public void StartPhase2()
    {
        //currentHealth = maxHealth;
        //isEnraged = true;
        //anim.SetTrigger("ToPhase2");
        //anim.SetBool("IsPhase2", true);

        Debug.Log("Controller: Kích hoạt Animation Biến hình");
        anim.SetTrigger("ToPhase2");
        anim.SetBool("IsPhase2", true);
        moveSpeed *= 1.5f; // Tăng tốc

        // Reset combo
        attackCounter = 0;
        hitsToSkill = Random.Range(3, 6);
    }

    public void Die()
    {
        isDead = true;
        anim.SetTrigger("Dead");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    public void CastSkill1()
    {
        if (skill1Prefab != null && firePoint1 != null)
            Instantiate(skill1Prefab, firePoint1.position, firePoint1.rotation);
    }

    public void CastSkill2()
    {
        Transform fp = (firePoint2 != null) ? firePoint2 : firePoint1;
        if (skill2Prefab != null) Instantiate(skill2Prefab, fp.position, fp.rotation);
    }

    // --- VẼ GIZMOS ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ đường đi tuần
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.DrawLine(new Vector3(center.x - patrolRange, center.y, 0), new Vector3(center.x + patrolRange, center.y, 0));
    }
}

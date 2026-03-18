using UnityEngine;
using System.Collections;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;
    private Rigidbody2D rb;

    [Header("Hệ thống Máu & Phase")]
    public int maxHp = 1000;
    public int currentHp;
    public enum BossPhase { Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;
    private int phase2Threshold;
    private int phase3Threshold;

    [Header("Cài đặt Phase 1 & 3")]
    public float meleeRange = 4.0f;
    public float attackCooldown = 3.0f;

    [Header("Vũ khí & Sát thương")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject meleeHitbox;

    [Header("Đồng bộ Hoạt ảnh Ground")]
    public float shootDelay = 0.5f;
    public float meleeDelay = 0.4f;
    public float meleeDuration = 0.2f;

    [Header("Cài đặt Phase 2 (Trên Không)")]
    public Transform aerialFlyPoint;
    public float takeoffSpeed = 5f;
    public float aerialAttackCooldown = 2.0f;

    [Header("Cài đặt Phase 3 (Bẫy Lửa)")]
    // Kéo các object FireTrap trên map vào mảng này trong Inspector
    public FireTrap[] fireTraps;

    public enum BossState { Idle, Attacking, Transitioning, Flying, Dead }
    public BossState currentState = BossState.Idle;

    private float stateTimer = 2.0f;
    private float roarTimer = 10f;
    private float aerialAttackTimer;

    // Biến lưu trữ Coroutine để có thể Stop chúng khi đổi Phase đột ngột
    private Coroutine currentAttackRoutine;
    private Coroutine flyRoutine;

    // Lưu trữ thông số Vật lý gốc
    private float originalGravity;
    private RigidbodyType2D originalBodyType;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            originalGravity = rb.gravityScale;
            originalBodyType = rb.bodyType; // Lưu lại kiểu Body (thường là Dynamic)
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        currentHp = maxHp;
        phase2Threshold = (int)(maxHp * 0.7f); // 70%
        phase3Threshold = (int)(maxHp * 0.4f); // 40%
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null) return;

        CheckPhaseTransition();

        // Bấm T để trừ máu test (chỉ có tác dụng ở Phase 1 và 3)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(100);
        }

        // Bấm Y để giả lập quái nhỏ chết, trừ máu Boss ở Phase 2
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TakeDamageFromMinion(100);
        }

        switch (currentState)
        {
            case BossState.Idle:
                if (currentPhase != BossPhase.Phase2) GroundCombatLogic();
                break;

            case BossState.Attacking:
                if (stateTimer > 0) stateTimer -= Time.deltaTime;
                else ChangeState(BossState.Idle, 0f);
                break;

            case BossState.Flying:
                if (currentPhase == BossPhase.Phase2) AerialCombatLogic();
                break;

            case BossState.Transitioning:
                // Đang bay lên, không chạy AI
                break;
        }
    }

    void GroundCombatLogic()
    {
        roarTimer -= Time.deltaTime;
        LookAtPlayer();
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= meleeRange)
        {
            anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, attackCooldown);
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
        }
        else
        {
            if (roarTimer <= 0)
            {
                anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, attackCooldown + 1f);
                roarTimer = 10f;
            }
            else
            {
                anim.SetTrigger("Ranged");
                ChangeState(BossState.Attacking, attackCooldown);
                currentAttackRoutine = StartCoroutine(ShootRoutine());
            }
        }
    }

    void AerialCombatLogic()
    {
        LookAtPlayer();

        if (aerialAttackTimer > 0)
        {
            aerialAttackTimer -= Time.deltaTime;
        }
        else
        {
            anim.SetTrigger("FlyingAttack");
            currentAttackRoutine = StartCoroutine(ShootRoutine());
            aerialAttackTimer = aerialAttackCooldown;
            Debug.Log("Boss bắn tỉa từ trên không!");
        }
    }

    // Đòn đánh của Player gọi vào hàm này (Bất tử ở Phase 2)
    public void TakeDamage(int damageAmount)
    {
        if (currentState == BossState.Dead || currentPhase == BossPhase.Phase2) return;
        ApplyDamage(damageAmount);
    }

    // Quái nhỏ dưới đất khi chết sẽ gọi hàm này (Được phép trừ máu ở Phase 2)
    public void TakeDamageFromMinion(int damageAmount)
    {
        if (currentState == BossState.Dead) return;
        ApplyDamage(damageAmount);
    }

    private void ApplyDamage(int amount)
    {
        currentHp -= amount;
        Debug.Log("Boss HP: " + currentHp);
        if (currentHp <= 0) Die();
    }

    void CheckPhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1 && currentHp <= phase2Threshold)
        {
            StartPhase2Transition();
        }
        else if (currentPhase == BossPhase.Phase2 && currentHp <= phase3Threshold)
        {
            StartPhase3Transition();
        }
    }

    void StartPhase2Transition()
    {
        Debug.Log("<color=magenta>PHASE 2 BẮT ĐẦU: XÓA TRỌNG LỰC & CẤT CÁNH!</color>");
        currentPhase = BossPhase.Phase2;

        // Dừng ngay lập tức các đòn đánh đang dang dở
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        // Chuyển Vật Lý sang Kinematic để loại bỏ hoàn toàn cản trở từ môi trường
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        anim.SetTrigger("TakeOff");
        anim.SetBool("IsFlying", true);

        ChangeState(BossState.Transitioning, 0f);

        // Quản lý tiến trình bay để có thể hủy nếu đổi phase
        if (flyRoutine != null) StopCoroutine(flyRoutine);
        flyRoutine = StartCoroutine(FlyUpToStationRoutine());
    }

    IEnumerator FlyUpToStationRoutine()
    {
        if (aerialFlyPoint == null)
        {
            Debug.LogError("Chưa gán điểm FlyPoint_Phase2!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        while (Vector3.Distance(transform.position, aerialFlyPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                aerialFlyPoint.position,
                takeoffSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = aerialFlyPoint.position;
        ChangeState(BossState.Flying, 0f);
        aerialAttackTimer = 1f;
        Debug.Log("Đã lơ lửng thành công!");
    }

    void StartPhase3Transition()
    {
        Debug.Log("<color=red>PHASE 3 BẮT ĐẦU: TRẢ LẠI VẬT LÝ, RƠI XUỐNG VÀ BẬT BẪY!</color>");
        currentPhase = BossPhase.Phase3;

        // BẮT BUỘC: Dừng tiến trình bay nếu nó vẫn đang chạy
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (flyRoutine != null) StopCoroutine(flyRoutine);

        anim.SetBool("IsFlying", false);

        // Trả lại Vật lý như cũ để Boss rơi bịch xuống đất
        if (rb != null)
        {
            rb.bodyType = originalBodyType; // Trả lại Dynamic
            rb.gravityScale = originalGravity; // Trả lại trọng lực
        }

        // Kích hoạt bẫy
        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
            {
                if (trap != null) trap.ActivateTrap();
            }
        }

        ChangeState(BossState.Idle, 1f); // Reset về Idle để kích hoạt lại GroundCombatLogic
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else
            transform.eulerAngles = new Vector3(0, 180, 0);
    }

    IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(shootDelay);
        if (firePoint != null && bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        yield return new WaitForSeconds(meleeDelay);
        if (meleeHitbox != null)
        {
            meleeHitbox.SetActive(true);
            yield return new WaitForSeconds(meleeDuration);
            meleeHitbox.SetActive(false);
        }
    }

    void ChangeState(BossState newState, float waitTime)
    {
        currentState = newState;
        stateTimer = waitTime;
    }

    void Die()
    {
        ChangeState(BossState.Dead, 0f);
        anim.SetTrigger("Die");
        anim.SetBool("IsFlying", false);

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            rb.gravityScale = originalGravity;
        }

        // Tắt bẫy khi Boss chết
        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
            {
                if (trap != null) trap.DeactivateTrap();
            }
        }

        Debug.Log("Boss đã bị tiêu diệt!");
    }
}
using UnityEngine;
using System.Collections;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;
    private Rigidbody2D rb;
    private Collider2D[] bossColliders;

    [Header("Hệ thống Máu & Phase")]
    public int maxHp = 1000;
    public int currentHp;
    public enum BossPhase { Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;
    private int phase2Threshold;
    private int phase3Threshold;

    // CỜ ĐÁNH DẤU CHỐNG NHẢY PHASE
    private bool isPhase2Triggered = false;
    private bool isPhase3Triggered = false;

    [Header("Cài đặt Phase 1")]
    public float meleeRange = 4.0f;
    public float attackCooldown = 3.0f;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject meleeHitbox;
    public float shootDelay = 0.5f;
    public float meleeDelay = 0.4f;
    public float meleeDuration = 0.2f;

    [Header("Cài đặt Phase 2 (Trên Không)")]
    public Transform aerialFlyPoint;
    public float takeoffSpeed = 8f;
    public float aerialAttackCooldown = 2.0f;

    [Header("Cài đặt Phase 3 (Bẫy Lửa & Cuồng Nộ)")]
    public FireTrap[] fireTraps;
    public float enragedAttackCooldown = 1.5f;
    public float enragedRoarCooldown = 5f;

    public enum BossState { Idle, Attacking, Transitioning, Flying, Dead }
    public BossState currentState = BossState.Idle;

    private float stateTimer = 2.0f;
    private float roarTimer = 10f;
    private float aerialAttackTimer;

    private Coroutine currentAttackRoutine;
    private Coroutine flyRoutine;
    private float originalGravity;
    private RigidbodyType2D originalBodyType;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossColliders = GetComponents<Collider2D>();

        if (rb != null)
        {
            originalGravity = rb.gravityScale;
            originalBodyType = rb.bodyType;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        // Chốt cứng thông số
        currentHp = maxHp;
        phase2Threshold = 700;
        phase3Threshold = 400;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) TakeDamage(100);
        if (Input.GetKeyDown(KeyCode.Y)) TakeDamageFromMinion(100);

        if (currentState == BossState.Dead || player == null) return;

        CheckPhaseTransition();

        switch (currentState)
        {
            case BossState.Idle:
                if (currentPhase == BossPhase.Phase1) GroundCombatLogic();
                else if (currentPhase == BossPhase.Phase3) EnragedGroundCombatLogic();
                break;

            case BossState.Attacking:
                if (stateTimer > 0) stateTimer -= Time.deltaTime;
                else ChangeState(BossState.Idle, 0f);
                break;

            case BossState.Flying:
                if (currentPhase == BossPhase.Phase2) AerialCombatLogic();
                break;

            case BossState.Transitioning:
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
        }
    }

    void EnragedGroundCombatLogic()
    {
        roarTimer -= Time.deltaTime;
        LookAtPlayer();
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= meleeRange)
        {
            anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, enragedAttackCooldown);
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
        }
        else
        {
            if (roarTimer <= 0)
            {
                anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, enragedAttackCooldown + 1f);
                roarTimer = enragedRoarCooldown;
            }
            else
            {
                anim.SetTrigger("Ranged");
                ChangeState(BossState.Attacking, enragedAttackCooldown);
                currentAttackRoutine = StartCoroutine(EnragedShootRoutine());
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // Nhắc nhở: Đòn đánh thường vô hiệu hóa trong Phase 2!
        if (currentState == BossState.Dead || currentPhase == BossPhase.Phase2)
        {
            Debug.Log("Boss đang ở Phase 2 (trên không), miễn nhiễm đòn đánh thường!");
            return;
        }
        ApplyDamage(damageAmount);
    }

    public void TakeDamageFromMinion(int damageAmount)
    {
        if (currentState == BossState.Dead) return;
        ApplyDamage(damageAmount);
    }

    private void ApplyDamage(int amount)
    {
        currentHp -= amount;
        Debug.Log(">>> Boss bị đánh! Máu còn: " + currentHp);
        if (currentHp <= 0) Die();
    }

    // ==================== HỆ THỐNG CHUYỂN PHASE (ĐÃ SỬA LỖI) ====================

    void CheckPhaseTransition()
    {
        // 1. Kiểm tra Phase 3 TRƯỚC TIÊN (Ưu tiên cao nhất, cắt đứt mọi logic)
        if (currentHp <= phase3Threshold && !isPhase3Triggered)
        {
            isPhase3Triggered = true;
            StartPhase3Transition();
            return; // Dừng hàm tại đây, chắc chắn không kiểm tra các điều kiện bên dưới nữa
        }

        // 2. Kiểm tra độc lập Phase 2
        if (currentHp <= phase2Threshold && currentHp > phase3Threshold && !isPhase2Triggered)
        {
            isPhase2Triggered = true;
            StartPhase2Transition();
        }
    }

    void StartPhase2Transition()
    {
        Debug.Log("<color=magenta>KÍCH HOẠT PHASE 2: CẤT CÁNH!</color>");
        currentPhase = BossPhase.Phase2;

        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (Collider2D col in bossColliders)
            if (col != null) col.enabled = false;

        anim.SetTrigger("TakeOff");
        anim.SetBool("IsFlying", true);
        ChangeState(BossState.Transitioning, 0f);

        if (flyRoutine != null) StopCoroutine(flyRoutine);
        flyRoutine = StartCoroutine(FlyUpToStationRoutine());
    }

    IEnumerator FlyUpToStationRoutine()
    {
        if (takeoffSpeed <= 0f) takeoffSpeed = 8f;
        Vector3 targetPos = (aerialFlyPoint != null) ?
            new Vector3(aerialFlyPoint.position.x, aerialFlyPoint.position.y, transform.position.z) :
            transform.position + new Vector3(0, 5f, 0);

        yield return new WaitForSeconds(0.5f);

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, takeoffSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        ChangeState(BossState.Flying, 0f);
        aerialAttackTimer = 1f;
        Debug.Log("<color=magenta>ĐĐA ĐẠT ĐỘ CAO PHASE 2!</color>");
    }

    void StartPhase3Transition()
    {
        Debug.Log("<color=red>KÍCH HOẠT PHASE 3: RƠI XUỐNG VÀ BẬT BẪY!</color>");

        // Gán trạng thái ngay lập tức
        currentPhase = BossPhase.Phase3;

        // 1. Dừng ngay lập tức các hành động trên không
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (flyRoutine != null) StopCoroutine(flyRoutine);

        // 2. Xử lý Animator an toàn
        anim.ResetTrigger("FlyingAttack");
        anim.SetBool("IsFlying", false);

        // 3. Phục hồi vật lý để Boss rơi xuống đất
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = (originalGravity > 0) ? originalGravity : 3f;
        }

        foreach (Collider2D col in bossColliders)
            if (col != null) col.enabled = true;

        StartCoroutine(Phase3DelayTrapRoutine());
    }

    IEnumerator Phase3DelayTrapRoutine()
    {
        ChangeState(BossState.Transitioning, 0f);
        yield return new WaitForSeconds(1.5f);

        Debug.Log("<color=red>BOSS ĐÃ CHẠM ĐẤT - MƯA BOM BẮT ĐẦU!</color>");
        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
                if (trap != null) trap.ActivateTrap();
        }

        ChangeState(BossState.Idle, 1f);
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
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    IEnumerator EnragedShootRoutine()
    {
        yield return new WaitForSeconds(shootDelay / 2);
        if (firePoint != null && bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            yield return new WaitForSeconds(0.2f);
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
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = (originalGravity > 0) ? originalGravity : 3f;
        }

        foreach (Collider2D col in bossColliders)
            if (col != null) col.enabled = true;

        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
                if (trap != null) trap.DeactivateTrap();
        }

        Debug.Log("BOSS BỊ TIÊU DIỆT TẬN GỐC!");
    }
}
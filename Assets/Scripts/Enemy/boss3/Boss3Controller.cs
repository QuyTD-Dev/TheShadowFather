using UnityEngine;
using System.Collections;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;
    private Rigidbody2D rb; // Thêm biến chứa Vật lý của Boss

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
    public float takeoffSpeed = 5f; // Tăng tốc độ bay lên một chút cho dứt khoát
    public float aerialAttackCooldown = 2.0f;

    public enum BossState { Idle, Attacking, Transitioning, Flying, Dead }
    public BossState currentState = BossState.Idle;

    private float stateTimer = 2.0f;
    private float roarTimer = 10f;
    private float aerialAttackTimer;

    private Coroutine currentAttackRoutine;
    private float originalGravity; // Lưu lại trọng lực gốc để dùng cho Phase 3

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // Nhận diện Rigidbody2D

        if (rb != null)
        {
            originalGravity = rb.gravityScale; // Ghi nhớ trọng lực ban đầu (thường là 1)
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        currentHp = maxHp;
        phase2Threshold = (int)(maxHp * 0.7f);
        phase3Threshold = (int)(maxHp * 0.4f);
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null) return;

        CheckPhaseTransition();

        // Bấm T để trừ máu test
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(100);
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
                AerialCombatLogic();
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
            StartCoroutine(ShootRoutine());
            aerialAttackTimer = aerialAttackCooldown;
            Debug.Log("Boss bắn tỉa từ trên không!");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == BossState.Dead || currentPhase == BossPhase.Phase2) return;

        currentHp -= damageAmount;
        Debug.Log("Boss HP: " + currentHp);

        if (currentHp <= 0) Die();
    }

    void CheckPhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1 && currentHp <= phase2Threshold)
        {
            StartPhase2Transition();
        }
    }

    void StartPhase2Transition()
    {
        Debug.Log("<color=magenta>PHASE 2 BẮT ĐẦU: XÓA TRỌNG LỰC & CẤT CÁNH!</color>");
        currentPhase = BossPhase.Phase2;

        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        // ĐÂY LÀ ĐIỂM MẤU CHỐT: Can thiệp Vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Xóa bỏ mọi lực rớt hiện tại
            rb.gravityScale = 0f; // Boss chính thức mất trọng lượng

            // Tạm thời tắt va chạm với Player để bay xuyên qua người Player không bị vướng
            // (Nếu bạn có layer riêng cho Boss và Player, nên dùng Physics2D.IgnoreLayerCollision)
        }

        anim.SetTrigger("TakeOff");
        anim.SetBool("IsFlying", true);

        ChangeState(BossState.Transitioning, 0f);
        StartCoroutine(FlyUpToStationRoutine());
    }

    IEnumerator FlyUpToStationRoutine()
    {
        if (aerialFlyPoint == null)
        {
            Debug.LogError("Chưa gán điểm FlyPoint_Phase2!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f); // Đợi gập người cất cánh

        // Ép vị trí tiến thẳng tới mỏ neo
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

        // Rớt xuống đất khi chết
        if (rb != null) rb.gravityScale = originalGravity;

        Debug.Log("Boss đã bị tiêu diệt!");
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;
    private Rigidbody2D rb;
    private Collider2D[] bossColliders;

    [Header("UI Chiến Thắng (THÊM MỚI)")]
    [Tooltip("Kéo Panel UI Chiến thắng vào đây")]
    public GameObject victoryPanel;

    [Header("Hệ thống Máu & Phase")]
    public int maxHp = 1000;
    public int currentHp;
    public enum BossPhase { Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;

    private int phase2Threshold;
    private int phase3Threshold;

    [Header("Cài đặt Phase 1")]
    public float meleeRange = 4.0f;
    public float attackCooldown = 3.0f;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject meleeHitbox;
    public float shootDelay = 0.5f;
    public float meleeDelay = 0.4f;
    public float meleeDuration = 0.2f;

    [Tooltip("Lượng máu Boss mất đi mỗi khi bị Player chém trúng")]
    public int damageTakenPerHit = 25;

    [Header("Cài đặt Phase 2 (Trên Không & Quái)")]
    public Transform aerialFlyPoint;
    public float takeoffSpeed = 8f;
    public float aerialAttackCooldown = 2.0f;

    public GameObject wolfPrefab;
    public GameObject plantPrefab;
    [Tooltip("Kéo 6 Transform (điểm xuất hiện) vào đây để sinh quái")]
    public Transform[] minionSpawnPoints;
    private List<GameObject> activeMinions = new List<GameObject>();
    private bool isCheckingMinions = false;

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

        // Tắt Panel Chiến thắng từ đầu game để đảm bảo nó luôn ẩn
        if (victoryPanel != null) victoryPanel.SetActive(false);

        currentHp = maxHp;
        phase2Threshold = 700;
        phase3Threshold = 400;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) TakeDamage(100);

        if (currentState == BossState.Dead) return;

        CheckPhaseTransition();

        if (player == null) return;

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
                if (currentPhase == BossPhase.Phase2)
                {
                    AerialCombatLogic();
                    CheckMinionStatus();
                }
                break;

            case BossState.Transitioning:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == BossState.Dead) return;

        if (collision.CompareTag("PlayerWeapon") || collision.gameObject.name.Contains("Sword") || collision.gameObject.name.Contains("Hitbox"))
        {
            TakeDamage(damageTakenPerHit);
        }
    }

    void SpawnPhase2Minions()
    {
        activeMinions.Clear();

        for (int i = 0; i < minionSpawnPoints.Length; i++)
        {
            if (minionSpawnPoints[i] == null) continue;

            GameObject prefabToSpawn = (i % 2 == 0) ? wolfPrefab : plantPrefab;

            if (prefabToSpawn != null)
            {
                GameObject minion = Instantiate(prefabToSpawn, minionSpawnPoints[i].position, Quaternion.identity);
                activeMinions.Add(minion);
            }
        }

        isCheckingMinions = true;
    }

    void CheckMinionStatus()
    {
        if (!isCheckingMinions) return;

        bool allDead = true;
        foreach (GameObject minion in activeMinions)
        {
            if (minion != null)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            isCheckingMinions = false;
            TakeDamageFromMinion(300);
        }
    }

    void CheckPhaseTransition()
    {
        if (currentHp <= phase3Threshold && currentPhase != BossPhase.Phase3)
        {
            StartPhase3Transition();
            return;
        }

        if (currentHp <= phase2Threshold && currentHp > phase3Threshold && currentPhase == BossPhase.Phase1)
        {
            StartPhase2Transition();
        }
    }

    void StartPhase2Transition()
    {
        currentPhase = BossPhase.Phase2;

        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (bossColliders != null)
        {
            foreach (Collider2D col in bossColliders)
                if (col != null) col.enabled = false;
        }

        if (anim != null)
        {
            anim.SetTrigger("TakeOff");
            anim.SetBool("IsFlying", true);
        }

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

        SpawnPhase2Minions();
    }

    void StartPhase3Transition()
    {
        currentPhase = BossPhase.Phase3;

        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (flyRoutine != null) StopCoroutine(flyRoutine);

        if (anim != null)
        {
            anim.ResetTrigger("FlyingAttack");
            anim.SetBool("IsFlying", false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = (originalGravity > 0) ? originalGravity : 3f;
        }

        if (bossColliders != null)
        {
            foreach (Collider2D col in bossColliders)
                if (col != null) col.enabled = true;
        }

        StartCoroutine(Phase3DelayTrapRoutine());
    }

    IEnumerator Phase3DelayTrapRoutine()
    {
        ChangeState(BossState.Transitioning, 0f);
        yield return new WaitForSeconds(1.5f);

        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
                if (trap != null) trap.ActivateTrap();
        }

        ChangeState(BossState.Idle, 1f);
    }

    void GroundCombatLogic()
    {
        roarTimer -= Time.deltaTime;
        LookAtPlayer();
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= meleeRange)
        {
            if (anim != null) anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, attackCooldown);
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
        }
        else
        {
            if (roarTimer <= 0)
            {
                if (anim != null) anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, attackCooldown + 1f);
                roarTimer = 10f;
            }
            else
            {
                if (anim != null) anim.SetTrigger("Ranged");
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
            if (anim != null) anim.SetTrigger("FlyingAttack");
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
            if (anim != null) anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, enragedAttackCooldown);
            currentAttackRoutine = StartCoroutine(MeleeAttackRoutine());
        }
        else
        {
            if (roarTimer <= 0)
            {
                if (anim != null) anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, enragedAttackCooldown + 1f);
                roarTimer = enragedRoarCooldown;
            }
            else
            {
                if (anim != null) anim.SetTrigger("Ranged");
                ChangeState(BossState.Attacking, enragedAttackCooldown);
                currentAttackRoutine = StartCoroutine(EnragedShootRoutine());
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == BossState.Dead || currentPhase == BossPhase.Phase2) return;
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

        if (currentHp <= 0) Die();
    }

    void LookAtPlayer()
    {
        if (player == null) return;
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
        if (anim != null)
        {
            anim.SetTrigger("Die");
            anim.SetBool("IsFlying", false);
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = (originalGravity > 0) ? originalGravity : 3f;
        }

        if (bossColliders != null)
        {
            foreach (Collider2D col in bossColliders)
                if (col != null) col.enabled = false;
        }

        if (fireTraps != null)
        {
            foreach (FireTrap trap in fireTraps)
                if (trap != null) trap.DeactivateTrap();
        }

        // --- GỌI GIAO DIỆN CHIẾN THẮNG ---
        StartCoroutine(ShowVictoryScreen());
    }

    // --- COROUTINE HIỂN THỊ CHIẾN THẮNG ---
    IEnumerator ShowVictoryScreen()
    {
        // Đợi 3 giây để người chơi xem Boss gục ngã
        yield return new WaitForSeconds(3f);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true); // Bật giao diện
            Time.timeScale = 0f; // Đóng băng thời gian (Tùy chọn: Để quái/cảnh vật ngừng chạy nền)
            Debug.Log("🎉 ĐÃ HIỂN THỊ MÀN HÌNH CHIẾN THẮNG!");
        }
    }
}
using UnityEngine;
using System.Collections;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;

    [Header("Hệ thống Máu & Phase")]
    public int maxHp = 1000;
    public int currentHp;
    public enum BossPhase { Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;
    // Mốc máu chuyển Phase
    private int phase2Threshold;
    private int phase3Threshold;

    [Header("Cài đặt Phase 1")]
    public float meleeRange = 4.0f;
    public float attackCooldown = 3.0f;

    [Header("Vũ khí & Sát thương")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject meleeHitbox;

    [Header("Đồng bộ Hoạt ảnh")]
    public float shootDelay = 0.5f;
    public float meleeDelay = 0.4f;
    public float meleeDuration = 0.2f;

    public enum BossState { Idle, Attacking, Transitioning, Flying, Dead }
    public BossState currentState = BossState.Idle;

    private float stateTimer = 2.0f; // Chờ 2s lúc mới chạm mặt
    private float roarTimer = 10f;

    // Biến để lưu trữ các Coroutine (giúp ngắt đòn đánh lập tức khi chuyển Phase)
    private Coroutine currentAttackRoutine;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (meleeHitbox != null) meleeHitbox.SetActive(false);

        // Thiết lập máu
        currentHp = maxHp;
        phase2Threshold = (int)(maxHp * 0.7f); // 700 HP
        phase3Threshold = (int)(maxHp * 0.4f); // 400 HP
    }

    void Update()
    {
        if (currentState == BossState.Dead || currentState == BossState.Transitioning || player == null) return;

        // Chỉ chạy AI tấn công nếu đang ở Phase 1 hoặc Phase 3 (đánh dưới đất)
        if (currentPhase == BossPhase.Phase1 || currentPhase == BossPhase.Phase3)
        {
            GroundCombatLogic();
        }
        else if (currentPhase == BossPhase.Phase2)
        {
            // Tạm thời để trống, ta sẽ làm logic bay bắn tỉa ở Bước 2
        }
    }

    // --- LOGIC CHIẾN ĐẤU MẶT ĐẤT (Phase 1 & 3) ---
    void GroundCombatLogic()
    {
        roarTimer -= Time.deltaTime;

        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
        }
        else if (currentState == BossState.Idle)
        {
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
        else if (currentState == BossState.Attacking && stateTimer <= 0)
        {
            ChangeState(BossState.Idle, 0f);
        }
    }

    // --- HÀM NHẬN SÁT THƯƠNG (Player chém vào Boss sẽ gọi hàm này) ---
    public void TakeDamage(int damageAmount)
    {
        if (currentState == BossState.Dead) return;

        // Phase 2 Boss được bảo vệ, không nhận sát thương trực tiếp từ Player
        if (currentPhase == BossPhase.Phase2) return;

        currentHp -= damageAmount;
        Debug.Log("Boss bị chém! Máu còn: " + currentHp);

        CheckPhaseTransition();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // --- KIỂM TRA ĐỔI PHASE ---
    void CheckPhaseTransition()
    {
        // Chuyển từ Phase 1 sang Phase 2
        if (currentPhase == BossPhase.Phase1 && currentHp <= phase2Threshold)
        {
            Debug.Log("<color=magenta>PHASE 2 BẮT ĐẦU: BOSS CẤT CÁNH!</color>");
            currentPhase = BossPhase.Phase2;

            // 1. Ngắt ngay lập tức đòn đánh hiện tại (nếu có)
            if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
            if (meleeHitbox != null) meleeHitbox.SetActive(false);

            // 2. Chuyển state sang đang chuyển đổi để khóa AI mặt đất
            ChangeState(BossState.Transitioning, 0f);

            // 3. Gọi hoạt ảnh cất cánh
            anim.SetTrigger("TakeOff");
            anim.SetBool("IsFlying", true);

            // Chuyển sang State Flying sau khi hoạt ảnh cất cánh xong (khoảng 1.2s)
            StartCoroutine(WaitAndChangeToFlyingState(1.2f));
        }
    }

    IEnumerator WaitAndChangeToFlyingState(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        currentState = BossState.Flying;
        Debug.Log("Boss đã lên không trung, sẵn sàng xả đạn!");
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
        Debug.Log("Boss đã bị tiêu diệt!");
    }
}
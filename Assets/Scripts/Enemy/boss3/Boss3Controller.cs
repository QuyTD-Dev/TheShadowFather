using UnityEngine;

public class Boss3Controller : MonoBehaviour
{
    private Animator anim;

    // 1. Định nghĩa các trạng thái của Boss
    public enum BossState { Idle, Attacking, TakeOff, Flying, FlyingAttack, Dead }
    public BossState currentState = BossState.Idle;

    [Header("Cài đặt AI")]
    public Transform player; // Kéo thả nhân vật người chơi vào đây trong Inspector
    public float meleeRange = 2.5f; // Khoảng cách chém trúng
    public float attackCooldown = 2.5f; // Thời gian nghỉ giữa 2 đòn đánh

    private float stateTimer = 0f;
    private int maxHp = 100;
    private int currentHp;
    private bool isPhase2 = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        currentHp = maxHp;

        // Nếu quên chưa kéo Player vào Inspector, code sẽ tự tìm object có tag "Player"
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // Nếu sếp đã "tạch" thì không làm gì nữa
        if (currentState == BossState.Dead) return;

        // Bộ đếm thời gian lùi dần cho các trạng thái
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
        }

        // 2. Vòng lặp State Machine (Bộ não chính)
        switch (currentState)
        {
            case BossState.Idle:
                // Nếu hết thời gian chờ, bắt đầu ra quyết định đánh
                if (stateTimer <= 0)
                {
                    DecideNextAction();
                }
                break;

            case BossState.Attacking:
                // Diễn xong hoạt ảnh đánh dưới đất -> Quay về chờ
                if (stateTimer <= 0)
                {
                    ChangeState(BossState.Idle, attackCooldown);
                }
                break;

            case BossState.TakeOff:
                // Diễn xong hoạt ảnh cất cánh -> Chuyển sang lơ lửng
                if (stateTimer <= 0)
                {
                    ChangeState(BossState.Flying, attackCooldown);
                }
                break;

            case BossState.Flying:
                // Đang bay lơ lửng, hết thời gian chờ -> Lao xuống đánh
                if (stateTimer <= 0)
                {
                    anim.SetTrigger("FlyingAttack");
                    ChangeState(BossState.FlyingAttack, 1.5f); // 1.5s là thời gian skill bay chém
                }
                break;

            case BossState.FlyingAttack:
                // Đánh trên không xong -> Quay về lơ lửng
                if (stateTimer <= 0)
                {
                    ChangeState(BossState.Flying, attackCooldown);
                }
                break;
        }
    }

    // 3. Hàm suy nghĩ: Boss sẽ làm gì tiếp theo?
    void DecideNextAction()
    {
        if (player == null) return;

        // Đo khoảng cách giữa Boss và Player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Chuyển Phase 2 nếu máu dưới 50% và chưa bay
        if (currentHp <= maxHp / 2 && !isPhase2)
        {
            isPhase2 = true;
            anim.SetTrigger("TakeOff");
            anim.SetBool("IsFlying", true);
            ChangeState(BossState.TakeOff, 1.2f); // Chờ 1.2s cho hoạt ảnh cất cánh diễn xong
            return;
        }

        // Nếu Player đứng quá gần -> Chém cận chiến
        if (distanceToPlayer <= meleeRange)
        {
            anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, 1.0f); // 1.0s là thời gian hoạt ảnh chém
        }
        else
        {
            // Nếu ở xa -> Tỉ lệ 70% bắn xa, 30% gầm thét
            int randomSkill = Random.Range(0, 100);
            if (randomSkill < 70)
            {
                anim.SetTrigger("Ranged");
                ChangeState(BossState.Attacking, 1.2f);
            }
            else
            {
                anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, 2.0f);
            }
        }
    }

    // Hàm tiện ích để chuyển trạng thái và set luôn thời gian chờ
    void ChangeState(BossState newState, float waitTime)
    {
        currentState = newState;
        stateTimer = waitTime;
    }

    // 4. Hàm nhận sát thương (Gọi từ vũ khí của Player)
    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ChangeState(BossState.Dead, 0f);
        anim.SetTrigger("Die");
        anim.SetBool("IsFlying", false); // Rớt xuống đất nếu đang bay

        // Disable collider chặn đường để Player đi qua
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Boss 3 Night Lord đã bị tiêu diệt!");
    }
}
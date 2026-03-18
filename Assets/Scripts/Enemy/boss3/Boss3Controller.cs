using UnityEngine;
using System.Collections;

public class Boss3Controller : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public Animator anim;
    public Transform player;

    [Header("Cài đặt Phase 1")]
    public float meleeRange = 2.5f;
    public float attackCooldown = 3.0f;

    [Header("Vũ khí & Sát thương")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject meleeHitbox;

    [Header("Đồng bộ Hoạt ảnh")]
    public float shootDelay = 0.5f;
    public float meleeDelay = 0.4f;
    public float meleeDuration = 0.2f;

    public enum BossState { Idle, Attacking, Dead }
    public BossState currentState = BossState.Idle;

    // Đã sửa: Cho Boss thời gian chờ 1 giây khi mới vào game
    private float stateTimer = 1.0f;
    private float roarTimer = 10f;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("LỖI: Không tìm thấy Player! Hãy kiểm tra lại Tag của nhân vật.");
        }

        if (meleeHitbox != null) meleeHitbox.SetActive(false);
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null) return;

        roarTimer -= Time.deltaTime;

        // BỘ NÃO FSM (Đã sửa lỗi vòng lặp)
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
        }
        else
        {
            // Tự động phân luồng theo trạng thái hiện tại
            switch (currentState)
            {
                case BossState.Idle:
                    // Rảnh rỗi -> Bắt đầu suy nghĩ và ra đòn
                    DecideNextAction();
                    break;

                case BossState.Attacking:
                    // Đánh xong (Hết thời gian cooldown) -> Quay lại trạng thái Idle để đánh tiếp
                    ChangeState(BossState.Idle, 0f);
                    break;
            }
        }
    }

    void DecideNextAction()
    {
        LookAtPlayer();

        float distance = Vector2.Distance(transform.position, player.position);

        // IN RA CONSOLE ĐỂ KIỂM TRA
        Debug.Log("Boss 3 đang đo khoảng cách: " + distance + " | Tầm đánh gần là: " + meleeRange);

        if (distance <= meleeRange)
        {
            Debug.Log("==> KẾT LUẬN: ĐÁNH GẦN (MELEE)");
            anim.SetTrigger("Melee");
            ChangeState(BossState.Attacking, attackCooldown);
            StartCoroutine(MeleeAttackRoutine());
        }
        else
        {
            if (roarTimer <= 0)
            {
                Debug.Log("==> KẾT LUẬN: GẦM THÉT (ROAR)");
                anim.SetTrigger("Roar");
                ChangeState(BossState.Attacking, attackCooldown + 1f);
                roarTimer = 10f;
            }
            else
            {
                Debug.Log("==> KẾT LUẬN: BẮN XA (RANGED)");
                anim.SetTrigger("Ranged");
                ChangeState(BossState.Attacking, attackCooldown);
                StartCoroutine(ShootRoutine());
            }
        }
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
            Debug.Log("Phóng đạn!");
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        yield return new WaitForSeconds(meleeDelay);

        if (meleeHitbox != null)
        {
            Debug.Log("Bật sát thương lưỡi hái!");
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
}
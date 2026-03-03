using UnityEngine;

public class BossMageController : MonoBehaviour
{
    [Header("Target & Movement")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    [Header("Attack Ranges")]
    public float meleeRange = 2f;
    public float skillRange = 5f;
    public float summonRange = 8f;

    [Header("Combat Settings")]
    public float attackCooldown = 3f;
    public float attackDamageDelay = 0.5f;
    public float skillDamageDelay = 0.8f;

    private float nextAttackTime = 0f;
    private Animator anim;
    private BossCombat combat;

    private void Start()
    {
        anim = GetComponent<Animator>();
        combat = GetComponent<BossCombat>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Xoay mặt chuẩn 2D: Lật Sprite sang trái/phải dựa vào vị trí Player
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1); // Nhìn sang phải
        }
        else if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Nhìn sang trái
        }

        // Đo khoảng cách chuẩn 2D (Bỏ qua trục Z)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= nextAttackTime)
        {
            DecideAction(distanceToPlayer);
        }
        else
        {
            if (distanceToPlayer > stopDistance) MoveTowardsPlayer();
            else anim.SetFloat("Speed", 0f);
        }
    }

    private void DecideAction(float distance)
    {
        anim.SetFloat("Speed", 0f);

        if (distance <= meleeRange)
        {
            // Tỉ lệ 50-50: Random.value trả về số ngẫu nhiên từ 0.0 đến 1.0
            if (Random.value > 0.4f) 
            {
                anim.SetTrigger("Attack");
                combat.PerformAttack(attackDamageDelay);
            }
            else 
            {
                anim.SetTrigger("Skill");
                combat.PerformSkill(skillDamageDelay);
            }
            nextAttackTime = Time.time + attackCooldown;
        }
        else if (distance <= skillRange)
        {
            anim.SetTrigger("Skill");
            combat.PerformSkill(skillDamageDelay);
            nextAttackTime = Time.time + attackCooldown;
        }
        else if (distance <= summonRange)
        {
            anim.SetTrigger("Summon");
            nextAttackTime = Time.time + attackCooldown;
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        anim.SetFloat("Speed", moveSpeed);
        // Di chuyển Boss chuẩn 2D
        Vector2 targetPosition = new Vector2(player.position.x, player.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
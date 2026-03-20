using UnityEngine;

public class SimpleWolfAI : MonoBehaviour
{
    // ĐÃ ẨN Ô NÀY ĐI, SÓI SẼ TỰ ĐỘNG TÌM PLAYER
    private Transform player;

    [Header("Cài đặt Hành Động")]
    public float moveSpeed = 3f;
    public float attackRangeX = 1.5f;
    public float chaseRange = 7f;
    public float attackCooldown = 1.5f;

    [Header("Cài Đặt Máu & Chết")]
    public int maxHealth = 500;
    private int currentHealth;
    private bool isDead = false;

    [Tooltip("Lượng máu Sói mất đi mỗi khi bị Player chém trúng")]
    public int damageTakenPerHit = 25;

    [Header("Chỉnh sửa Hình Ảnh")]
    public bool isFacingRightByDefault = true;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;
    private float nextAttackTime = 0f;
    private Vector3 baseScale;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;

        currentHealth = maxHealth;

        // TỰ ĐỘNG TÌM PLAYER NGAY KHI VỪA ĐƯỢC BOSS ĐẺ RA
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) return;

        Vector2 myCenter = col.bounds.center;
        Vector2 playerCenter = playerCol.bounds.center;

        float distanceToPlayer = Vector2.Distance(myCenter, playerCenter);
        float distanceX = Mathf.Abs(myCenter.x - playerCenter.x);

        Bounds myBounds = col.bounds;
        myBounds.Expand(0.1f);
        bool isTouchingPlayer = myBounds.Intersects(playerCol.bounds);

        if (distanceToPlayer <= chaseRange)
        {
            float sizeX = Mathf.Abs(baseScale.x);
            if (playerCenter.x > myCenter.x)
                transform.localScale = new Vector3(isFacingRightByDefault ? sizeX : -sizeX, baseScale.y, baseScale.z);
            else
                transform.localScale = new Vector3(isFacingRightByDefault ? -sizeX : sizeX, baseScale.y, baseScale.z);

            if (distanceX <= attackRangeX || isTouchingPlayer)
            {
                anim.SetBool("IsPatrolling", false);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (Time.time >= nextAttackTime)
                {
                    anim.SetTrigger("Attack");
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                anim.SetBool("IsPatrolling", true);
                float moveDirection = (playerCenter.x > myCenter.x) ? 1f : -1f;
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            anim.SetBool("IsPatrolling", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("PlayerWeapon") || collision.gameObject.name.Contains("Sword") || collision.gameObject.name.Contains("Hitbox"))
        {
            TakeDamage(damageTakenPerHit); // TRỪ MÁU THEO CHỈ SỐ MỚI
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("🐺 Sói bị chém! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null) col.enabled = false;

        anim.SetTrigger("Die");
        Destroy(gameObject, 1.5f);
    }
}
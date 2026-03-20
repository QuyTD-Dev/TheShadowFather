using UnityEngine;

public class SimpleWolfAI : MonoBehaviour
{
    [Header("KÉO NHÂN VẬT VÀO Ô TRỐNG BÊN DƯỚI")]
    public Transform player;

    [Header("Cài đặt Hành Động")]
    public float moveSpeed = 3f;
    public float attackRangeX = 3.5f; // Có thể để 3.5 hoặc 4.0
    public float chaseRange = 7f;
    public float attackCooldown = 1.5f;

    [Header("Cài Đặt Máu & Chết")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("Chỉnh sửa Hình Ảnh")]
    [Tooltip("Tắt dấu tick này nếu con sói vẫn bị ngược đầu/đuôi")]
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
    }

    void Update()
    {
        if (isDead || player == null) return;

        Vector2 myCenter = col != null ? col.bounds.center : (Vector2)transform.position;
        Collider2D playerCol = player.GetComponent<Collider2D>();
        Vector2 playerCenter = playerCol != null ? playerCol.bounds.center : (Vector2)player.position;

        float distanceToPlayer = Vector2.Distance(myCenter, playerCenter);
        float distanceX = Mathf.Abs(myCenter.x - playerCenter.x);

        if (distanceToPlayer <= chaseRange)
        {
            // --- Xoay mặt nhân vật ---
            float sizeX = Mathf.Abs(baseScale.x);
            if (playerCenter.x > myCenter.x)
                transform.localScale = new Vector3(isFacingRightByDefault ? sizeX : -sizeX, baseScale.y, baseScale.z);
            else
                transform.localScale = new Vector3(isFacingRightByDefault ? -sizeX : sizeX, baseScale.y, baseScale.z);

            // --- KIỂM TRA TẦM CẮN ---
            if (distanceX <= attackRangeX)
            {
                anim.SetBool("IsPatrolling", false);

                // SỬA LỖI ĐẨY: Ép phanh vật lý chuẩn
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

                // SỬA LỖI ĐẨY VÀ NHẬN SÁT THƯƠNG: Dùng Vector Vật Lý (velocity) để chạy thay vì dịch chuyển
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

    // =========================================================
    // HỆ THỐNG NHẬN SÁT THƯƠNG (CÓ TÍCH HỢP BÁO LỖI)
    // =========================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // DÒNG NÀY ĐỂ MÁY QUÉT KIỂM TRA XEM SÓI ĐÃ CHẠM VÀO CÁI GÌ
        Debug.Log("🔍 Sói vừa va chạm với: " + collision.gameObject.name + " | Tag: " + collision.tag);

        if (collision.CompareTag("PlayerWeapon") || collision.gameObject.name.Contains("Sword") || collision.gameObject.name.Contains("Hitbox"))
        {
            TakeDamage(1);
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
        Debug.Log("💀 Sói đã chết!");

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmos()
    {
        Collider2D c = GetComponent<Collider2D>();
        Vector2 center = c != null ? c.bounds.center : (Vector2)transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, attackRangeX);
    }
}
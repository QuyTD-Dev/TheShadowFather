using UnityEngine;
using System.Collections;

public class PlantAI : MonoBehaviour
{
    [Header("Cài Đặt Tấn Công")]
    public float attackRange = 5f;
    public float fireRate = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Cài Đặt Máu & Chết")]
    public int maxHealth = 3;       // Số nhát chém cần thiết để giết cây
    private int currentHealth;
    private bool isDead = false;

    [Header("Chỉnh sửa Hình Ảnh")]
    [Tooltip("Tắt dấu tick này nếu cây bị ngược hướng")]
    public bool isFacingRightByDefault = true;

    private Animator anim;
    private Transform player;
    private float nextFireTime;
    private Vector3 baseScale;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        baseScale = transform.localScale;

        // Hồi đầy máu khi bắt đầu
        currentHealth = maxHealth;
    }

    void Update()
    {
        // QUAN TRỌNG: Nếu cây đã chết thì KHÔNG làm gì nữa (không xoay mặt, không bắn)
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (Time.time > nextFireTime)
            {
                StartCoroutine(ShootSequence());
                nextFireTime = Time.time + fireRate;
            }
        }

        // Xoay mặt
        float sizeX = Mathf.Abs(baseScale.x);
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(isFacingRightByDefault ? sizeX : -sizeX, baseScale.y, baseScale.z);
        else
            transform.localScale = new Vector3(isFacingRightByDefault ? -sizeX : sizeX, baseScale.y, baseScale.z);
    }

    IEnumerator ShootSequence()
    {
        anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.3f);

        if (bulletPrefab != null && firePoint != null && !isDead)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        }
    }

    // =========================================================
    // HỆ THỐNG NHẬN SÁT THƯƠNG TỪ KIẾM CỦA PLAYER
    // =========================================================

    // CÁCH 1: Dành cho trường hợp Game của bạn dùng Tag cho Vũ khí
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // KIỂM TRA: Nếu vật chạm vào quái là Kiếm của Player
        if (collision.CompareTag("PlayerWeapon") || collision.gameObject.name.Contains("Sword") || collision.gameObject.name.Contains("Hitbox"))
        {
            TakeDamage(1); // Mất 1 máu
        }
    }

    // CÁCH 2: Dành cho trường hợp Game của bạn có dùng script DamageDealer
    // Hàm này được để dạng 'public' để các Script chém của Player có thể gọi trực tiếp
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("🌿 Cây bị chém! Máu còn lại: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines(); // Bắt buộc ngừng việc sinh ra đạn

        // 1. Tắt khung va chạm để Player có thể đi xuyên qua xác chết
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        // 2. SỬA LỖI: Tắt trọng lực chuẩn cho Unity mới để xác không bị rơi
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        // 3. Kích hoạt hiệu ứng Chết trong Animator
        anim.SetTrigger("Die");
        Debug.Log("💀 Cây đã chết!");

        // 4. Biến mất (Xóa khỏi màn hình) sau 1.5 giây để đợi Animation chết diễn xong
        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
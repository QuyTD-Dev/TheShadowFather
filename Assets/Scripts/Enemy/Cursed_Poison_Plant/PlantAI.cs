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
    public int maxHealth = 500;
    private int currentHealth;
    private bool isDead = false;

    [Tooltip("Lượng máu Cây mất đi mỗi khi bị Player chém trúng")]
    public int damageTakenPerHit = 25;

    [Header("Chỉnh sửa Hình Ảnh")]
    public bool isFacingRightByDefault = true;

    private Animator anim;
    private Transform player;
    private float nextFireTime;
    private Vector3 baseScale;

    void Start()
    {
        anim = GetComponent<Animator>();

        // TỰ ĐỘNG TÌM PLAYER
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        baseScale = transform.localScale;
        currentHealth = maxHealth;
    }

    void Update()
    {
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
        Debug.Log("🌿 Cây bị chém! Máu còn lại: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        anim.SetTrigger("Die");
        Destroy(gameObject, 1.5f);
    }
}
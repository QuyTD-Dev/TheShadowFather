using UnityEngine;

public class BossSkillObject : MonoBehaviour
{
    [Header("Cấu hình Skill")]
    public float damage = 20f;
    public float speed = 8f;        // Tốc độ bay (đặt = 0 nếu muốn nó đứng im nổ tại chỗ)
    public float lifeTime = 4f;     // Tự hủy sau bao lâu
    public bool isHoming = false;   // Có đuổi theo người chơi không?
    public GameObject hitEffect;    // Hiệu ứng nổ khi trúng (nếu có)

    private Transform target;       // Mục tiêu (Player)
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tìm Player để dí (nếu bật chế độ Homing)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        // Tự hủy sau thời gian lifeTime
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 1. Nếu là skill bay thẳng (tương tự đạn)
        if (!isHoming)
        {
            // Bay theo hướng bên phải của object (Local Right)
            transform.Translate(Vector3.right * speed * Time.fixedDeltaTime);
        }
        // 2. Nếu là skill đuổi theo người chơi (Homing)
        else if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            // Xoay đầu lâu về phía player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Kiểm tra va chạm với Player
        if (hitInfo.CompareTag("Player"))
        {
            // Gây sát thương (Giả sử Player có script PlayerHealth)
            // hitInfo.GetComponent<PlayerHealth>().TakeDamage(damage); 
            Debug.Log($"Skill trúng Player! Trừ {damage} máu.");

            // Tạo hiệu ứng nổ (nếu có)
            if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);

            // Hủy object skill sau khi trúng
            Destroy(gameObject);
        }
        // Va chạm với đất thì cũng hủy
        else if (hitInfo.CompareTag("Ground"))
        {
            if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}

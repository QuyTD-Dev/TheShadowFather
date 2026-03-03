using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Chỉ số đạn")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tìm vị trí người chơi
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Vector3 direction = player.transform.position - transform.position;

            // --- SỬA LỖI TẠI ĐÂY ---
            // Cũ: rb.velocity = ...
            // Mới: rb.linearVelocity = ...
            rb.linearVelocity = new Vector2(direction.x, direction.y).normalized * speed;

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rot + 90);
        }
        else
        {
            // --- SỬA LỖI TẠI ĐÂY ---
            rb.linearVelocity = Vector2.left * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            Debug.Log("Player trúng đạn!");
            Destroy(gameObject);
        }
        else if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
        else if (hitInfo.CompareTag("Enemy") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Không làm gì nếu trúng phe mình
        }
    }
}
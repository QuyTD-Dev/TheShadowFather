using UnityEngine;

public class Boss3Bullet : MonoBehaviour
{
    [Header("Cài đặt đạn")]
    public float speed = 10f;
    public int damage = 20;
    public float lifetime = 5f;

    private Vector2 moveDirection;

    void Start()
    {
        // Hủy đạn sau 5s để tránh nặng máy
        Destroy(gameObject, lifetime);

        // 1. Tìm mục tiêu (Player)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. TÌM TÂM CHUẨN XÁC CỦA PLAYER
            Vector3 targetPos = player.transform.position; // Lấy vị trí gốc làm dự phòng

            // Lấy Collider (BoxCollider2D, CapsuleCollider2D...) đang gắn trên Player
            Collider2D playerCollider = player.GetComponent<Collider2D>();

            if (playerCollider != null)
            {
                // ĐÂY LÀ ĐIỂM ĂN TIỀN: Lấy tọa độ điểm chính giữa của hộp va chạm
                // Dù Player to hay nhỏ, tâm này luôn nằm ở giữa người!
                targetPos = playerCollider.bounds.center;
            }

            // 3. Tính toán Vector hướng bay và xoay đạn
            moveDirection = (targetPos - transform.position).normalized;
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Dự phòng: Nếu Player chết hoặc mất tích, đạn bay ngang
            moveDirection = transform.right;
        }
    }

    void Update()
    {
        // Di chuyển đạn liên tục theo hướng đã khóa
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Gây sát thương nếu trúng Player
        if (hitInfo.CompareTag("Player"))
        {
            Health playerHealth = hitInfo.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("<color=red>Đạn của Boss 3 đã trúng giữa tâm mục tiêu!</color>");
            }
            Destroy(gameObject);
        }
        // Hủy đạn nếu đập vào mặt đất/tường
        else if (hitInfo.CompareTag("Ground") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
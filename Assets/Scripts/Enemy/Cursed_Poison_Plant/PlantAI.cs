using UnityEngine;
using System.Collections;

public class PlantAI : MonoBehaviour
{
    [Header("Cài Đặt Tấn Công")]
    public float attackRange = 5f; // Tầm bắn xa hơn quái thường
    public float fireRate = 2f;    // 2 giây bắn 1 phát
    public GameObject bulletPrefab; // Kéo Prefab viên đạn vào đây
    public Transform firePoint;     // Vị trí nòng súng (miệng cây)

    private Animator anim;
    private Transform player;
    private float nextFireTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Tính khoảng cách
        float distance = Vector2.Distance(transform.position, player.position);

        // Nếu Player trong tầm -> Bắn
        if (distance <= attackRange)
        {
            if (Time.time > nextFireTime)
            {
                StartCoroutine(ShootSequence());
                nextFireTime = Time.time + fireRate;
            }
        }
        if (player.position.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator ShootSequence()
    {
        anim.SetTrigger("Shoot");

        // Chờ đúng thời điểm miệng cây mở ra (ví dụ 0.3s)
        yield return new WaitForSeconds(0.3f);

        // Kiểm tra an toàn trước khi bắn
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Debug.Log("BẰNG! Đã sinh ra đạn!"); // <--- Dòng này để kiểm tra ở Console
        }
        else
        {
            Debug.LogError("LỖI: Chưa gắn BulletPrefab hoặc FirePoint vào cây!");
        }
    }

    // Vẽ vòng tròn tầm bắn để dễ chỉnh (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
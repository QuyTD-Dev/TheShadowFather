using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Cấu Hình Thời Gian")]
    [Tooltip("Thời gian chờ ban đầu khi mới vào game (để tạo nhịp so le)")]
    [SerializeField] private float startDelay = 0f;

    [Tooltip("Thời gian nghỉ giữa 2 lần dập (Càng nhỏ dập càng nhanh)")]
    [SerializeField] private float cooldownTime = 2.5f;

    [Header("Cấu Hình Sát Thương")]
    [SerializeField] private int damage = 1;

    private Animator anim;
    private BoxCollider2D boxCollider;

    void Start()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Mặc định tắt collider đi cho an toàn
        if (boxCollider != null) boxCollider.enabled = false;

        StartCoroutine(TrapRoutine());
    }

    IEnumerator TrapRoutine()
    {
        // 1. Chờ delay ban đầu (chỉ chạy 1 lần lúc start)
        yield return new WaitForSeconds(startDelay);

        while (true) // Vòng lặp vĩnh cửu
        {
            // 2. Kích hoạt Animation Dập xuống
            anim.SetTrigger("Smash");

            // 3. Chờ cho đến khi Animation dập xuống chạy xong
            // (Lấy độ dài của clip hiện tại để chờ, tránh việc dập liên tục khi chưa thu về)
            yield return new WaitForSeconds(0.5f); // Chờ 1 chút để animation chuyển state
            float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);

            // 4. Vào trạng thái nghỉ (Cooldown)
            // Đây là lúc bẫy đang ở trên trần và chờ lần dập tiếp theo
            yield return new WaitForSeconds(cooldownTime);
        }
    }

    // Xử lý va chạm gây sát thương
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Code trừ máu của bạn ở đây. Ví dụ:
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log("Người chơi bị đâm trúng! Mất " + damage + " máu.");
        }
    }
}
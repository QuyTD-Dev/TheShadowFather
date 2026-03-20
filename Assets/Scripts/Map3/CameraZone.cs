using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    private BoxCollider2D zoneCollider;

    void Start()
    {
        // Tự động lấy BoxCollider2D trên object này và ép nó thành Trigger
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
    }

    // Khi có một vật thể chạm vào vùng này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đó có phải là Player (Elias) không
        if (collision.CompareTag("Player"))
        {
            // Tìm Camera chính và đổi giới hạn sang Box của vùng này
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetNewBounds(zoneCollider);
            }
        }
    }
}
using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("CỔNG ĐÍCH ĐẾN")]
    [Tooltip("Kéo Object Cổng đích vào đây")]
    public Portal destinationPortal;

    [Header("ĐIỂM XUẤT HIỆN Ở CỔNG NÀY")]
    [Tooltip("Kéo Object SpawnPoint con của cổng NÀY vào đây")]
    public Transform spawnPoint;

    [Header("CAMERA ZONE (Chỉ dành cho Cổng Cuối)")]
    [Tooltip("Kéo BoxCollider2D vùng camera cảnh TIẾP THEO vào đây")]
    public BoxCollider2D nextCameraZone;

    [Header("ĐIỀU KIỆN MỞ CỔNG (QUÁI VẬT)")]
    [Tooltip("Kéo tất cả quái vật trong khu vực này vào đây. Tiêu diệt hết thì cổng mới hiện ra.")]
    public GameObject[] requiredEnemies;

    [Tooltip("Kéo Object chứa hình/hiệu ứng của cổng vào đây (nếu có). Để trống cũng được.")]
    public GameObject portalVisuals;

    private bool isTeleporting = false;
    private bool isLockedByEnemies = false;

    private Collider2D portalCollider;
    private SpriteRenderer portalSprite;

    void Start()
    {
        portalCollider = GetComponent<Collider2D>();
        portalSprite = GetComponent<SpriteRenderer>();

        // Nếu có gán quái vật vào danh sách, khóa cổng và ẩn nó đi
        if (requiredEnemies != null && requiredEnemies.Length > 0)
        {
            isLockedByEnemies = true;
            SetPortalVisibility(false);
        }
    }

    void Update()
    {
        // Nếu cổng đang bị khóa, liên tục kiểm tra xem quái chết hết chưa
        if (isLockedByEnemies)
        {
            if (CheckAllEnemiesDead())
            {
                isLockedByEnemies = false; // Mở khóa
                SetPortalVisibility(true); // Hiện cổng
                Debug.Log($"✨ [Portal] Cổng {gameObject.name} đã xuất hiện vì quái bị tiêu diệt sạch!");
            }
        }
    }

    // Hàm kiểm tra xem toàn bộ quái trong mảng đã bị Destroy chưa
    bool CheckAllEnemiesDead()
    {
        foreach (GameObject enemy in requiredEnemies)
        {
            // Khi quái chết, lệnh Destroy(gameObject) sẽ biến enemy thành null
            if (enemy != null)
            {
                return false; // Vẫn còn ít nhất 1 con quái sống
            }
        }
        return true; // Tất cả đều đã biến mất (chết)
    }

    // Hàm Tắt/Bật cổng
    void SetPortalVisibility(bool isVisible)
    {
        // 1. Tắt/bật khung va chạm để Player không thể chui vào lúc cổng ẩn
        if (portalCollider != null) portalCollider.enabled = isVisible;

        // 2. Tắt/bật hình ảnh Sprite của cổng
        if (portalSprite != null) portalSprite.enabled = isVisible;

        // 3. Tắt/bật các hiệu ứng con (nếu bạn có gán Object Visuals)
        if (portalVisuals != null) portalVisuals.SetActive(isVisible);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Khóa an toàn: Nếu cổng đang khóa hoặc đang dịch chuyển thì bỏ qua
        if (isTeleporting || isLockedByEnemies) return;

        if (destinationPortal == null)
        {
            Debug.LogError($"[Portal] Lỗi: {gameObject.name} chưa gán Destination Portal!");
            return;
        }

        StartCoroutine(TeleportProcess(collision.transform));
    }

    IEnumerator TeleportProcess(Transform playerTransform)
    {
        isTeleporting = true;

        // 1. Khóa cổng đích
        destinationPortal.LockPortal();

        // 2. Dịch chuyển Player đến điểm an toàn
        if (destinationPortal.spawnPoint != null)
        {
            playerTransform.position = destinationPortal.spawnPoint.position;
        }
        else
        {
            playerTransform.position = destinationPortal.transform.position;
            Debug.LogWarning($"[Portal] {destinationPortal.name} bị thiếu Spawn Point!");
        }

        // ========================================================
        // 3. XỬ LÝ SỬA LỖI CAMERA LEM MAP (CRITICAL FIX)
        // ========================================================
        if (nextCameraZone != null)
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetNewBounds(nextCameraZone);

                Camera cam = Camera.main;
                float camHeight = cam.orthographicSize;
                float camWidth = cam.orthographicSize * cam.aspect;
                Bounds bounds = nextCameraZone.bounds;

                float minX = bounds.min.x + camWidth;
                float maxX = bounds.max.x - camWidth;
                float minY = bounds.min.y + camHeight;
                float maxY = bounds.max.y - camHeight;

                if (minX > maxX) minX = maxX = bounds.center.x;
                if (minY > maxY) minY = maxY = bounds.center.y;

                Vector3 targetDesiredPos = playerTransform.position + camFollow.offset;

                float clampedX = Mathf.Clamp(targetDesiredPos.x, minX, maxX);
                float clampedY = Mathf.Clamp(targetDesiredPos.y, minY, maxY);

                cam.transform.position = new Vector3(clampedX, clampedY, cam.transform.position.z);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }

    public void LockPortal()
    {
        StartCoroutine(LockRoutine());
    }

    private IEnumerator LockRoutine()
    {
        isTeleporting = true;
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
}
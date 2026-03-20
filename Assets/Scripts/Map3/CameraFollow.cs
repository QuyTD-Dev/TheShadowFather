using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Đối tượng theo dõi (Elias)")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Vùng giới hạn hiện tại")]
    public BoxCollider2D mapBounds;

    private Camera cam;
    private float camHeight, camWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null || mapBounds == null) return;

        camHeight = cam.orthographicSize;
        camWidth = cam.orthographicSize * cam.aspect;

        Vector3 desiredPosition = target.position + offset;

        Bounds bounds = mapBounds.bounds;

        float minX = bounds.min.x + camWidth;
        float maxX = bounds.max.x - camWidth;
        float minY = bounds.min.y + camHeight;
        float maxY = bounds.max.y - camHeight;

        if (minX > maxX) minX = maxX = bounds.center.x;
        if (minY > maxY) minY = maxY = bounds.center.y;

        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
    }

    // --- ĐOẠN CODE MỚI THÊM VÀO ---
    // Hàm này dùng để các Camera Zone gọi và báo cho Camera biết vùng mới
    public void SetNewBounds(BoxCollider2D newBounds)
    {
        mapBounds = newBounds;
    }
}
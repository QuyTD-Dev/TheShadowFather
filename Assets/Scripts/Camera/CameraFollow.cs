using UnityEngine;

namespace TheShadowFather.Camera
{
    /// <summary>
    /// Camera follow script với smooth movement và customizable boundaries
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target; // Player transform
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f); // Offset từ player

        [Header("Smooth Settings")]
        [SerializeField] private float smoothSpeed = 0.125f; // Tốc độ smooth (càng nhỏ càng mượt)
        [SerializeField] private bool useSmoothDamp = false; // Sử dụng SmoothDamp thay vì Lerp
        [SerializeField] private float smoothTime = 0.3f; // Thời gian smooth cho SmoothDamp

        [Header("Camera Boundaries (Optional)")]
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

        [Header("Look Ahead Settings")]
        [SerializeField] private bool enableLookAhead = false;
        [SerializeField] private float lookAheadDistance = 2f; // Khoảng cách nhìn trước
        [SerializeField] private float lookAheadSpeed = 2f; // Tốc độ điều chỉnh look ahead

        // Private variables
        private Vector3 velocity = Vector3.zero; // Cho SmoothDamp
        private float currentLookAhead = 0f;

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("[CameraFollow] Target chưa được gán!");
                return;
            }

            FollowTarget();
        }

        private void FollowTarget()
        {
            // Tính toán vị trí mục tiêu
            Vector3 desiredPosition = target.position + offset;

            // Áp dụng look ahead nếu được bật
            if (enableLookAhead)
            {
                // Lấy hướng di chuyển của player (giả sử player có Rigidbody2D)
                Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    float targetLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadDistance;
                    currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
                    desiredPosition.x += currentLookAhead;
                }
            }

            // Áp dụng boundaries nếu được bật
            if (useBoundaries)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            // Di chuyển camera với smooth
            Vector3 smoothedPosition;
            if (useSmoothDamp)
            {
                smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            }
            else
            {
                smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            }

            transform.position = smoothedPosition;
        }

        /// <summary>
        /// Set target mới cho camera (có thể gọi từ code khác)
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Teleport camera đến vị trí target ngay lập tức (không smooth)
        /// </summary>
        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Set boundaries mới cho camera
        /// </summary>
        public void SetBoundaries(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBoundaries = true;
        }

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
                Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
                Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
                Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);

                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
            }

            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
        #endregion
    }
}

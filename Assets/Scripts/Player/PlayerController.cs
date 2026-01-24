using UnityEngine;
using UnityEngine.InputSystem;

namespace TheShadowFather.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 6.5f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float deceleration = 10f;

        [Header("Jump Parameters")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("Dash Parameters")]
        [SerializeField] private float dashDistance = 8f;
        [SerializeField] private float dashDuration = 0.3f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Attack Parameters")]
        [SerializeField] private float attack1Cooldown = 0.5f;
        [SerializeField] private float attack2Cooldown = 0.7f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayer;

        // Component References
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        // Movement State
        private float horizontalInput;
        private float currentSpeed;
        private bool isRunning;
        private bool isFacingRight = true;

        // Jump State
        private bool isGrounded;
        private bool wasGrounded;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool jumpRequested;

        // Dash State
        private bool isDashing;
        private float dashTimeCounter;
        private float dashCooldownCounter;
        private Vector2 dashDirection;

        // Attack State
        private bool isAttacking;
        private float attack1CooldownCounter;
        private float attack2CooldownCounter;
        private int currentAttack; // 1 or 2

        // Animator Parameter Hashes (performance optimization)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            ValidateGroundCheck();
        }

        private void Update()
        {
            HandleInput();
            UpdateGroundedState();
            UpdateCoyoteTime();
            UpdateJumpBuffer();
            UpdateDashTimers();
            UpdateAttackTimers();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                PerformDash();
            }
            else if (!isAttacking)
            {
                HandleMovement();
                HandleJump();
            }

            ApplyBetterJumpPhysics();
            UpdateAnimator();

            // Lock scale to prevent animation from changing it
            transform.localScale = Vector3.one;
        }

        private void HandleInput()
        {
            // Kiểm tra keyboard có sẵn không
            if (Keyboard.current == null) return;

            // Horizontal movement input (A/D hoặc mũi tên trái/phải)
            float moveInput = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveInput = 1f;

            horizontalInput = moveInput;

            // Run input (Left Shift + đang di chuyển)
            isRunning = Keyboard.current.leftShiftKey.isPressed && Mathf.Abs(horizontalInput) > 0.1f;

            // Jump input with buffer (Space)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
                jumpBufferCounter = jumpBufferTime;

                // Debug: Kiểm tra tại sao không jump được
                Debug.Log($"[JUMP INPUT] Space pressed! isGrounded: {isGrounded}, coyoteTime: {coyoteTimeCounter:F2}, jumpBuffer: {jumpBufferCounter:F2}");
            }

            // Dash input (L key)
            if (Keyboard.current.lKey.wasPressedThisFrame && dashCooldownCounter <= 0f && !isDashing)
            {
                StartDash();
            }

            // Attack input (J and K keys)
            if (!isAttacking && !isDashing)
            {
                // Attack 1 (J key)
                if (Keyboard.current.jKey.wasPressedThisFrame && attack1CooldownCounter <= 0f)
                {
                    StartAttack(1);
                }
                // Attack 2 (K key)
                else if (Keyboard.current.kKey.wasPressedThisFrame && attack2CooldownCounter <= 0f)
                {
                    StartAttack(2);
                }
            }
        }

        private void HandleMovement()
        {
            // Chọn tốc độ: Walk (không Shift) hoặc Run (có Shift)
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            float targetVelocity = horizontalInput * targetSpeed;

            // Smooth acceleration/deceleration
            float speedDifference = targetVelocity - currentSpeed;
            float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
            float movement = speedDifference * accelRate;

            currentSpeed += movement * Time.fixedDeltaTime;

            // Apply horizontal velocity
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

            // Handle sprite flipping
            if (currentSpeed > 0.1f && !isFacingRight)
            {
                Flip();
            }
            else if (currentSpeed < -0.1f && isFacingRight)
            {
                Flip();
            }
        }

        private void HandleJump()
        {
            // Jump execution with coyote time and jump buffer
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                Debug.Log($"[JUMP EXECUTED] Jumping! jumpBuffer: {jumpBufferCounter:F2}, coyoteTime: {coyoteTimeCounter:F2}");
                PerformJump();
                jumpBufferCounter = 0f;
            }

            // Cancel jump if button released early (variable jump height)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }

        private void PerformJump()
        {
            // Reset vertical velocity before applying jump force for consistency
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimeCounter = 0f;
        }

        private void ApplyBetterJumpPhysics()
        {
            // Enhanced gravity for better jump feel
            if (rb.linearVelocity.y < 0)
            {
                // Falling - apply stronger gravity
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && (Keyboard.current == null || !Keyboard.current.spaceKey.isPressed))
            {
                // Rising but jump button released - apply moderate gravity
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void UpdateGroundedState()
        {
            wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

            // Reset jump state when landing
            if (!wasGrounded && isGrounded)
            {
                jumpRequested = false;
            }

            // Debug: Hiển thị trạng thái chạm đất (nhấn G để xem, xóa sau khi debug xong)
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                Debug.Log($"[Ground Check] isGrounded: {isGrounded}, Position: {groundCheck.position}, Size: {groundCheckSize}, Layer: {groundLayer.value}");
            }
        }

        private void UpdateCoyoteTime()
        {
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void UpdateJumpBuffer()
        {
            if (jumpBufferCounter > 0f)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void UpdateAnimator()
        {
            // Update animator parameters
            animator.SetFloat(SpeedHash, Mathf.Abs(currentSpeed));
            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
            animator.SetBool(IsRunningHash, isRunning);
            animator.SetBool(IsDashingHash, isDashing);
        }

        private void StartDash()
        {
            // Xác định hướng dash dựa trên hướng nhân vật đang quay
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
            
            isDashing = true;
            dashTimeCounter = dashDuration;
            dashCooldownCounter = dashCooldown;

            Debug.Log($"[DASH] Started! Direction: {dashDirection}, Distance: {dashDistance}");
        }

        private void PerformDash()
        {
            // Tính toán tốc độ dash để đạt được khoảng cách mong muốn
            float dashSpeed = dashDistance / dashDuration;
            
            // Apply dash velocity (giữ nguyên vertical velocity)
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);

            // Giảm dash timer
            dashTimeCounter -= Time.fixedDeltaTime;

            // Kết thúc dash khi hết thời gian
            if (dashTimeCounter <= 0f)
            {
                isDashing = false;
                currentSpeed = 0f; // Reset speed để tránh trượt sau dash
                Debug.Log("[DASH] Finished!");
            }
        }

        private void UpdateDashTimers()
        {
            // Giảm cooldown timer
            if (dashCooldownCounter > 0f)
            {
                dashCooldownCounter -= Time.deltaTime;
            }
        }

        private void StartAttack(int attackType)
        {
            isAttacking = true;
            currentAttack = attackType;
            currentSpeed = 0f; // Dừng di chuyển khi attack

            if (attackType == 1)
            {
                animator.SetTrigger(Attack1Hash);
                attack1CooldownCounter = attack1Cooldown;
                Debug.Log("[ATTACK] Attack 1 started!");
            }
            else if (attackType == 2)
            {
                animator.SetTrigger(Attack2Hash);
                attack2CooldownCounter = attack2Cooldown;
                Debug.Log("[ATTACK] Attack 2 started!");
            }
        }

        // Được gọi từ Animation Event khi attack animation kết thúc
        public void OnAttackAnimationEnd()
        {
            isAttacking = false;
            currentAttack = 0;
            Debug.Log("[ATTACK] Animation finished!");
        }

        private void UpdateAttackTimers()
        {
            // Giảm attack cooldown timers
            if (attack1CooldownCounter > 0f)
            {
                attack1CooldownCounter -= Time.deltaTime;
            }

            if (attack2CooldownCounter > 0f)
            {
                attack2CooldownCounter -= Time.deltaTime;
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            spriteRenderer.flipX = !isFacingRight;
        }

        private void ValidateGroundCheck()
        {
            if (groundCheck == null)
            {
                Debug.LogError($"[PlayerController] Ground Check Transform is not assigned on {gameObject.name}!");
            }

            if (groundLayer == 0)
            {
                Debug.LogWarning($"[PlayerController] Ground Layer is not set on {gameObject.name}. Ground detection will not work!");
            }
        }

        // Visualize ground check in editor
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }
        }
    }
}

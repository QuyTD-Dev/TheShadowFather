using UnityEngine;
using UnityEngine.InputSystem;
namespace TheShadowFather.Player
{
    /// <summary>
    /// Định nghĩa các forms transformation của player
    /// </summary>
    public enum PlayerFormState
    {
        Human = 0,
        HalfDemon = 1,
        Demon = 2
    }
    /// <summary>
    /// Định nghĩa các loại element cho form Half_Demon
    /// </summary>
    public enum ElementType
    {
        Fire = 0,
        Frost = 1
    }
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
        [Header("Form System")]
        [SerializeField] private PlayerFormState startingForm = PlayerFormState.Human;
        [SerializeField] private ElementType startingElement = ElementType.Fire;
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
        private float attackTimeoutCounter;
        private const float ATTACK_TIMEOUT = 1f; // Safety timeout để tránh stuck
        private int currentAttack;
        // Form State
        private PlayerFormState currentForm;
        private ElementType currentElement;
        private bool isTransforming;
        // Animator Parameter Hashes (tối ưu performance)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        private static readonly int FormStateHash = Animator.StringToHash("FormState");
        private static readonly int ElementTypeHash = Animator.StringToHash("ElementType");
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Khởi tạo form state
            currentForm = startingForm;
            currentElement = startingElement;
            ValidateGroundCheck();
        }
        private void Start()
        {
            // Set form ban đầu trong animator
            UpdateFormAnimator();
        }
        private void Update()
        {
            HandleInput();
            HandleFormInput();
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
            // Khóa scale để animation không thay đổi
            transform.localScale = Vector3.one;
        }
        #region Input Handling
        private void HandleInput()
        {
            if (Keyboard.current == null) return;
            // Input di chuyển ngang
            float moveInput = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveInput = 1f;
            horizontalInput = moveInput;
            // Input chạy
            isRunning = Keyboard.current.leftShiftKey.isPressed && Mathf.Abs(horizontalInput) > 0.1f;
            // Input nhảy
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
                jumpBufferCounter = jumpBufferTime;
            }
            // Input dash
            if (Keyboard.current.lKey.wasPressedThisFrame && dashCooldownCounter <= 0f && !isDashing)
            {
                StartDash();
            }
            // Input attack
            if (!isAttacking && !isDashing)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame && attack1CooldownCounter <= 0f)
                {
                    StartAttack(1);
                }
                else if (Keyboard.current.kKey.wasPressedThisFrame && attack2CooldownCounter <= 0f)
                {
                    StartAttack(2);
                }
            }
        }
        /// <summary>
        /// Xử lý input transformation và chuyển element
        /// </summary>
        private void HandleFormInput()
        {
            if (Keyboard.current == null || isTransforming) return;
            // Phím O: Transform sang form tiếp theo
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                TransformToNextForm();
            }
            // Phím H: Đổi element (chỉ hoạt động ở form Half_Demon)
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                ToggleElement();
            }
        }
        #endregion
        #region Form System
        /// <summary>
        /// Transform player sang form tiếp theo trong chu kỳ:
        /// Human → Half_Demon (Fire) → Demon → Human
        /// </summary>
        private void TransformToNextForm()
        {
            isTransforming = true;
            
            // CRITICAL FIX: Reset tất cả action flags để tránh stuck states
            isAttacking = false;
            isDashing = false;
            currentAttack = 0;
            dashTimeCounter = 0f;
            attackTimeoutCounter = 0f;
            
            // NEW FIX: Force về Idle state để transition hoạt động
            currentSpeed = 0f;
            horizontalInput = 0f;
            isRunning = false;
            
            PlayerFormState previousForm = currentForm;
            switch (currentForm)
            {
                case PlayerFormState.Human:
                    currentForm = PlayerFormState.HalfDemon;
                    currentElement = ElementType.Fire; // Luôn bắt đầu với Fire
                    Debug.Log($"[TRANSFORM] Human → Half_Demon (Fire)");
                    break;
                case PlayerFormState.HalfDemon:
                    currentForm = PlayerFormState.Demon;
                    Debug.Log($"[TRANSFORM] Half_Demon ({currentElement}) → Demon");
                    break;
                case PlayerFormState.Demon:
                    currentForm = PlayerFormState.Human;
                    Debug.Log($"[TRANSFORM] Demon → Human");
                    break;
            }
            UpdateFormAnimator();
            
            // DEBUG: Kiểm tra Animator state hiện tại
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[DEBUG] Current Animator State Hash: {stateInfo.shortNameHash}, IsName check: Human={animator.GetCurrentAnimatorStateInfo(0).IsName("Human_States")}, Fire={animator.GetCurrentAnimatorStateInfo(0).IsName("HalfDemon_Fire_States")}, Demon={animator.GetCurrentAnimatorStateInfo(0).IsName("Demon_States")}");
            
            OnFormChanged(previousForm, currentForm);
            isTransforming = false;
        }
        /// <summary>
        /// Đổi giữa Fire và Frost elements.
        /// Chỉ hoạt động khi đang ở form Half_Demon.
        /// </summary>
        private void ToggleElement()
        {
            // Chỉ cho phép đổi element ở form Half_Demon
            if (currentForm != PlayerFormState.HalfDemon)
            {
                Debug.Log($"[ELEMENT TOGGLE] Bỏ qua - Form hiện tại là {currentForm}, không phải Half_Demon");
                return;
            }
            ElementType previousElement = currentElement;
            currentElement = (currentElement == ElementType.Fire) ? ElementType.Frost : ElementType.Fire;
            Debug.Log($"[ELEMENT TOGGLE] {previousElement} → {currentElement}");
            UpdateFormAnimator();
            OnElementChanged(previousElement, currentElement);
        }
        /// <summary>
        /// Cập nhật Animator parameters để phản ánh form và element hiện tại
        /// </summary>
        private void UpdateFormAnimator()
        {
            animator.SetInteger(FormStateHash, (int)currentForm);
            animator.SetInteger(ElementTypeHash, (int)currentElement);
            Debug.Log($"[ANIMATOR UPDATE] FormState={currentForm} ({(int)currentForm}), ElementType={currentElement} ({(int)currentElement})");
        }
        /// <summary>
        /// Được gọi khi form thay đổi. Override để thêm hành vi tùy chỉnh (VFX, SFX, v.v.)
        /// </summary>
        private void OnFormChanged(PlayerFormState from, PlayerFormState to)
        {
            // TODO: Thêm transformation VFX
            // TODO: Thêm transformation SFX
            // TODO: Thêm screen shake hoặc hiệu ứng khác
        }
        /// <summary>
        /// Được gọi khi element thay đổi. Override để thêm hành vi tùy chỉnh (VFX, SFX, v.v.)
        /// </summary>
        private void OnElementChanged(ElementType from, ElementType to)
        {
            // TODO: Thêm element switch VFX
            // TODO: Thêm element switch SFX
        }
        #endregion
        #region Movement
        private void HandleMovement()
        {
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            float targetVelocity = horizontalInput * targetSpeed;
            float speedDifference = targetVelocity - currentSpeed;
            float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
            float movement = speedDifference * accelRate;
            currentSpeed += movement * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
            if (currentSpeed > 0.1f && !isFacingRight)
            {
                Flip();
            }
            else if (currentSpeed < -0.1f && isFacingRight)
            {
                Flip();
            }
        }
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            spriteRenderer.flipX = !isFacingRight;
        }
        #endregion
        #region Jump
        private void HandleJump()
        {
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                PerformJump();
                jumpBufferCounter = 0f;
            }
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
        private void PerformJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimeCounter = 0f;
        }
        private void ApplyBetterJumpPhysics()
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && (Keyboard.current == null || !Keyboard.current.spaceKey.isPressed))
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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
        #endregion
        #region Dash
        private void StartDash()
        {
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
            isDashing = true;
            dashTimeCounter = dashDuration;
            dashCooldownCounter = dashCooldown;
        }
        private void PerformDash()
        {
            float dashSpeed = dashDistance / dashDuration;
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);
            dashTimeCounter -= Time.fixedDeltaTime;
            if (dashTimeCounter <= 0f)
            {
                isDashing = false;
                currentSpeed = 0f;
            }
        }
        private void UpdateDashTimers()
        {
            if (dashCooldownCounter > 0f)
            {
                dashCooldownCounter -= Time.deltaTime;
            }
        }
        #endregion
        #region Attack
        private void StartAttack(int attackType)
        {
            isAttacking = true;
            currentAttack = attackType;
            currentSpeed = 0f;
            attackTimeoutCounter = ATTACK_TIMEOUT; // Set safety timeout
            
            if (attackType == 1)
            {
                animator.SetTrigger(Attack1Hash);
                attack1CooldownCounter = attack1Cooldown;
            }
            else if (attackType == 2)
            {
                animator.SetTrigger(Attack2Hash);
                attack2CooldownCounter = attack2Cooldown;
            }
        }
        public void OnAttackAnimationEnd()
        {
            isAttacking = false;
            currentAttack = 0;
        }
        private void UpdateAttackTimers()
        {
            // Cooldown timers
            if (attack1CooldownCounter > 0f)
            {
                attack1CooldownCounter -= Time.deltaTime;
            }
            if (attack2CooldownCounter > 0f)
            {
                attack2CooldownCounter -= Time.deltaTime;
            }
            
            // SAFETY FIX: Timeout để tự động kết thúc attack nếu animation không gọi event
            if (isAttacking)
            {
                attackTimeoutCounter -= Time.deltaTime;
                if (attackTimeoutCounter <= 0f)
                {
                    Debug.LogWarning($"[ATTACK TIMEOUT] Force ending attack {currentAttack}!");
                    OnAttackAnimationEnd();
                }
            }
        }
        #endregion
        #region Ground Check
        private void UpdateGroundedState()
        {
            wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
            if (!wasGrounded && isGrounded)
            {
                jumpRequested = false;
            }
        }
        private void ValidateGroundCheck()
        {
            if (groundCheck == null)
            {
                Debug.LogError($"[PlayerController] Ground Check Transform chưa được gán trên {gameObject.name}!");
            }
            if (groundLayer == 0)
            {
                Debug.LogWarning($"[PlayerController] Ground Layer chưa được set trên {gameObject.name}!");
            }
        }
        #endregion
        #region Animator
        private void UpdateAnimator()
        {
            animator.SetFloat(SpeedHash, Mathf.Abs(currentSpeed));
            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
            animator.SetBool(IsRunningHash, isRunning);
            animator.SetBool(IsDashingHash, isDashing);
            
            // DEBUG: Phát hiện state conflicts
            if (isAttacking && isDashing)
            {
                Debug.LogError("[STATE CONFLICT] isAttacking && isDashing both true! Resetting...");
                isDashing = false;
            }
            
            // Form state được cập nhật riêng qua UpdateFormAnimator()
        }
        #endregion
        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            }
        }
        #endregion
    }
}
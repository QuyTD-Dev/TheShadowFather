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
        [SerializeField] private float airJumpForce = 5f;   // Lực nhảy lần 2 trên không (nhỏ hơn)
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        [SerializeField] private int maxAirJumps = 1;        // Số lần nhảy phép trên không (1 = double jump)
        [Header("Dash Parameters")]
        [SerializeField] private float dashDistance = 8f;
        [SerializeField] private float dashDuration = 0.3f;
        [SerializeField] private float dashCooldown = 1f;
        [Header("Attack Parameters")]
        [SerializeField] private float attack1Cooldown = 0.5f;
        [SerializeField] private float attack2Cooldown = 0.7f;
        [Header("Until Skill")]
        [SerializeField] private GameObject untilProjectilePrefab;      // Prefab lưỡi kiếm (Human)
        [SerializeField] private GameObject fireUntilProjectilePrefab;  // Prefab rồng lửa (Fire form)
        [SerializeField] private GameObject frostUntilProjectilePrefab; // Prefab lốc xoáy băng (Frost form)
        [SerializeField] private GameObject demonUntilEffectPrefab;     // Prefab hiệu ứng AoE (Demon form)
        [SerializeField] private Transform firePoint;                   // Empty GameObject ở tay/vai player
        [SerializeField] private float untilCooldown = 1.5f;
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;          // (tùy chọn) gán thêm offset thủ công
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayer;          // Chọn layer của mặt đất
        // Collider chính của player — dùng để tính vị trí chân tự động
        private Collider2D playerCollider;
        // Buffer nội bộ — dùng khi groundLayer chưa được set
        private readonly Collider2D[] _groundResults = new Collider2D[8];
        private readonly ContactFilter2D _groundFilter = new ContactFilter2D { useTriggers = false, useLayerMask = false };
        [Header("Form System")]
        [SerializeField] private PlayerFormState startingForm = PlayerFormState.Human;
        [SerializeField] private ElementType startingElement = ElementType.Fire;
        [Header("Audio SFX")]
        [SerializeField] private AudioClip humanSlashSound;
        [SerializeField] private AudioClip fireSlashSound;
        [SerializeField] private AudioClip frostSlashSound;
        [SerializeField] private AudioClip demonSlashSound;
        [Header("Movement SFX")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip dashSound;
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
        private bool canJump = true;        // Mở khóa khi đang đứng trên đất
        private float groundCheckCooldown;  // Thời gian tắt ground check sau khi nhảy
        private int airJumpsRemaining;      // Số lần nhảy trên không còn lại
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
        // Until Skill State
        private bool isUsingUntil;
        private float untilCooldownCounter;
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
        private static readonly int UntilHash = Animator.StringToHash("Until");
        private Vector3 initialScale; // Lưu kích thước gốc từ Inspector

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Lấy collider chính của player (BoxCollider2D hoặc CapsuleCollider2D ở root)
            playerCollider = GetComponent<Collider2D>();
            // Khởi tạo form state
            currentForm = startingForm;
            currentElement = startingElement;
            InitGroundCheck();
            initialScale = transform.localScale; // Lưu lại kích thước bạn chỉnh trong Inspector
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
            HandleJump();       // ✅ Đặt ở đây để đọc input cùng frame với wasPressedThisFrame
            UpdateDashTimers();
            UpdateAttackTimers();
            UpdateUntilTimers();
        }
        private void FixedUpdate()
        {
            if (isDashing)
            {
                PerformDash();
            }
            else if (!isAttacking && !isUsingUntil)
            {
                HandleMovement();
                // HandleJump() đã được chuyển sang Update()
            }
            ApplyBetterJumpPhysics();
            UpdateAnimator();
            // Khóa scale để animation không thay đổi
            transform.localScale = initialScale; // Ép về kích thước ban đầu thay vì Vector3.one
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
            // Input nhảy — cho phép buffer khi:
            //   a) Đang trên đất / coyote window (nhảy thường)
            //   b) Đang trên không và còn lượt air jump (double jump nhỏ)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (isGrounded || coyoteTimeCounter > 0f)
                {
                    jumpRequested = true;
                    jumpBufferCounter = jumpBufferTime;
                }
                else if (airJumpsRemaining > 0)
                {
                    // Air jump: thực hiện ngay, không cần buffer
                    PerformAirJump();
                }
                // else: hoàn toàn bỏ qua
            }
            // Input dash
            if (Keyboard.current.lKey.wasPressedThisFrame && dashCooldownCounter <= 0f && !isDashing)
            {
                StartDash();
            }
            // Input attack
            if (!isAttacking && !isDashing && !isUsingUntil)
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
            // Input Until Skill (phím U) — Human, Fire, Frost, Demon
            if (Keyboard.current.uKey.wasPressedThisFrame
                && !isUsingUntil && !isAttacking && !isDashing
                && untilCooldownCounter <= 0f
                && (currentForm == PlayerFormState.Human
                    || currentForm == PlayerFormState.HalfDemon
                    || currentForm == PlayerFormState.Demon))
            {
                StartUntilSkill();
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
            // Nhảy thường từ mặt đất (có coyote time)
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && canJump)
            {
                PerformGroundJump();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
                canJump = false;
            }
            // Nhả Space khi đang bay lên → cắt ngắn độ cao (variable jump)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
        /// <summary>Nhảy thường từ mặt đất.</summary>
        private void PerformGroundJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimeCounter = 0f;
            groundCheckCooldown = 0.15f;
            canJump = false;
            airJumpsRemaining = maxAirJumps; // Reset số lượt air jump
            // --- PHÁT ÂM THANH NHẢY ---
            if (jumpSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(jumpSound);
            }
        }
        /// <summary>
        /// Double jump — nhảy lần 2 trên không.
        /// Reset Y velocity trước để cảm giác nhảy sắc nét, không phụ thuộc vận tốc hiện tại.
        /// </summary>
        private void PerformAirJump()
        {
            // Reset Y velocity → cảm giác double jump sắc nét như nhảy từ đất
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * airJumpForce, ForceMode2D.Impulse);
            // Đặt lại coyote để variable jump (nhả Space sớm = nhảy thấp hơn) vẫn work
            coyoteTimeCounter = 0f;
            // Ngăn ground check bắt lỗi ngay sau khi double jump
            groundCheckCooldown = 0.1f;
            airJumpsRemaining--;
            // --- PHÁT ÂM THANH NHẢY (LẦN 2) ---
            if (jumpSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(jumpSound);
            }
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
            // --- PHÁT ÂM THANH LƯỚT ---
            if (dashSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(dashSound);
            }
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
            PlaySlashSound();
        }
        // Hàm xử lý logic chọn âm thanh
        private void PlaySlashSound()
        {
            AudioClip clipToPlay = null;

            switch (currentForm)
            {
                case PlayerFormState.Human:
                    clipToPlay = humanSlashSound;
                    break;
                case PlayerFormState.HalfDemon:
                    if (currentElement == ElementType.Fire)
                        clipToPlay = fireSlashSound;
                    else if (currentElement == ElementType.Frost)
                        clipToPlay = frostSlashSound;
                    break;
                case PlayerFormState.Demon:
                    clipToPlay = demonSlashSound;
                    break;
            }

            if (clipToPlay != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clipToPlay);
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
        #region Until Skill
        private void StartUntilSkill()
        {
            isUsingUntil = true;
            currentSpeed = 0f;
            attackTimeoutCounter = ATTACK_TIMEOUT;
            animator.SetTrigger(UntilHash);
            untilCooldownCounter = untilCooldown;
        }
        /// <summary>
        /// Animation Event — Gọi tại frame 7 của clip Until (mọi form).
        /// Spawn projectile phù hợp với form hiện tại.
        /// </summary>
        public void OnUntilFireProjectile()
        {
            if (firePoint == null)
            {
                Debug.LogWarning("[UNTIL] Chưa gắn firePoint!");
                return;
            }

            float direction = isFacingRight ? 1f : -1f;

            if (currentForm == PlayerFormState.Demon)
            {
                // --- DEMON FORM: Hiệu ứng AoE toàn màn hình ---
                if (demonUntilEffectPrefab == null)
                {
                    Debug.LogWarning("[UNTIL] Chưa gắn demonUntilEffectPrefab!");
                    return;
                }
                Instantiate(demonUntilEffectPrefab, transform.position, Quaternion.identity);
            }
            else if (currentForm == PlayerFormState.HalfDemon && currentElement == ElementType.Fire)
            {
                // --- FIRE FORM: Spawn rồng lửa ---
                if (fireUntilProjectilePrefab == null)
                {
                    Debug.LogWarning("[UNTIL] Chưa gắn fireUntilProjectilePrefab!");
                    return;
                }
                GameObject proj = Instantiate(fireUntilProjectilePrefab, firePoint.position, Quaternion.identity);
                FireDragonProjectile dragon = proj.GetComponent<FireDragonProjectile>();
                if (dragon != null)
                    dragon.Launch(direction);
            }
            else if (currentForm == PlayerFormState.HalfDemon && currentElement == ElementType.Frost)
            {
                // --- FROST FORM: Spawn lốc xoáy băng ---
                if (frostUntilProjectilePrefab == null)
                {
                    Debug.LogWarning("[UNTIL] Chưa gắn frostUntilProjectilePrefab!");
                    return;
                }
                GameObject proj = Instantiate(frostUntilProjectilePrefab, firePoint.position, Quaternion.identity);
                FrostTornadoProjectile tornado = proj.GetComponent<FrostTornadoProjectile>();
                if (tornado != null)
                    tornado.Launch(direction);
            }
            else
            {
                // --- HUMAN FORM: Spawn lưỡi kiếm ---
                if (untilProjectilePrefab == null)
                {
                    Debug.LogWarning("[UNTIL] Chưa gắn untilProjectilePrefab!");
                    return;
                }
                GameObject proj = Instantiate(untilProjectilePrefab, firePoint.position, Quaternion.identity);
                UntilProjectile bullet = proj.GetComponent<UntilProjectile>();
                if (bullet != null)
                    bullet.Launch(direction);
            }
        }
        /// <summary>
        /// Animation Event — Gọi tại frame cuối cùng của clip Human_Until.
        /// </summary>
        public void OnUntilAnimationEnd()
        {
            isUsingUntil = false;
        }
        private void UpdateUntilTimers()
        {
            if (untilCooldownCounter > 0f)
            {
                untilCooldownCounter -= Time.deltaTime;
            }
            // Safety timeout cho Until (dùng chung attackTimeoutCounter)
            if (isUsingUntil)
            {
                attackTimeoutCounter -= Time.deltaTime;
                if (attackTimeoutCounter <= 0f)
                {
                    Debug.LogWarning("[UNTIL TIMEOUT] Force ending Until skill!");
                    OnUntilAnimationEnd();
                }
            }
        }
        #endregion
        #region Ground Check
        private void InitGroundCheck()
        {
            // Nếu đã được gán trong Inspector thì dùng luôn
            if (groundCheck != null) return;

            // Tự tìm child 'Ground' nếu chưa gán
            Transform groundChild = transform.Find("Ground");
            if (groundChild != null)
            {
                groundCheck = groundChild;
                Debug.Log($"[PlayerController] Tự tìm thấy groundCheck từ child '{groundChild.name}'");
            }
            else
            {
                Debug.LogError($"[PlayerController] Chưa gán groundCheck và không tìm thấy child 'Ground' trên {gameObject.name}!");
            }

            if (groundLayer == 0)
                Debug.LogWarning($"[PlayerController] groundLayer chưa được set — sẽ dùng fallback.");
        }

        /// <summary>
        /// Tính vị trí ground check chính xác dưới chân player.
        /// Ưu tiên dùng bounds của playerCollider; fallback về groundCheck.position.
        /// </summary>
        private Vector2 GetGroundCheckPosition()
        {
            if (playerCollider != null)
            {
                Bounds b = playerCollider.bounds;
                return new Vector2(b.center.x, b.min.y - 0.02f);
            }
            if (groundCheck != null) return groundCheck.position;
            return (Vector2)transform.position + Vector2.down;
        }

        private void UpdateGroundedState()
        {
            wasGrounded = isGrounded;

            // Giảm cooldown sau khi nhảy
            if (groundCheckCooldown > 0f)
            {
                groundCheckCooldown -= Time.deltaTime;
                isGrounded = false;
                return;
            }

            Vector2 checkPos = GetGroundCheckPosition();

            if (groundLayer != 0)
            {
                isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);
            }
            else
            {
                int count = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, _groundFilter, _groundResults);
                isGrounded = false;
                for (int i = 0; i < count; i++)
                {
                    if (_groundResults[i] != null &&
                        _groundResults[i].transform.root != transform.root)
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }

            // canJump reset mỗi frame đứng trên đất (an toàn)
            if (isGrounded)
            {
                canJump = true;
            }

            // airJumpsRemaining và jumpRequested chỉ reset khi HẠ CÁNH THẬT SỰ
            if (!wasGrounded && isGrounded)
            {
                airJumpsRemaining = maxAirJumps;
                jumpRequested = false;
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
            // Vẽ hộp ground check tự động tại vị trí chân player
            Vector2 pos;
            if (playerCollider != null)
            {
                Bounds b = playerCollider.bounds;
                pos = new Vector2(b.center.x, b.min.y - 0.02f);
            }
            else if (groundCheck != null)
            {
                pos = groundCheck.position;
            }
            else
            {
                pos = (Vector2)transform.position + Vector2.down;
            }
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(pos, groundCheckSize);
        }
        #endregion
    }
}
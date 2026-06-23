using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 8f;
    public float moveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float airControl = 0.6f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Jump & Dive Settings")]
    public float jumpForce = 10f;
    [SerializeField] private float diveForwardForce = 12f;
    [SerializeField] private float diveUpwardForce = 4f;
    [SerializeField] private float diveCooldown = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckOffset; // If null, we'll use bottom of capsule
    [SerializeField] private float groundCheckRadius = 0.45f;
    [SerializeField] private LayerMask groundLayer = ~0; // Default to all layers

    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial normalPhysicMaterial;
    [SerializeField] private PhysicsMaterial tumbledPhysicMaterial;

    [Header("Visual Effects")]
    [SerializeField] private Transform _visualHolder;
    public Transform visualHolder
    {
        get => _visualHolder;
        set => _visualHolder = value;
    }
    [SerializeField] private Animator animator;
    [SerializeField] private float tiltAngleMax = 15f;
    [SerializeField] private float tiltSpeed = 10f;
    [SerializeField] private float squashStretchSpeed = 12f;

    [Header("Death / Hazard")]
    [Tooltip("Prefab with explosion animation to spawn on player death.")]
    [SerializeField] private GameObject deathExplosionPrefab;
    [Tooltip("Objects with this tag will kill the player on touch.")]
    [SerializeField] private string hazardTag = "Hazard";
    [Tooltip("If the player falls below this Y position, they die.")]
    [SerializeField] private float deathYThreshold = -20f;
    [Tooltip("How long the player stays stunned before scene restarts.")]
    [SerializeField] private float deathStunDuration = 0.8f;

    [Header("Win / Victory")]
    [Tooltip("Objects with this tag trigger victory when touched.")]
    [SerializeField] private string winTag = "Win";
    [Tooltip("TV object in the scene that will explode on victory.")]
    [SerializeField] private GameObject tvObject;
    [Tooltip("Prefab with explosion animation to spawn on the TV.")]
    [SerializeField] private GameObject tvExplosionPrefab;
    [Tooltip("Local position offset for the TV explosion (relative to the TV).")]
    [SerializeField] private Vector3 tvExplosionOffset = Vector3.zero;
    [Tooltip("How long the victory stun lasts before quitting.")]
    [SerializeField] private float winStunDuration = 1.5f;

    // Actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction diveAction;

    // Component References
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Camera mainCamera;

    [SerializeField] private AudioSource explodeSound;

    // State Variables
    public bool isGrounded;
    public bool isTumbled { get; private set; }
    public bool isDead { get; private set; }
    public bool isWin { get; private set; }
    private float tumbleTimer;
    private float nextDiveTime;
    private bool diveRequested;
    private bool jumpRequested;
    private Vector2 moveInput;
    [System.NonSerialized] public Vector2 moveInputMultiplier = Vector2.one; // used by effects (e.g. invert controls)

    // Animation / Wobble
    private Vector3 targetScale = Vector3.one;
    private float turnAmount;
    private float currentWobbleTime;
    [System.NonSerialized] public Vector3 baseScaleMultiplier = Vector3.one; // used by effects (Wider, Longer)

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main;

        // Ensure Rigidbody settings
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.mass = 80f;
        FreezeRotation();
    }

    private void Start()
    {
        // Bind Actions from global input system
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        // Try Sprint first, then Attack for Dive
        diveAction = InputSystem.actions.FindAction("Player/Sprint") ?? InputSystem.actions.FindAction("Player/Attack");

        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (diveAction != null) diveAction.Enable();
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (diveAction != null) diveAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (diveAction != null) diveAction.Disable();
    }

    private void Update()
    {
        // Read input if not tumbled, not dead, and not winning
        if (!isTumbled && !isDead && !isWin)
        {
            if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();
            else moveInput = Vector2.zero;

            if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
            {
                jumpRequested = true;
            }

            if (diveAction != null && diveAction.WasPressedThisFrame() && Time.time >= nextDiveTime)
            {
                diveRequested = true;
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }

        if (!isDead && !isWin)
            HandleVisuals();
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        // Death from falling below threshold
        if (!isDead && !isWin && transform.position.y < deathYThreshold)
        {
            Die();
            return;
        }

        if (isDead || isWin) return;

        if (isTumbled)
        {
            HandleTumbledState();
        }
        else
        {
            HandleMovement();
            HandleJumpAndDive();
        }

        // Apply extra custom gravity for heavy falling (fall-guys feel)
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * 1.5f, ForceMode.Acceleration);
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        // Position at bottom of the capsule
        Vector3 checkPos;
        if (groundCheckOffset != null)
        {
            checkPos = groundCheckOffset.position;
        }
        else
        {
            // Fallback capsule bottom
            checkPos = transform.position + Vector3.down * (capsuleCollider.height * 0.5f - capsuleCollider.radius);
        }

        // SphereCast / CheckSphere
        isGrounded = Physics.CheckSphere(checkPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        // Landing squash effect!
        if (isGrounded && !wasGrounded && rb.linearVelocity.y < -2f)
        {
            ApplySquash(0.65f); // squash down on hard landing
        }
    }

    private void HandleMovement()
    {
        // Calculate camera-relative direction
        Vector3 camForward = mainCamera != null ? mainCamera.transform.forward : Vector3.forward;
        Vector3 camRight = mainCamera != null ? mainCamera.transform.right : Vector3.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * (moveInput.y * moveInputMultiplier.y) + camRight * (moveInput.x * moveInputMultiplier.x)).normalized;

        // Target Velocity
        Vector3 targetVelocity = moveDir * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Preserve vertical speed

        // Current horizontal velocity
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 targetHorizontalVelocity = new Vector3(targetVelocity.x, 0, targetVelocity.z);

        // Decide accel rate based on grounded/air
        float accelRate = isGrounded ? acceleration : acceleration * airControl;
        float decelRate = isGrounded ? deceleration : deceleration * airControl;

        Vector3 force;
        if (targetHorizontalVelocity.magnitude > 0.01f)
        {
            Vector3 diff = targetHorizontalVelocity - horizontalVelocity;
            force = diff * accelRate;
        }
        else
        {
            force = -horizontalVelocity * decelRate;
        }

        // Apply force
        force.y = 0;
        rb.AddForce(force, ForceMode.Acceleration);

        // Rotation
        if (moveDir.magnitude > 0.05f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            
            // Compute turning amount for sideways wobble tilt
            float angleDiff = Vector3.SignedAngle(transform.forward, moveDir, Vector3.up);
            turnAmount = Mathf.Lerp(turnAmount, angleDiff / 90f, Time.fixedDeltaTime * 10f);
        }
        else
        {
            turnAmount = Mathf.Lerp(turnAmount, 0f, Time.fixedDeltaTime * 10f);
        }
    }

    public void ForceJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Clear vertical velocity first
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        ApplyStretch(1.35f); // Stretch up on jump
    }

    private void HandleJumpAndDive()
    {
        if (jumpRequested)
        {
            jumpRequested = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Clear vertical velocity first
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            ApplyStretch(1.35f); // Stretch up on jump
        }

        if (diveRequested)
        {
            diveRequested = false;
            nextDiveTime = Time.time + diveCooldown;
            
            // Dive forward relative to player orientation
            Vector3 diveDir = transform.forward;
            rb.linearVelocity = Vector3.zero; // Clear velocity for snappy dive feel
            
            rb.AddForce(diveDir * diveForwardForce + Vector3.up * diveUpwardForce, ForceMode.VelocityChange);
            ApplyStretch(1.4f);

            // Trigger tumble
            TriggerTumble();
        }
    }

    public void TriggerTumble()
    {
        isTumbled = true;
        tumbleTimer = 0f;

        // Unfreeze rotation to let physics roll the bean
        UnfreezeRotation();

        // Apply a torque/push to make tumble dynamic and funny
        Vector3 torque = transform.right * -400f + transform.up * Random.Range(-150f, 150f);
        rb.AddTorque(torque, ForceMode.Impulse);

        if (capsuleCollider != null && tumbledPhysicMaterial != null)
            capsuleCollider.sharedMaterial = tumbledPhysicMaterial;
    }

    private void HandleTumbledState()
    {
        tumbleTimer += Time.fixedDeltaTime;

        // Tumble recovery starts after some time, if we are grounded and moving slowly
        if (tumbleTimer >= 0.8f && isGrounded && rb.linearVelocity.magnitude < 2f)
        {
            RecoverFromTumble();
        }
        else if (tumbleTimer >= 2.5f) // Forced recovery if stuck
        {
            RecoverFromTumble();
        }
    }

    private void RecoverFromTumble()
    {
        StartCoroutine(RecoveryRoutine());
    }

    private System.Collections.IEnumerator RecoveryRoutine()
    {
        float recoveryDuration = 0.35f;
        float elapsed = 0f;

        Quaternion startRot = transform.rotation;
        
        // Keep the horizontal forward direction but stand upright
        Vector3 projForward = transform.forward;
        projForward.y = 0;
        if (projForward.magnitude < 0.05f) projForward = transform.up; // fallback
        projForward.Normalize();
        Quaternion endRot = Quaternion.LookRotation(projForward, Vector3.up);

        while (elapsed < recoveryDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoveryDuration;
            // Smoothly lerp rotation back to upright
            rb.MoveRotation(Quaternion.Slerp(startRot, endRot, t));
            yield return null;
        }

        // Re-freeze rotation
        FreezeRotation();
        
        isTumbled = false;
        if (capsuleCollider != null && normalPhysicMaterial != null)
            capsuleCollider.sharedMaterial = normalPhysicMaterial;
    }

    private void FreezeRotation()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void UnfreezeRotation()
    {
        rb.constraints = RigidbodyConstraints.None;
    }

    private void HandleVisuals()
    {
        float speedPercent = 0f;

        if (visualHolder != null)
        {
            speedPercent = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude / moveSpeed;

            // 1. Squash & Stretch Lerping back to Vector3.one
            // baseScaleMultiplier is applied on top so effects (Wider, Longer) don't get overwritten
            Vector3 finalTargetScale = new Vector3(
                targetScale.x * baseScaleMultiplier.x,
                targetScale.y * baseScaleMultiplier.y,
                targetScale.z * baseScaleMultiplier.z
            );
            visualHolder.localScale = Vector3.Lerp(visualHolder.localScale, finalTargetScale, Time.deltaTime * squashStretchSpeed);
            targetScale = Vector3.Lerp(targetScale, Vector3.one, Time.deltaTime * squashStretchSpeed);

            // 2. Wobble & Tilt based on movement and tumbling
            if (!isTumbled)
            {
                // Wobble animation over time (cute little waddle)
                if (speedPercent > 0.1f && isGrounded)
                {
                    currentWobbleTime += Time.deltaTime * 15f;
                    // Side-to-side waddle wobble
                    float wobbleZ = Mathf.Sin(currentWobbleTime) * 8f * speedPercent;
                    // Forward tilt when running
                    float wobbleX = tiltAngleMax * speedPercent;

                    Quaternion targetVisualRot = Quaternion.Euler(wobbleX, 0, wobbleZ - turnAmount * tiltAngleMax);
                    visualHolder.localRotation = Quaternion.Lerp(visualHolder.localRotation, targetVisualRot, Time.deltaTime * tiltSpeed);
                }
                else
                {
                    // Idle breathe/wobble
                    currentWobbleTime += Time.deltaTime * 3f;
                    float wobbleX = Mathf.Sin(currentWobbleTime) * 2f;
                    Quaternion targetVisualRot = Quaternion.Euler(wobbleX, 0, 0);
                    visualHolder.localRotation = Quaternion.Lerp(visualHolder.localRotation, targetVisualRot, Time.deltaTime * tiltSpeed);
                }
            }
            else
            {
                // Let physics handle the rotation of the root, visual child aligns to root
                visualHolder.localRotation = Quaternion.Lerp(visualHolder.localRotation, Quaternion.identity, Time.deltaTime * tiltSpeed);
            }
        }

        // 3. Animator — pause when not moving, resume when moving
        if (animator != null)
        {
            bool isMoving = speedPercent > 0.1f;
            animator.speed = isMoving ? 1f : 0f;
        }
    }

    public void ApplySquash(float squashY)
    {
        float stretchXZ = 1f + (1f - squashY) * 0.5f;
        visualHolder.localScale = new Vector3(stretchXZ, squashY, stretchXZ);
    }

    public void ApplyStretch(float stretchY)
    {
        float squashXZ = 1f - (stretchY - 1f) * 0.5f;
        visualHolder.localScale = new Vector3(squashXZ, stretchY, squashXZ);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || isWin) return;

        if (collision.gameObject.CompareTag(hazardTag))
        {
            Die();
        }
        else if (collision.gameObject.CompareTag(winTag))
        {
            Destroy(collision.gameObject);
            Win();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || isWin) return;

        if (other.CompareTag(hazardTag))
        {
            Die();
        }
        else if (other.CompareTag(winTag))
        {
            Win();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop all movement & freeze rotation
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        FreezeRotation();

        // Disable input & clear state
        moveInput = Vector2.zero;
        jumpRequested = false;
        diveRequested = false;

        // Spawn explosion on player
        if (deathExplosionPrefab != null)
        {
            Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
        }

        explodeSound.Play();

        // Hide the visual
        if (visualHolder != null)
        {
            visualHolder.gameObject.SetActive(false);
        }

        // Start death routine
        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        // Wait for explosion animation to play
        yield return new WaitForSeconds(deathStunDuration);

        // Restart the entire scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Called when the player touches an object with the Win tag.
    /// Freezes the player, blows up the TV, then quits the game.
    /// </summary>
    public void Win()
    {
        if (isDead || isWin) return;
        isWin = true;

        // Stop all movement & freeze rotation
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        FreezeRotation();

        // Disable input & clear state
        moveInput = Vector2.zero;
        jumpRequested = false;
        diveRequested = false;

        // Blow up the TV object
        if (tvObject != null && tvExplosionPrefab != null)
        {
            Vector3 explosionPos = tvObject.transform.position + tvObject.transform.TransformDirection(tvExplosionOffset);
            Instantiate(tvExplosionPrefab, explosionPos, Quaternion.identity);
            tvObject.SetActive(false);
        }

        explodeSound.Play();

        // Start win routine
        StartCoroutine(WinRoutine());
    }

    private System.Collections.IEnumerator WinRoutine()
    {
        // Wait for explosion animation to play
        yield return new WaitForSeconds(winStunDuration);

        // Quit the application (stops play in Editor)
        UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
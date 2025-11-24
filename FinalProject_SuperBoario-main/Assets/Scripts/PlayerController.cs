using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] Rigidbody2D rb;
    public Animator myAnimator;
    public CapsuleCollider2D myCapsuleCollider;
    public AudioSource audioSource;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;
    private float velocityX = 0f;
    private float previousVelocityX = 0f;
    private float speedMultiplier = 1f;

    private float flipDelayTimer = 0f;
    private float flipDelayDuration = 0.1f;
    private int lastMoveDirection = 1;
    private Vector3 originalScale;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float desiredJumpDuration = 1f;
    private float calculatedGravityScale;

    [Header("Ground Detection")]
    public float groundRayLength = 0.2f;
    public LayerMask groundLayer;
    public Vector2 groundRayOffset = new Vector2(0f, -0.5f);
    public float fastFallMultiplier = 20f;
    private bool wasGroundedLastFrame = false;

    [Header("Slope Rotation")]
    public float slopeRayLength = 1f;
    public float slopeRotationSpeed = 10f;
    public float maxSlopeTilt = 20f;

    [Header("Skid Effects")]
    public AudioClip skidClip;
    public ParticleSystem skidParticles;

    [Header("Visual Root")]
    public Transform visualRoot;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color activeColor = Color.white;

    [Header("References")]
    [SerializeField] PlayerInputHandler input;
    [SerializeField] LadderController ladderController;
    private KnockbackReceiver knockback;

    public bool canMove = true;
    private bool isClimbing = false;
    private bool justJumped = false;
    private SimpleEnemy carriedEnemy;
    public Transform carryAnchor;

    [Header("Charge Settings")]
    public float chargeSpeedMultiplier = 1.5f;
    private bool isCharging = false;

    [Header("Charge Feedback")]
    public ParticleSystem chargeParticles;
    public AudioSource chargeLoopSource;


    void Awake()
    {
        knockback = GetComponent<KnockbackReceiver>();
    }

    void Start()
    {
        UpdateTint();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCapsuleCollider = GetComponent<CapsuleCollider2D>();
        originalScale = transform.localScale;

        float timeToApex = desiredJumpDuration / 2f;
        float gravity = jumpForce / timeToApex;
        calculatedGravityScale = gravity / Mathf.Abs(Physics2D.gravity.y);
        rb.gravityScale = calculatedGravityScale;
    }

    void Update()
    {
        if (!canMove) return;

        bool isGroundedNow = IsGrounded();
        isClimbing = ladderController.IsTouchingLadder;

        rb.linearDamping = (knockback != null && knockback.IsMovementBlocked()) ? 0f : 5f;
        if (knockback != null && knockback.IsMovementBlocked()) return;

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            float climbVelocity = Mathf.Abs(ladderController.VerticalInput) > 0f
                ? ladderController.VerticalInput * ladderController.climbSpeed
                : 0f;
            rb.linearVelocity = new Vector2(0f, climbVelocity);
        }
        else
        {
            rb.gravityScale = calculatedGravityScale;
            if (!isGroundedNow && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        HandleMovement();
        HandleJump();

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.down * fastFallMultiplier * Time.deltaTime;

        if (isGroundedNow && !wasGroundedLastFrame)
            rb.position += Vector2.down * 0.001f;

        // --- Unified Pickup/Throw logic ---
        if (input.PickupOrThrowPressed)
        {
            Debug.Log("Pickup/Throw button pressed!");
            if (carriedEnemy != null)
            {
                Debug.Log("Throwing enemy now!");
                // Throw in facing direction with upward arc
                Vector2 throwDir = new Vector2(transform.localScale.x, 0.5f).normalized;
                carriedEnemy.Throw(throwDir);
                carriedEnemy = null;
            }
            else
            {
                Debug.Log("Trying to pick up enemy...");
                // Try to pick up stunned enemy
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
                foreach (var hit in hits)
                {
                    SimpleEnemy enemy = hit.GetComponent<SimpleEnemy>();
                    if (enemy != null && enemy.IsStunned && !enemy.IsCarried)
                    {
                        enemy.PickUp(carryAnchor);
                        carriedEnemy = enemy;
                        break;
                    }
                }
            }
        }
        wasGroundedLastFrame = isGroundedNow;
    }

    public void DropCarriedEnemy()
    {
        // Look for any carried enemy under the player
        foreach (Transform child in transform)
        {
            SimpleEnemy carried = child.GetComponent<SimpleEnemy>();
            if (carried != null && carried.IsCarried)
            {
                // Drop straight down with no force
                carried.isCarried = false;
                carried.transform.SetParent(null);
                carried.rb.simulated = true;
                carried.rb.linearVelocity = Vector2.zero;

                if (carried.animator) carried.animator.SetBool("Carried", false);

                Debug.Log("Dropped carried enemy due to player damage!");
            }
        }
    }

    public void UpdateTint()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;
    }

    public bool JustJumped()
    {
        bool result = justJumped;
        justJumped = false;
        return result;
    }

    public void ForceJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        SoundManager.Instance.PlayJump();
    }

    public bool CanTakeDamage()
    {
        HealthSystem hs = GetComponent<HealthSystem>();
        return hs != null && hs.personalHealth > 0f;
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
            transform.localScale = new Vector3(originalScale.x * Mathf.Sign(moveInput), originalScale.y, originalScale.z);

        bool inputChangedDirection = Mathf.Sign(moveInput) != Mathf.Sign(previousVelocityX) && moveInput != 0;
        bool grounded = IsGrounded();
        bool wasAtFullSpeed = Mathf.Abs(previousVelocityX) >= moveSpeed * 0.9f;

        // Skid trigger (works during run or charge)
        if (inputChangedDirection && wasAtFullSpeed && grounded)
        {
            if (!audioSource.isPlaying && skidClip != null)
                audioSource.PlayOneShot(skidClip);
            skidParticles?.Play();

            flipDelayTimer = flipDelayDuration;
            lastMoveDirection = (int)Mathf.Sign(moveInput);
        }

        // --- Movement branch ---
        bool wantsToCharge = input.ChargeHeld;
        bool atFullSpeed = Mathf.Abs(previousVelocityX) >= moveSpeed * 0.95f;

        if (wantsToCharge && grounded && atFullSpeed)
        {
            // Enter charge
            if (!isCharging)
            {
                isCharging = true;
                myAnimator.SetBool("isCharging", true);
                StartChargeFeedback();
            }

            // Steering-enabled charge
            float targetChargeSpeed = moveInput * moveSpeed * chargeSpeedMultiplier;
            velocityX = Mathf.MoveTowards(velocityX, targetChargeSpeed, acceleration * Time.deltaTime);

            float maxChargeSpeed = moveSpeed * chargeSpeedMultiplier;
            velocityX = Mathf.Clamp(velocityX, -maxChargeSpeed, maxChargeSpeed);
        }
        else
        {
            // Exit charge
            if (isCharging)
            {
                isCharging = false;
                myAnimator.SetBool("isCharging", false);
                StopChargeFeedback();
            }

            // Normal run
            float targetRunSpeed = moveInput * moveSpeed * speedMultiplier;
            velocityX = (moveInput != 0)
                ? Mathf.MoveTowards(velocityX, targetRunSpeed, acceleration * Time.deltaTime)
                : Mathf.MoveTowards(velocityX, 0f, deceleration * Time.deltaTime);
        }

        // Flip delay settle
        if (flipDelayTimer > 0f)
        {
            flipDelayTimer -= Time.deltaTime;
            if (flipDelayTimer <= 0f)
                transform.localScale = new Vector3(originalScale.x * lastMoveDirection, originalScale.y, originalScale.z);
        }

        // Knockback block
        if (knockback != null && knockback.IsMovementBlocked())
        {
            velocityX = 0f;
            previousVelocityX = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        previousVelocityX = velocityX;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
    }

    private void StartChargeFeedback()
    {
        if (chargeParticles != null && !chargeParticles.isPlaying)
        {
            chargeParticles.loop = true; // enforce looping
            chargeParticles.Play();
        }

        if (chargeLoopSource != null && !chargeLoopSource.isPlaying)
        {
            chargeLoopSource.loop = true; // enforce looping
            chargeLoopSource.Play();
        }
    }

    private void StopChargeFeedback()
    {
        if (chargeParticles != null && chargeParticles.isPlaying)
            chargeParticles.Stop();

        if (chargeLoopSource != null && chargeLoopSource.isPlaying)
            chargeLoopSource.Stop();
    }

    public bool IsCharging()
    {
        return isCharging;
    }

    public void ResetHorizontalVelocity()
    {
        velocityX = 0f;
        previousVelocityX = 0f;
    }

    private void HandleJump()
    {
        if (input.JumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            SoundManager.Instance.PlayJump();
            justJumped = true;
        }
    }

    private RaycastHit2D GetGroundHit()
    {
        Vector2 origin = (Vector2)transform.position + groundRayOffset;
        return Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);
    }

    public bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + groundRayOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);
        Debug.DrawRay(origin, Vector2.down * groundRayLength, Color.green);
        return hit.collider != null;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void PlayParticles(ParticleSystem ps, Transform anchor)
    {
        if (ps != null && anchor != null)
        {
            ps.transform.position = anchor.position;
            ps.transform.localScale = Vector3.one;
            ps.Play();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging)
        {
            // --- Enemy reaction ---
            SimpleEnemy enemy = collision.gameObject.GetComponent<SimpleEnemy>();
            if (enemy != null)
            {
                enemy.ApplyChargeHit(transform.position); // NEW call
            }

            // --- Breakable reaction ---
            if (collision.gameObject.CompareTag("Breakable"))
            {
                Destroy(collision.gameObject);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 origin = (Vector2)transform.position + groundRayOffset;
        Gizmos.DrawRay(origin, Vector2.down * groundRayLength);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);

        Gizmos.color = Color.red;
        Vector2 throwDir = new Vector2(Mathf.Sign(transform.localScale.x), 0.5f).normalized;
        Gizmos.DrawRay(transform.position, throwDir * 2f);
    }
}
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class SimpleEnemy : MonoBehaviour
{
    public enum DeathType { Generic, Throwable, Shootable }

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float normalSpeed = 2f;
    public float chaseSpeed = 4f;
    public float directionChangeInterval = 2f;
    public LayerMask groundLayer;
    public float groundRayLength = 1f;

    [Header("Combat")]
    public float maxHealth = 3f;
    public float damage = 1f;
    public float attackCooldown = 1f;
    public float attackRange = 1f;
    public LayerMask playerLayer;

    [Header("Respawn")]
    public float respawnDelay = 3f;

    [Header("Visuals")]
    public ParticleSystem deathEffect;
    public ParticleSystem bleedingEffect;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public Animator animator;

    [Header("Targeting")]
    public float detectionRange = 5f;
    private Transform playerTarget;

    [Header("Stun & Knockback")]
    public float knockbackForce = 10f;
    public float knockbackForceX = 10f;
    public float knockbackForceY = 8f;
    public float stunDuration = 1f;
    private bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Ladder Avoidance")]
    public float ladderAvoidRadius = 1.5f;
    public LayerMask ladderLayer;
    private bool avoidingLadder = false;
    private Transform nearestLadder;

    [Header("Carry & Throw")]
    public bool isCarried = false;
    public Transform carryAnchor;
    public float throwForceX = 12f;
    public float throwForceY = 8f;

    public bool IsStunned => isStunned;
    public bool IsCarried => isCarried;

    private float currentHealth;
    private Vector2 moveDirection;
    private Vector3 spawnPosition;
    public Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private AudioSource audioSource;
    private bool isAlive = true;
    private bool isBleeding = false;
    private float lastTouchTime = -Mathf.Infinity;
    public float touchDamageCooldown = 1f;
    public float horizontalDeadZone = 0.5f;
    private bool isThrown = false;

    void Start()
    {
        moveSpeed = normalSpeed;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentHealth = maxHealth;
        spawnPosition = transform.position;

        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        if (isThrown)
        {
            if (animator) animator.SetFloat("Speed", 0f);
            return;
        }

        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
                isStunned = false;

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator) animator.SetFloat("Speed", 0f);
            return;
        }

        Vector2 moveVec;

        if (playerTarget != null)
        {
            float xOffset = playerTarget.position.x - transform.position.x;
            float absOffset = Mathf.Abs(xOffset);
            float direction = absOffset > horizontalDeadZone ? Mathf.Sign(xOffset) : 0f;
            moveVec = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            Vector2 slopeDirection = GetSlopeDirection();
            moveVec = new Vector2(slopeDirection.x * moveSpeed, rb.linearVelocity.y);
        }

        rb.linearVelocity = moveVec;
        if (animator) animator.SetFloat("Speed", Mathf.Abs(moveVec.x));
    }

    void Update()
    {
        if (!isAlive) return;

        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (player != null)
        {
            HealthSystem hs = player.GetComponent<HealthSystem>();
            if (hs != null && hs.IsInvincible())
            {
                // Player is invincible & ignore them
                playerTarget = null;
                moveSpeed = normalSpeed;
                if (animator) animator.SetFloat("Speed", 0f);
                return;
            }

            // Normal chase
            playerTarget = player.transform;
            moveSpeed = chaseSpeed;
        }
        else
        {
            playerTarget = null;
            moveSpeed = normalSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isThrown)
        {
            SimpleEnemy otherEnemy = collision.gameObject.GetComponent<SimpleEnemy>();
            if (otherEnemy != null && otherEnemy != this)
            {
                Vector2 bounceDir = collision.contacts[0].normal;

                rb.linearVelocity = -bounceDir * throwForceX;
                otherEnemy.rb.linearVelocity = bounceDir * otherEnemy.throwForceX;

                StartCoroutine(BounceThenFall(rb.linearVelocity));
                otherEnemy.StartCoroutine(otherEnemy.BounceThenFall(otherEnemy.rb.linearVelocity));

                Debug.Log("Enemies bounced off each other — now falling through the floor!");
            }

            isThrown = false;
            return;
        }

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        HealthSystem hs = collision.gameObject.GetComponent<HealthSystem>();

        if (pc != null && hs != null)
        {

            if (pc.IsCharging())
            {
                ApplyChargeHit(pc.transform.position);
                return;
            }

            if (isStunned) return;

            if (pc.CanTakeDamage())
            {
                KnockbackReceiver knockback = pc.GetComponent<KnockbackReceiver>();
                if (knockback != null)
                    knockback.ApplyKnockback(transform.position);

                hs.TakeDamage(damage, transform.position);
                lastTouchTime = Time.time;
                if (animator) animator.SetTrigger("Attack");
            }
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            float direction = Random.value < 0.5f ? -1f : 1f;
            moveDirection = new Vector2(direction, 0f);
            yield return new WaitForSeconds(directionChangeInterval);
        }
    }

    Vector2 GetSlopeDirection()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRayLength, groundLayer);
        if (hit.collider != null)
        {
            Vector2 slope = Vector2.Perpendicular(hit.normal).normalized;
            return slope.x > 0 ? moveDirection : -moveDirection;
        }
        return moveDirection;
    }

    public void TakeDamage(float amount, DeathType source)
    {
        if (!isAlive) return;

        bool willSurvive = currentHealth - amount > 0f;

        if (willSurvive && damageSound && audioSource)
            audioSource.PlayOneShot(damageSound);

        currentHealth -= amount;

        if (!isBleeding && currentHealth > 0f && currentHealth <= maxHealth * 0.5f)
        {
            isBleeding = true;
            if (bleedingEffect) bleedingEffect.Play();
        }

        if (currentHealth <= 0f)
        {
            Die(source);
        }
    }

    void Die(DeathType source)
    {
        isAlive = false;

        if (bleedingEffect) bleedingEffect.Stop();
        if (deathSound && audioSource) audioSource.PlayOneShot(deathSound);

        if (deathEffect)
        {
            ParticleSystem fx = Instantiate(deathEffect, transform.position, Quaternion.identity);
            fx.transform.SetParent(null);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        StartCoroutine(SinkThenRespawn());
    }

    IEnumerator SinkThenRespawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        ResetState();

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
        rb.simulated = true;
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
        rb.linearVelocity = Vector2.zero;
        isAlive = true;
        isBleeding = false;

        if (bleedingEffect) bleedingEffect.Stop();
        if (animator) animator.ResetTrigger("Attack");

        StopAllCoroutines();
        StartCoroutine(ChangeDirectionRoutine());
    }

    void CheckLadderProximity()
    {
        Collider2D ladder = Physics2D.OverlapCircle(transform.position, ladderAvoidRadius, ladderLayer);
        avoidingLadder = ladder != null;
        nearestLadder = ladder ? ladder.transform : null;
    }

    public void ApplyChargeHit(Vector3 sourcePosition)
    {
        // Direction away from player
        Vector2 direction = (transform.position - sourcePosition).normalized;

        // Reset current velocity
        rb.linearVelocity = Vector2.zero;

        // Apply knockback with upward lift
        Vector2 knockbackVector = new Vector2(direction.x * knockbackForce, knockbackForce * 0.75f);
        rb.AddForce(knockbackVector, ForceMode2D.Impulse);

        // Stun logic
        isStunned = true;
        stunTimer = stunDuration;

        if (animator) animator.SetTrigger("Stunned");
    }

    public void PickUp(Transform anchor)
    {
        isCarried = true;
        rb.simulated = false;
        transform.SetParent(anchor);

        transform.position = anchor.position;
        Debug.Log("Enemy picked up!");

        if (animator) animator.SetBool("Carried", true);
    }

    public void Throw(Vector2 direction)
    {
        isCarried = false;
        isThrown = true;
        transform.SetParent(null);
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        transform.position += Vector3.up * 0.2f;

        Vector2 force = direction.normalized * throwForceX;
        rb.AddForce(force, ForceMode2D.Impulse);

        if (animator) animator.SetBool("Carried", false);
        Debug.Log($"Throw direction: {direction}, normalized: {direction.normalized}, force: {force}");
    }

    private IEnumerator ReenableCollision(Collider2D a, Collider2D b)
    {
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(a, b, false);
    }

    private IEnumerator BounceThenFall(Vector2 momentum)
    {
        yield return new WaitForSeconds(0.3f);

        rb.simulated = false;
        if (col != null) col.enabled = false;
        if (animator) animator.SetTrigger("Fall");

        float duration = 3f;
        float timer = 0f;

        while (timer < duration)
        {
            transform.position += new Vector3(momentum.x, -Mathf.Abs(momentum.y) - 6f, 0f) * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRayLength);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, Vector2.up * knockbackForceY);
    }
}
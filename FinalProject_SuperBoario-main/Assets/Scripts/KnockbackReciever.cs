using UnityEngine;
using System.Collections;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForceX = 25f;
    public float knockbackForceY = 18f;
    public float knockbackDuration = 0.5f;
    public float postKnockbackCooldown = 0.2f;
    public float maxKnockbackMagnitude = 30f;
    public string triggeringTag = "Enemy";
    public bool debugLog = true;

    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    private float knockbackCooldownTimer = 0f;
    private Vector2 knockbackVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (knockbackCooldownTimer > 0f)
            knockbackCooldownTimer -= Time.deltaTime;
    }

    public bool IsMovementBlocked()
    {
        return isKnockedBack || knockbackCooldownTimer > 0f;
    }

    public Vector2 GetKnockbackVelocity()
    {
        return knockbackVelocity;
    }

    // External knockback from hazards or enemies
    public void ApplyKnockback(Vector3 sourcePosition)
    {
        Vector2 direction = (transform.position - sourcePosition).normalized;
        Vector2 knockVelocity = new Vector2(direction.x * knockbackForceX, knockbackForceY);
        ApplyKnockback(knockVelocity);
    }

    // Internal knockback with custom velocity
    public void ApplyKnockback(Vector2 velocity)
    {
        StartCoroutine(DoKnockback(velocity));
    }

    private IEnumerator DoKnockback(Vector2 velocity)
    {
        GetComponent<PlayerController>()?.ResetHorizontalVelocity();

        isKnockedBack = true;

        velocity = Vector2.ClampMagnitude(velocity, maxKnockbackMagnitude);
        knockbackVelocity = velocity;
        rb.linearVelocity = velocity;

        if (debugLog)
            Debug.Log($"Knockback applied: {velocity}");

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        knockbackVelocity = Vector2.zero;
        knockbackCooldownTimer = postKnockbackCooldown;

        yield return new WaitForSeconds(0.1f);

        isKnockedBack = false;

        if (debugLog)
            Debug.Log("Knockback ends. Velocity reset.");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(triggeringTag) && !isKnockedBack)
        {
            Vector2 direction = (transform.position.x < collision.transform.position.x) ? Vector2.left : Vector2.right;
            Vector2 knockVelocity = new Vector2(direction.x * knockbackForceX, knockbackForceY);
            ApplyKnockback(knockVelocity);
        }
    }
}
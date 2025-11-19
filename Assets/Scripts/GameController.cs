using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    [Header("Particles")]
    public ParticleSystem waterParticles;
    public ParticleSystem damageParticles;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    private float damageCooldown = 1.5f;
    private float lastDamageTime = -999f;

    [SerializeField] private ScreenOverlayController screenOverlay;

    private PlayerRespawn2D respawn;
    private Rigidbody2D rb;
    private Vector3 checkpointPosition;

    private void Awake()
    {
        // Add this scene's treasures to the grand total
        CollectibleManager.AddSceneTreasuresToGrandTotal();

        // Reset per-level treasure counter
        CollectibleManager.ResetLevel();

        // Optional: Reset everything if this is the first scene
        // Uncomment this if you're using a persistent GameController and want to reset only once
        // CollectibleManager.ResetAll();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawn = GetComponent<PlayerRespawn2D>();
        checkpointPosition = transform.position;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController pc = GetComponent<PlayerController>();
        HealthSystem hs = GetComponent<HealthSystem>();
        if (pc == null || hs == null) return;

        KnockbackReceiver knockback = pc.GetComponent<KnockbackReceiver>();
        if (knockback != null && collision.CompareTag("Spike"))
            knockback.ApplyKnockback(collision.transform.position);

        if (!pc.CanTakeDamage()) return;

        if (collision.CompareTag("Spike"))
        {
            if (Time.time - lastDamageTime > damageCooldown)
            {
                damageParticles?.Play();
                lastDamageTime = Time.time;
                hs.TakeDamage(0.5f);
            }
        }

        if (collision.CompareTag("Water"))
        {
            waterParticles?.Play();
            SoundManager.Instance?.PlayWaterSplash();
            hs.TakeDamage(hs.personalHealth); // Full damage = death
        }
    }

    public void HandleDeath()
    {
        HealthSystem.ResetHealth();
        HealthSystem.ReassignCameraFocus();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
        }

        screenOverlay?.ShowDeathOverlay();
        StartCoroutine(RespawnAfterDelay(0.5f));
    }

    public void DelayedRespawn(float delay)
    {
        StartCoroutine(RespawnAfterDelay(delay));
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    public void Respawn()
    {
        HealthSystem.ResetHealth();

        transform.position = checkpointPosition;
        HealthSystem.ResetHealth(); // Reset both characters to 0.5 health
        Debug.Log("Respawning to: " + checkpointPosition);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
        }

        respawn?.Respawn();
        SoundManager.Instance?.PlayRespawn();
        HealthSystem.ReassignCameraFocus();
    }
    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        checkpointPosition = newCheckpoint;
    }

    public void ForceRespawn()
    {
        Respawn();
    }

    void Knockback(Transform source)
    {
        if (rb == null) return;

        Vector2 direction = (transform.position - source.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }
}
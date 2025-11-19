using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public float personalHealth = 0.5f;
    public static bool isDead = false;
    private static List<HealthSystem> instances = new();

    [SerializeField] private CinemachineCamera virtualCam;

    [Header("UI")]
    public GameObject heartPrefab;
    public Transform heartContainer;
    private List<HeartDisplay> hearts = new();

    [Header("Low Health Feedback")]
    public AudioSource heartbeatSource;
    public AudioSource breathingSource;
    public float heartbeatThreshold = 0.25f;
    public float stopThreshold = 0.5f;
    private bool heartbeatPlaying = false;
    public LowHealthVignette lowHealthVignette;

    [Header("Bleeding Effect")]
    public ParticleSystem bleedingParticles;
    public float bleedingTriggerThreshold = 0.25f;
    public float bleedingResetThreshold = 0.5f;
    private bool isBleeding = false;

    [Header("Sprite Feedback")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite hurtSprite;

    [Header("Damage Cooldown")]
    public float damageCooldown = 1f;
    private float lastDamageTime = -Mathf.Infinity;

    [Header("Invincibility Frames")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Collider2D playerCollider;

    [SerializeField] private HeartDisplay heartDisplay;

    public ScreenOverlayController screenOverlay;
    public PlayerController playerController;
    public Vector2 lastHitPosition;

    void Awake()
    {
        if (!instances.Contains(this))
            instances.Add(this);

        screenOverlay?.HideOverlay();
        lowHealthVignette?.HideVignette();
        playerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (heartPrefab != null && heartContainer != null)
        {
            GameObject heartGO = Instantiate(heartPrefab, heartContainer);
            HeartDisplay heart = heartGO.GetComponent<HeartDisplay>();
            if (heart != null) hearts.Add(heart);
        }

        UpdateHealthUI();
    }

    // Old version for scripts that don’t care about knockback
    public void TakeDamage(float damage)
    {
        // Default to player’s own position if none provided
        TakeDamage(damage, transform.position);
    }

    // New version with source position for knockback
    public void TakeDamage(float damage, Vector2 sourcePosition)
    {
        if (isInvincible || Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        lastHitPosition = sourcePosition;

        StartCoroutine(TemporarilyIgnoreCollisions());

        if (isDead || personalHealth <= 0f) return;

        personalHealth = Mathf.Max(personalHealth - damage, 0f);
        UpdateHealthUI();
        SoundManager.Instance?.PlayYellHurt();

        if (personalHealth <= 0f)
        {
            SoundManager.Instance?.PlayYellDeath();
            Debug.Log($"{gameObject.name} has died.");
            TriggerFullDeath();
        }

        CheckGlobalLowHealth();
    }

    IEnumerator TemporarilyIgnoreCollisions()
    {
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        List<Collider2D> ignored = new();

        foreach (Collider2D col in allColliders)
        {
            if (col.CompareTag("Enemy") || col.CompareTag("Spike"))
            {
                Physics2D.IgnoreCollision(playerCollider, col, true);
                ignored.Add(col);
            }
        }

        // Wait until invincibility timer expires
        yield return new WaitForSeconds(invincibilityDuration);

        // Re-enable collisions
        foreach (Collider2D col in ignored)
        {
            if (col != null)
                Physics2D.IgnoreCollision(playerCollider, col, false);
        }
    }

    void TriggerFullDeath()
    {
        isDead = true;
        instances.Remove(this);

        GameController controller = GetComponent<GameController>();
        screenOverlay?.ShowDeathOverlay();
        controller?.HandleDeath();

        Debug.Log($"{gameObject.name} fully removed from play.");
    }

    public void Heal(float amount)
    {
        personalHealth = Mathf.Clamp(personalHealth + amount, 0f, 1f);
        UpdateHealthUI();
        CheckLowHealthFeedback();
        CheckGlobalLowHealth();
        Debug.Log($"{gameObject.name} healed by {amount}, new health = {personalHealth}");
    }

    public static void ResetHealth()
    {
        isDead = false;

        foreach (HealthSystem hs in FindObjectsOfType<HealthSystem>())
        {
            hs.personalHealth = 1f;
            hs.UpdateHealthUI();
            hs.screenOverlay?.HideOverlay();
            hs.lowHealthVignette?.HideVignette();

            if (!instances.Contains(hs))
                instances.Add(hs);
        }

        CheckGlobalLowHealth();
    }

    public void UpdateHealthUI()
    {
        float tempHealth = personalHealth;
        foreach (HeartDisplay heart in hearts)
        {
            heart?.SetHeart(Mathf.Clamp(tempHealth, 0f, 1f));
            tempHealth -= 1f;
        }

        CheckLowHealthFeedback();
        UpdateSprite();
        CheckBleeding();
        CheckGlobalLowHealth();
    }

    public static void CheckGlobalLowHealth()
    {
        foreach (HealthSystem hs in instances)
        {
            float health = hs.personalHealth;
            bool shouldShow = health <= hs.heartbeatThreshold;

            if (shouldShow && !hs.heartbeatPlaying)
            {
                hs.heartbeatSource?.Play();
                hs.breathingSource?.Play();
                hs.lowHealthVignette?.ShowVignette();
                hs.heartbeatPlaying = true;
            }
            else if (!shouldShow && hs.heartbeatPlaying)
            {
                hs.heartbeatSource?.Stop();
                hs.breathingSource?.Stop();
                hs.lowHealthVignette?.HideVignette();
                hs.heartbeatPlaying = false;
            }
        }
    }

    void CheckLowHealthFeedback()
    {
        float healthPercent = personalHealth;

        if (personalHealth <= 0f)
        {
            if (heartbeatPlaying)
            {
                heartbeatSource?.Stop();
                breathingSource?.Stop();
                heartbeatPlaying = false;
            }

            lowHealthVignette?.HideVignette();
            return;
        }

        if (healthPercent <= heartbeatThreshold && !heartbeatPlaying)
        {
            heartbeatSource?.Play();
            breathingSource?.Play();
            heartbeatPlaying = true;
            lowHealthVignette?.ShowVignette();
        }
        else if (healthPercent > stopThreshold && heartbeatPlaying)
        {
            heartbeatSource?.Stop();
            breathingSource?.Stop();
            heartbeatPlaying = false;
            lowHealthVignette?.HideVignette();
        }
    }

    void CheckBleeding()
    {
        float healthRatio = personalHealth;

        if (!isBleeding && healthRatio <= bleedingTriggerThreshold)
        {
            if (bleedingParticles != null && !bleedingParticles.isPlaying)
            {
                bleedingParticles.Play();
                isBleeding = true;
            }
        }
        else if (isBleeding && healthRatio > bleedingResetThreshold)
        {
            if (bleedingParticles != null && bleedingParticles.isPlaying)
            {
                bleedingParticles.Stop();
                isBleeding = false;
            }
        }
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        float healthRatio = personalHealth;

        spriteRenderer.sprite = (healthRatio <= heartbeatThreshold && hurtSprite != null)
            ? hurtSprite
            : normalSprite;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("Invincibility ended.");
            }
        }
    }

    public static void ReassignCameraFocus()
    {
        foreach (HealthSystem hs in instances)
        {
            if (hs.virtualCam != null)
            {
                hs.virtualCam.Follow = hs.transform;
                Debug.Log("Camera reassigned to: " + hs.name);
                break;
            }
        }
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
#endif
}
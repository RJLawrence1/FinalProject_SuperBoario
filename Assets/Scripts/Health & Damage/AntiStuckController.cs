using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AntiStuckController : MonoBehaviour
{
    [Header("Anti-Stuck Settings")]
    public float stuckThreshold = 0.5f;        // Time in seconds before anti-stuck triggers
    public float knockbackForce = 10f;         // Upward force applied to escape
    public float teleportOffsetY = 1f;         // Teleport offset if knockback fails
    public float teleportDelay = 2f;           // Time before teleport fallback

    private Rigidbody2D rb;
    private Collider2D playerCol;
    private float stuckTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if overlapping with enemies or spikes
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Spike"))
        {
            stuckTimer += Time.deltaTime;

            // First response: knockback push
            if (stuckTimer >= stuckThreshold && stuckTimer < teleportDelay)
            {
                rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
                Debug.Log("Anti-stuck knockback applied!");
                stuckTimer = 0f; // reset after push
            }

            // Failsafe: teleport escape if still stuck
            if (stuckTimer >= teleportDelay)
            {
                transform.position += Vector3.up * teleportOffsetY;
                rb.linearVelocity = Vector2.zero;
                Debug.Log("Anti-stuck teleport triggered!");
                stuckTimer = 0f;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Reset timer when leaving enemy/spike
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Spike"))
        {
            stuckTimer = 0f;
        }
    }
}
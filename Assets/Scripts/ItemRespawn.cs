using UnityEngine;
using System.Collections;

public class ItemRespawn : MonoBehaviour
{
    private Vector3 startPosition;
    private AudioSource audioSource;
    public Transform[] crateSpawnPoints;
    public GameObject cratePrefab;

    [Header("Splash Settings")]
    public AudioClip splashSound;
    public float respawnDelay = 5f;

    [Header("Splash FX")]
    public ParticleSystem splashParticles;

    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }
    public void RespawnCrate(int index)
    {
        Instantiate(cratePrefab, crateSpawnPoints[index].position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            splashParticles.Play();
            SoundManager.Instance.PlayWaterSplash();
            StartCoroutine(SinkThenRespawn());
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    IEnumerator SinkThenRespawn()
    {
        float sinkDelay = 1f;
        float respawnDelay = 5f;

        // Optional: simulate sinking visually
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        yield return new WaitForSeconds(sinkDelay);

        // Hide crate
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null) rb.simulated = false;

        yield return new WaitForSeconds(respawnDelay);

        // Reset position and rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        // Show crate again
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
        if (rb != null) rb.simulated = true;
    }
}
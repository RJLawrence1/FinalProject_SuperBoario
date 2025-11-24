using UnityEngine;

public class CollectiblePickup : MonoBehaviour
{
    public enum CollectibleType { Coin, Gem, Treasure }

    [Header("Collectible Settings")]
    public CollectibleType type;
    public int value = 50; // default, can override per prefab

    [Header("Visuals")]
    public ParticleSystem pickupEffect;
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to global score
            CollectibleManager.totalScore += value;
            Debug.Log($"{type} collected! +{value} points. Total = {CollectibleManager.totalScore}");

            // Play particle effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destroy pickup
            Destroy(gameObject);
        }
    }
}
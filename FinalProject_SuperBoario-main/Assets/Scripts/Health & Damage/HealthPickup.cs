using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float healAmount = 1f;
    [SerializeField] private bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        HealthSystem hs = other.GetComponent<HealthSystem>();

        if (pc != null && hs != null && pc.CanTakeDamage())
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            hs.Heal(healAmount);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}
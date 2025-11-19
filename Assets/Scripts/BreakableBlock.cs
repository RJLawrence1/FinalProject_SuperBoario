using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public ParticleSystem breakEffect;
    public AudioClip breakSound;

    public void Break()
    {
        if (breakEffect) Instantiate(breakEffect, transform.position, Quaternion.identity);
        if (breakSound) AudioSource.PlayClipAtPoint(breakSound, transform.position);

        Destroy(gameObject);
    }
}
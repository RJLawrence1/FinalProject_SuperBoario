using UnityEngine;

public class Checkpoint2D : MonoBehaviour
{
    public ParticleSystem checkpointParticles;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        checkpointParticles?.Play();
        SoundManager.Instance?.PlayCheckpointTouch();

        // Update player component(s)
        PlayerRespawn2D respawn = other.GetComponent<PlayerRespawn2D>()
                                   ?? other.GetComponentInParent<PlayerRespawn2D>()
                                   ?? other.GetComponentInChildren<PlayerRespawn2D>();

        if (respawn != null)
        {
            respawn.SetCheckpoint(transform.position);
            Debug.Log("Checkpoint set to: " + transform.position + " for player object: " + other.name);
        }
        else
        {
            Debug.LogWarning("Checkpoint triggered but PlayerRespawn2D not found on: " + other.name +
                             ". Ensure the Player GameObject (or its parent/child with the collider) has PlayerRespawn2D and tag is 'Player'.");
        }

        // Also notify the GameController (if present) so any central controller keeps the same checkpoint
        GameController gc = FindObjectOfType<GameController>();
        if (gc != null)
        {
            gc.SetCheckpoint(transform.position);
            Debug.Log("GameController checkpoint updated to: " + transform.position);
        }
    }
}
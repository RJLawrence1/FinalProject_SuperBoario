using UnityEngine;

public class Checkpoint2D : MonoBehaviour
{
    private Vector3 lastCheckpointPosition;

    public ParticleSystem checkpointParticles;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            checkpointParticles.Play();
            SoundManager.Instance.PlayCheckpointTouch();

            PlayerRespawn2D respawn = other.GetComponent<PlayerRespawn2D>();
            if (respawn != null)
            {
                respawn.SetCheckpoint(transform.position);
                Debug.Log("Checkpoint set to: " + transform.position);
            }
        }
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        // Store the checkpoint position locally
        lastCheckpointPosition = checkpointPosition;
    }
}
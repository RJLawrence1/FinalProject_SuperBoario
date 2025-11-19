using UnityEngine;

public class PlayerRespawn2D : MonoBehaviour
{
    private Vector3 checkpointPosition;

    void Start()
    {
        checkpointPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        checkpointPosition = newCheckpoint;
    }

    public void Respawn()
    {
        transform.position = checkpointPosition;
        HealthSystem.ResetHealth(); // Reset both characters to 0.5 health
        Debug.Log("Respawning to: " + checkpointPosition);
    }
}
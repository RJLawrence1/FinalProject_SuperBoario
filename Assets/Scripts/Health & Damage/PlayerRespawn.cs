using UnityEngine;

public class PlayerRespawn2D : MonoBehaviour
{
    [SerializeField] private Vector3 checkpointPosition;
    private bool hasCustomCheckpoint = false;

    void Awake()
    {
        // Initialize to player's start position only once
        checkpointPosition = transform.position;
        Debug.Log("PlayerRespawn2D initialized. Starting respawn pos: " + checkpointPosition);
    }

    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        checkpointPosition = newCheckpoint;
        hasCustomCheckpoint = true;
        Debug.Log("PlayerRespawn2D.SetCheckpoint called. New checkpoint: " + checkpointPosition);
    }

    public void Respawn()
    {
        transform.position = checkpointPosition;
        HealthSystem.ResetHealth(); // Reset both characters to 0.5 health
        Debug.Log("Respawning to: " + checkpointPosition);
    }
}
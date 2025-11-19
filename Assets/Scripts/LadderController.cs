using UnityEngine;

public class LadderController : MonoBehaviour
{
    [Header("Ladder Detection")]
    public LayerMask ladderLayer;
    public Vector2 detectionBoxSize = new Vector2(0.5f, 1f);
    public float climbSpeed = 6f;

    [Header("Debug")]
    public bool showGizmos = true;

    public bool IsTouchingLadder { get; private set; }
    public float VerticalInput { get; private set; }

    private Vector2 detectionOriginOffset = Vector2.zero;

    void Update()
    {
        VerticalInput = Input.GetAxisRaw("Vertical");
        CheckForLadder();
    }

    public void CheckForLadder()
    {
        Vector2 origin = (Vector2)transform.position + detectionOriginOffset;
        IsTouchingLadder = Physics2D.OverlapBox(origin, detectionBoxSize, 0f, ladderLayer);
    }

    public void ForceExitLadder()
    {
        IsTouchingLadder = false;
    }

    public void SetDetectionOffset(Vector2 offset)
    {
        detectionOriginOffset = offset;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.green;
        Vector2 origin = Application.isPlaying
            ? (Vector2)transform.position + detectionOriginOffset
            : (Vector2)transform.position;

        Gizmos.DrawWireCube(origin, detectionBoxSize);
    }
}
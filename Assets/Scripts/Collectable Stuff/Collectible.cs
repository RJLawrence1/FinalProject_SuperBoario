using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] int scoreValue = 10; // Points this collectible gives

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log("Triggered with: " + collision.name);
        // Check if the object that touched us is tagged "Player"
        if (collision.CompareTag("Player"))
        {
            // Find the ScoreKeeper in the scene and add score
            ScoreKeeper scoreKeeper = FindObjectOfType<ScoreKeeper>();
            if (scoreKeeper != null)
            {
                scoreKeeper.AddScore(scoreValue);
            }

            // Destroy collectible after collecting
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2exit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the ScoreKeeper in the scene
            ScoreKeeper scoreKeeper = FindObjectOfType<ScoreKeeper>();

            if (scoreKeeper != null)
            {
                int currentScore = scoreKeeper.GetScore();

                if (currentScore >= 360)
                {
                    SceneManager.LoadScene("AllTreasureEnding");
                }
                else if (currentScore >= 300)
                {
                    SceneManager.LoadScene("GoodEnding");
                }
                else if (currentScore >= 200)
                {
                    SceneManager.LoadScene("NormalEnding");
                }
                else if (currentScore < 100)
                {
                    SceneManager.LoadScene("BadEnding");
                }
                else
                {
                    SceneManager.LoadScene("NoTreasureEndingw");
                }
            }
        }
    }
}

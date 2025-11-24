using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    int currentScore = 0;

    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log("Score: " + currentScore);
    }

    public int GetScore()
    {
        return currentScore;
    }
}

using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    private int currentScore = 0;

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UpdateScoreUI(); 
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}

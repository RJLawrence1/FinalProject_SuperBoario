using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingFlag : MonoBehaviour
{
    [Header("Flag Type")]
    public bool isFinalFlag = false;

    [Header("Ending Thresholds (final flag only)")]
    public int normalThreshold = 2000;
    public int goodThreshold = 4000;

    [Header("Ending Scenes")]
    public string noTreasureEndingScene;
    public string badEndingScene;
    public string normalEndingScene;
    public string goodEndingScene;
    public string allTreasureEndingScene;

    [Header("Multiplier Settings (per-level only)")]
    public float fastTimeThreshold = 60f;
    public float mediumTimeThreshold = 120f;
    public float fastMultiplier = 2f;
    public float mediumMultiplier = 1.5f;
    public float slowMultiplier = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isFinalFlag)
        {
            HandleFinalEnding();
        }
        else
        {
            ApplyLevelMultiplier();
            CollectibleManager.ResetLevel(); // clear per-level treasures
            // Load next level here
        }
    }

    private void ApplyLevelMultiplier()
    {
        float time = LevelTimer.elapsedTime;
        float multiplier = slowMultiplier;

        if (time <= fastTimeThreshold) multiplier = fastMultiplier;
        else if (time <= mediumTimeThreshold) multiplier = mediumMultiplier;

        int treasureBonus = CollectibleManager.treasuresCollectedLevel * CollectibleManager.treasureValue;
        int adjustedScore = Mathf.RoundToInt(treasureBonus * multiplier);

        CollectibleManager.totalScore += adjustedScore;

        Debug.Log($"Level complete! Treasure bonus: {adjustedScore} (x{multiplier}), Total Score = {CollectibleManager.totalScore}");
    }

    private void HandleFinalEnding()
    {
        int score = CollectibleManager.totalScore;
        int treasures = CollectibleManager.treasuresCollectedGlobal;

        if (treasures == 0)
        {
            SceneManager.LoadScene(noTreasureEndingScene);
            return;
        }

        if (treasures >= CollectibleManager.totalTreasuresInGame && CollectibleManager.totalTreasuresInGame > 0)
        {
            SceneManager.LoadScene(allTreasureEndingScene);
            return;
        }

        if (score < normalThreshold)
        {
            SceneManager.LoadScene(badEndingScene);
            return;
        }

        if (score < goodThreshold)
        {
            SceneManager.LoadScene(normalEndingScene);
            return;
        }

        SceneManager.LoadScene(goodEndingScene);
    }
}
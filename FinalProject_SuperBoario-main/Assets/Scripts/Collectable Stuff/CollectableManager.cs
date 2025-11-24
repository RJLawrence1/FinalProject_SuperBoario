using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static int totalScore = 0;

    // Global treasure tracking
    public static int treasuresCollectedGlobal = 0;
    public static int totalTreasuresInGame = 0;

    // Per-level tracking
    public static int treasuresCollectedLevel = 0;

    public static int coinValue = 50;
    public static int gemValue = 250;
    public static int treasureValue = 500;

    public static void AddCoin() => totalScore += coinValue;
    public static void AddGem() => totalScore += gemValue;
    public static void AddTreasure()
    {
        treasuresCollectedGlobal++;
        treasuresCollectedLevel++;
    }

    public static void ResetLevel() => treasuresCollectedLevel = 0;

    public static void ResetAll()
    {
        totalScore = 0;
        treasuresCollectedGlobal = 0;
        treasuresCollectedLevel = 0;
        totalTreasuresInGame = 0;
    }

    // Add treasures from current scene into the grand total
    public static void AddSceneTreasuresToGrandTotal()
    {
        GameObject[] treasures = GameObject.FindGameObjectsWithTag("Treasure");
        totalTreasuresInGame += treasures.Length;
        Debug.Log("Grand total treasures updated: " + totalTreasuresInGame);
    }
}
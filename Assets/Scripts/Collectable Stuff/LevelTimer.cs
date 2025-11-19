using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public static float elapsedTime = 0f;
    private bool running = true;

    void Awake()
    {
        elapsedTime = 0f;
        running = true;
    }

    void Update()
    {
        if (running)
            elapsedTime += Time.deltaTime;
    }

    public static void ResetTimer() => elapsedTime = 0f;
    public void StopTimer() => running = false;
}
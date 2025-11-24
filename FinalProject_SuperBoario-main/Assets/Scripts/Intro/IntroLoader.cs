using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroLoader : MonoBehaviour
{
    public float delay = 3f;
    public string nextSceneName = "MainMenu";

    void Start()
    {
        Invoke(nameof(LoadNextScene), delay);
    }

    void Update()
    {
        if (Input.anyKeyDown)
            LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
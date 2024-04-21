using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    
    string activeScene;

    private void Awake()
    {
        // singleton pattern
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Update() => activeScene = ActiveScene();

    public string ActiveScene() => SceneManager.GetActiveScene().name;

    public void RestartCurrentScene() => SceneManager.LoadSceneAsync(ActiveScene());

    public void GoToScene(string sceneName) => SceneManager.LoadSceneAsync(sceneName);

    public void Quit() => Application.Quit();

    public void LevelFinish()
    {
        if (activeScene == "_Level-1")
            GoToScene("_Level-2");
        else if (activeScene == "_Level-2")
            GoToScene("_Level-3");
        else if (activeScene == "_Level-3")
            GoToScene("_End-Screen");
    }

    public void ResetGame() => GoToScene("_Level-1");

    public void ResetCurrentLevel()
    {
        GoToScene(ActiveScene());
    }
}

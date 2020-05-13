using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the links between each scene
/// The method LoadScene(string) loads the scene with the name given as a string parameter
/// </summary>
public class MainMenu : MonoBehaviour {
    // loads the scene with the name sceneName
    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
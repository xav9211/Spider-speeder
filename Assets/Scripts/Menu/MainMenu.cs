using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

    void ShowHighScores() {
        
    }

    void Exit() {
        Application.Quit();
    }
}

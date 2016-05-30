using System;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Map;
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

    void LoadSeed() {
        GameObject textObj = GameObject.Find("/Canvas/MainMenu/LeftPane/SeedInputPane/InputField/Text");

        int seed = 0;
        Int32.TryParse(textObj.GetComponent<Text>().text, out seed);

        Map.initialSeed = seed;
        SceneManager.LoadScene("Game");
    }

    void ShowLoadSeed() {
        GameObject.Find("/Canvas/MainMenu/LeftPane/ButtonContainer").SetActive(false);
        GameObject.Find("/Canvas/MainMenu/LeftPane/SeedInputPane").SetActive(true);
    }

    void ExitSeedInputMenu() {
        GameObject.Find("/Canvas/MainMenu/LeftPane/SeedInputPane").SetActive(false);
        GameObject.Find("/Canvas/MainMenu/LeftPane/ButtonContainer").SetActive(true);
    }
}

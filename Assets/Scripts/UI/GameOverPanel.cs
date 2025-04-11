using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    public void OnButtonMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void OnButtonPlayAgain()
    {
        SceneManager.LoadScene("GameScene");
    }
}

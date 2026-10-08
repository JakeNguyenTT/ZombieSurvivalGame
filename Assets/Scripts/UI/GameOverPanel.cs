using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_GameOverTimeText;

    public void OnButtonMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuScene");
    }

    public void OnButtonPlayAgain()
    {
        // The reloaded scene's GameManager calls StartGame() itself
        Time.timeScale = 1;
        SceneManager.LoadScene("GameScene");
    }
}

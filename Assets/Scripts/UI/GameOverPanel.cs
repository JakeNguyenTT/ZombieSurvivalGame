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
        SceneManager.LoadScene("MenuScene");
    }

    public void OnButtonPlayAgain()
    {
        SceneManager.LoadScene("GameScene");
        GameManager.Instance.StartGame();
    }
}

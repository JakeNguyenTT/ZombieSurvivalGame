using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    public void OnButtonMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuScene");
    }

    public void OnButtonResume()
    {
        GameManager.Instance.ResumeGame();
        gameObject.SetActive(false);
        UIManager.Instance.HideFadeBackground();
    }
}

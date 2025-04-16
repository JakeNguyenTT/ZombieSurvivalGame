using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    public void OnButtonMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void OnButtonResume()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        UIManager.Instance.HideFadeBackground();
    }
}

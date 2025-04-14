using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayBGM(AudioID.BGM_Menu, true, 1.5f);
    }

    public void OnButtonStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}

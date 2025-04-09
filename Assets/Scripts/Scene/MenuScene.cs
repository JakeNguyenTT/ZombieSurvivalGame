using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    public void OnButtonStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}

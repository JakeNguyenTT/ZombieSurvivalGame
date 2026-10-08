using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayBGM(AudioID.BGM_Menu, true, 1.5f);

        Button startButton = FindStartButton();
        if (startButton != null)
            gameObject.AddComponent<MenuMeta>().Init(startButton);
    }

    public void OnButtonStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // The button wired to OnButtonStartGame in the scene; its style is reused for the menu extras
    private static Button FindStartButton()
    {
        Button[] buttons = FindObjectsByType<Button>();
        foreach (var button in buttons)
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentMethodName(i) == nameof(OnButtonStartGame))
                    return button;
        return buttons.Length > 0 ? buttons[0] : null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayBGM(AudioID.BGM_Game, true);
    }
}

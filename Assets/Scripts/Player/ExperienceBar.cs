using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Image m_Fill;
    [SerializeField] private TextMeshProUGUI m_LevelText;
    [SerializeField] private TextMeshProUGUI m_ExperienceText;

    public void SetFill(float fillAmount)
    {
        m_Fill.fillAmount = fillAmount;
    }

    public void SetLevelText(int level)
    {
        m_LevelText.text = level.ToString();
    }

    public void SetExperienceText(int experience)
    {
        m_ExperienceText.text = experience.ToString();
    }


}

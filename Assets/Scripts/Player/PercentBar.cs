using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PercentBar : MonoBehaviour
{
    public static PercentBar Instance;
    [SerializeField] private Image m_Fill;
    [SerializeField] private TextMeshProUGUI m_PercentText;
    public float Value
    {
        get => m_Fill.fillAmount;
        set
        {
            m_Fill.fillAmount = value;
            m_PercentText.text = $"{value * 100}%";
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetFill(float fillAmount)
    {
        m_Fill.fillAmount = fillAmount;
    }

    public void SetPercent(float percent)
    {
        m_PercentText.text = $"{percent * 100}%";
    }
}

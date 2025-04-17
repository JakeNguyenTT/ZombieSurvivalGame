using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PercentBar : MonoBehaviour
{
    public static PercentBar Instance;
    [SerializeField] private Image m_Fill;
    [SerializeField] private TextMeshProUGUI m_PercentText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetValue(float value, float maxValue, bool percentSign = false)
    {
        m_Fill.fillAmount = value / maxValue;
        if (percentSign)
            m_PercentText.text = $"{value * 100}%";
        else
            m_PercentText.text = $"{value:F2}";
    }
}

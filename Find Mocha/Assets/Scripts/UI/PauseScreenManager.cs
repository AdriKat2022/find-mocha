using TMPro;
using UnityEngine;

public class PauseScreenManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI totalTimeText;

    private void OnEnable()
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        TimeData timeData = new(CollectibleManager.TotalTimeSeconds);
        totalTimeText.text = timeData.FormatTime();
    }
}

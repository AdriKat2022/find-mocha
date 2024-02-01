using TMPro;
using UnityEngine;

public class TipSelector : MonoBehaviour
{
    private enum OrderType
    {
        Ascending,
        Descending,
        Random
    }

    [SerializeField]
    private Tips tips;

    [SerializeField]
    private OrderType tipsOrder;

    [SerializeField]
    private bool autoRefreshOnEnable;

    [SerializeField]
    private TMP_Text tipText;


    private void OnEnable()
    {
        if (autoRefreshOnEnable)
            RefreshTip();
    }

    public void RefreshTip()
    {
        if (tipText == null)
            return;

        switch(tipsOrder)
        {
            case OrderType.Ascending:
                tips.SetReverse(false);
                tipText.text = tips.GetNextTip();
                break;

            case OrderType.Descending:
                tips.SetReverse(true);
                tipText.text = tips.GetNextTip();
                break;

            case OrderType.Random:
                tipText.text = tips.GetRandomTip();
                break;
        }
    }
}

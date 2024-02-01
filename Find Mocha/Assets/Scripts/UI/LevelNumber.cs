using TMPro;
using UnityEngine;

public class LevelNumber : MonoBehaviour
{
    [SerializeField]
    private TMP_Text numberTextLevel;

    private int numberLevel;

    private void OnEnable()
    {
        UpdateNumberLevel(GameManager.Instance.CurrentSceneIndex);
    }

    public void UpdateNumberLevel(int index)
    {
        numberLevel = index;
        numberTextLevel.text = "Niveau " + numberLevel.ToString();
    }
}

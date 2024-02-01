using UnityEngine;

[CreateAssetMenu(menuName = "Tips/New TipGroup")]
public class Tips : ScriptableObject
{
    [SerializeField][TextArea]
    private string[] tipsList;

    public string[] TipsList => tipsList;
    public int TipsNumber => tipsList.Length;

    private int _currentIndex = 0;
    private bool _inReverse = false;

    public void SetReverse(bool inReverse) {  _inReverse = inReverse; }
    public string GetNextTip()
    {
        string outputTip;

        if (_inReverse)
            outputTip = tipsList[TipsNumber - _currentIndex - 1];
        else
            outputTip = tipsList[_currentIndex];

        _currentIndex++;
        _currentIndex %= TipsNumber;

        return outputTip;
    }
    public string GetRandomTip()
    {
        return tipsList[Random.Range(0, TipsNumber)];
    }
    public string GetTip(int index)
    {
        if (index >= TipsNumber || index < 0)
            return "";

        return tipsList[index];
    }
}

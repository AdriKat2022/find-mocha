using UnityEngine;
using UnityEngine.Events;

public class PlayerActivator : MonoBehaviour
{
    [SerializeField]
    public UnityEvent action;
    [SerializeField]
    public bool repetable;
    [SerializeField]
    private bool isActive = false;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        if (action != null && (!used || repetable))
        {
            used = true;
            action.Invoke();
        }
    }
}

using UnityEngine;
using UnityEngine.Events;

public class PlayerActivator : MonoBehaviour
{
    [SerializeField]
    public UnityEvent action;
    [SerializeField]
    public bool repetable;
    [SerializeField]
    private bool isActiveOnAwake = false;

    private bool used = false;

    public void ToogleActive(bool toogle) => isActiveOnAwake = toogle;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActiveOnAwake)
            return;

        if (action != null && (!used || repetable))
        {
            used = true;
            action.Invoke();
        }
    }
}

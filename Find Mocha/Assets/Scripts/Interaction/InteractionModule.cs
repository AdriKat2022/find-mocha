using UnityEngine;
using UnityEngine.Events;


// Works with a Trigger Collider 2D
// This script calls the events uppon interaction of the Player Controller
public class InteractionModule : MonoBehaviour, IInteractable
{
    [SerializeField]
    private UnityEvent onInteract;

    [SerializeField]
    private bool repeatable = false;


    private int interacted = 0;

    public void Interact(PlayerController player)
    {
        if(interacted == 0 || repeatable)
        {
            onInteract?.Invoke();
            interacted++;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController pController))
        {
            pController.Interactable = this;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (
                collision.gameObject.TryGetComponent(out PlayerController pController) &&
                (object)pController.Interactable == this
            )
        {
            pController.Interactable = null;
        }
    }
}

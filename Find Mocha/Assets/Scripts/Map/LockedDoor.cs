using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField]
    private bool dialogueOnLock = false;
    [Space]
    [SerializeField]
    private CloseFollow key;
    [SerializeField]
    private GameObject visualEKey;
    [SerializeField]
    private OneTimeExplosion effect;


    private Animator animator;
    private Collider2D triggerCollider;
    private ManualDialogueActivator dialogueActivator;

    private bool isLocked = true;
    private bool isOpenning = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<Collider2D>();
        dialogueActivator = GetComponent<ManualDialogueActivator>();
    }

    public void Check()
    {
        if(isOpenning)
            return;

        if (isLocked)
        {
            if (dialogueOnLock)
            {
                // Key has not been found yet
                dialogueActivator.RunDialogue();
            }
        }
        else
        {
            // Launch the animations and disable the collider
            key.SetTarget(transform);
            triggerCollider.enabled = false;
            animator.SetTrigger("Open");
            print(animator);
            visualEKey.SetActive(false);
            isOpenning = true;
            print("Door is openning");
        }
    }

    public void UnlockDoor()
    {
        // Called by the key to unlock the door
        isLocked = false;
    }

    public void Explode()
    {
        OneTimeExplosion o = Instantiate(effect, null, false);
        o.transform.position = transform.position;
        Destroy(gameObject);
    }
}

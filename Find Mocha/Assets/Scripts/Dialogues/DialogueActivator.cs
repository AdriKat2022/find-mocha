using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField]
    private DialogueObject dialogue;
    [SerializeField]
    private bool isActiveOnAwake = true;



    [Header("Particles")]


    [SerializeField]
    private bool useParticles = true;
    [SerializeField]
    private Color defaultColor;
    [SerializeField]
    private float defaultSpeed;
    [SerializeField]
    private Color interactableColor;
    [SerializeField]
    private float interactableSpeed;
    [Space]
    [SerializeField]
    private new ParticleSystem particleSystem;

    private GameObject player;
    private PlayerController playerController;

    private bool isActive;
    private bool isCurrentlyInteractable = false;

    public void SetDialogueActive(bool active)
    {
        isActive = active;

        if(!active && isCurrentlyInteractable)
        {
            ToggleInteractible(false);
        }
    }


    private void Start()
    {
        isActive = isActiveOnAwake;
        player = PlayerController.Instance.gameObject;

        if (player == null)
        {
            Debug.LogError("Error: no player found");
            return;
        }

        player.TryGetComponent(out playerController);

        ToggleInteractible(false);
    }

    public void Interact(PlayerController player)
    {
        GameManager.Instance.DialogueUI.ShowDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        if(collision.gameObject == player)
        {
            ToggleInteractible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActive)
            return;

        if (collision.gameObject == player)
        {
            ToggleInteractible(false);
        }
    }

    private void ToggleInteractible(bool toggle)
    {
        if(!isCurrentlyInteractable && toggle)
        {
            playerController.Interactable = this;
            isCurrentlyInteractable = toggle;
        }
        else if (isCurrentlyInteractable && !toggle)
        {
            if (playerController.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
                playerController.Interactable = null;

            isCurrentlyInteractable = toggle;
        }


        if (!useParticles)
            return;

        ParticleSystem.MainModule main = particleSystem.main;
            
        main.startColor = toggle ? interactableColor : defaultColor;
        main.startSpeed = toggle ? interactableSpeed : defaultSpeed;
    }
}

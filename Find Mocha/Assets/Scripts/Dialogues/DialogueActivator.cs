using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    public DialogueObject dialogue;

    public Color defaultColor;
    public float defaultSpeed;
    public Color interactableColor;
    public float interactableSpeed;

    private GameObject player;
    private PlayerController playerController;

    private bool interactable;

    [SerializeField] private new ParticleSystem particleSystem;

    private void Start()
    {
        if (player == null)
            player = PlayerController.Instance.gameObject;
        if (player == null)
            Debug.LogError("Error: no player found");

        player.TryGetComponent(out playerController);

        ToggleInteractible(false);
    }

    public void Interact(PlayerController player)
    {
        GameManager.Instance.DialogueUI.ShowDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == player)
        {
            playerController.Interactable = this;
            ToggleInteractible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            if(playerController.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
                playerController.Interactable = null;

            ToggleInteractible(false);
        }
    }

    private void ToggleInteractible(bool toggle)
    {
        interactable = toggle;

        ParticleSystem.MainModule main = particleSystem.main;
            
        main.startColor = toggle ? interactableColor : defaultColor;
        main.startSpeed = toggle ? interactableSpeed : defaultSpeed;
    }
}

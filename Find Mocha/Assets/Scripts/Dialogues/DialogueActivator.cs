using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    public DialogueObject dialogue;

    public Color defaultColor;
    public Color interactableColor;
    public float defaultSpeed;
    public float interactableSpeed;

    private GameObject player;
    private PlayerController playerController;

    private bool interactable;

    [SerializeField] private new ParticleSystem particleSystem;
    [SerializeField] private GameObject spaceInteractable;
    //private SpriteRenderer spaceSpriteRenderer;
    [SerializeField] private float spaceAnimationDepth;
    [SerializeField] private float spaceAnimationSpeed;
    private Vector2 refPosition;


    //[SerializeField] private TMP_Text debugText;

    private void Start()
    {
        if (player == null)
            player = PlayerController.Instance.gameObject;
        if (player == null)
            Debug.LogError("Error: no player found");

        player.TryGetComponent(out playerController);


        //TryGetComponent(out spaceSpriteRenderer);

        //particleSystem = transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();

        interactable = false;
        ToggleInteractible(false);
        refPosition = spaceInteractable.transform.position;
    }

    public void Interact(PlayerController player)
    {
        player.DialogueUI.ShowDialogue(dialogue);
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

        if (toggle)
            StartCoroutine(CanInteract());
        else
        {
            spaceInteractable.SetActive(false);
        }
    }

    private IEnumerator CanInteract()
    {
        float time = 0f;
        /*float fade = 0f;

        Color colorToAssign = spaceSpriteRenderer.color;
        colorToAssign.a = 0f;*/

        spaceInteractable.SetActive(true);


        while (true)
        {
            spaceInteractable.transform.position = refPosition + Mathf.Sin(time * spaceAnimationSpeed) * spaceAnimationDepth * Vector2.up;

            time += Time.deltaTime;

            if (!interactable)
            {
                spaceInteractable.SetActive(false);

                yield break;
            }

            yield return null;
        }
    }
}

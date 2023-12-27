using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public float bubbleAnimationTime;
    public float readyAnimationSpeed;
    public float readyAnimationDepth;
    public TMP_Text dialogueTextLabel;
    public TMP_Text dialogueTitleLabel;

    public GameObject dialogBubble;

    public bool IsOpen { get; private set; }


    private TypeWritterEffect typeWritter;

    public GameObject readyIcon;
    private Vector2 refPosition;
    private bool isReady;




    //[SerializeField] private TMP_Text debugText;

    private void Start()
    {
        dialogBubble.TryGetComponent(out typeWritter);
        IsOpen = false;
        isReady = false;
        readyIcon.SetActive(false);
        refPosition = readyIcon.transform.position;
        CloseDialogueBox();
    }

    #region Dialogues

    public void ShowDialogue(DialogueObject dialogueObjectToDisplay)
    {
        IsOpen = true;
        StartCoroutine(StartDialogueAnimation(dialogueObjectToDisplay));
    }

    private IEnumerator StartDialogueAnimation(DialogueObject dialogueObjectToDisplay)
    {
        float scale = 0f;

        dialogBubble.transform.localScale = new Vector3(1,scale,1);
        dialogBubble.SetActive(true);

        while(scale < 1f)
        {
            scale = Mathf.Min(1f, scale + Time.deltaTime/bubbleAnimationTime);
            dialogBubble.transform.localScale = new Vector3 (1,scale,1);

            yield return null;
        }

        StartCoroutine(StepThroughDialogue(dialogueObjectToDisplay));
    }

    private IEnumerator StopDialogueAnimation()
    {
        ResetDialogueBox();

        float scale = 1f;

        dialogBubble.transform.localScale = new Vector3(1, scale, 1);

        while (scale > 0f)
        {
            scale = Mathf.Max(0f, scale - Time.deltaTime / bubbleAnimationTime);
            dialogBubble.transform.localScale = new Vector3(1, scale, 1);

            yield return null;
        }

        CloseDialogueBox();
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        foreach(string dialogue in dialogueObject.Dialogue)
        {
            yield return RunTypingEffect(dialogue);

            dialogueTextLabel.text = dialogue;

            yield return null;

            StartCoroutine(IsReady());

            yield return new WaitUntil(() => Input.GetKeyDown("e"));

            SoundManager.Instance.PlaySound(SoundManager.Instance.button_press);

            isReady = false;
        }

        StartCoroutine(StopDialogueAnimation());
    }

    private IEnumerator RunTypingEffect(string dialogue)
    {
        typeWritter.RunDialog(dialogue, dialogueTextLabel);

        while (typeWritter.IsRunning)
        {
            yield return null;

            if (Input.GetKeyDown("e"))
            {
                typeWritter.Stop();
            }
        }
    }

    private void CloseDialogueBox()
    {
        IsOpen = false;
        dialogBubble.SetActive(false);
        ResetDialogueBox();
    }

    private void ResetDialogueBox()
    {
        dialogueTextLabel.text = string.Empty;
        dialogueTitleLabel.text = string.Empty;
    }


    private IEnumerator IsReady()
    {
        isReady = true;
        float time = 0f;

        readyIcon.SetActive(true);

        while (true)
        {
            readyIcon.transform.position = refPosition + Vector2.up * Mathf.Abs(Mathf.Sin(time * readyAnimationSpeed)) * readyAnimationDepth;

            time += Time.deltaTime;

            if (!isReady)
            {
                readyIcon.SetActive(false);

                yield break;
            }

            yield return null;
        }
    }

    #endregion
}

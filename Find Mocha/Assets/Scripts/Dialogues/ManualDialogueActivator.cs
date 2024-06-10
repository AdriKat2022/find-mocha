using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualDialogueActivator : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField]
    private DialogueObject dialogue;

    public void RunDialogue()
    {
        GameManager.Instance.DialogueUI.ShowDialogue(dialogue);
    }
}

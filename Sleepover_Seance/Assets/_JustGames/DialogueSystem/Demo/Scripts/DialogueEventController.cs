using UnityEngine;

public class DialogueEventController : MonoBehaviour
{
    public DialogueManager manager;
    public DialogueContainer dialogue;

    private void OnEnable()
    {
        DialogueManager.OnDialogueEvent += HandleDialogueEvent;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueEvent -= HandleDialogueEvent;
    }

    private void HandleDialogueEvent(string eventID, string parameter) // 'parameter' use is as needed.
    {
        if (eventID == "_EndDialogue")
        {
            //
        }

        /// Add Methods/Events HERE

    }

    void Start()
    {
        manager.StartDialogue(dialogue);
    }
}

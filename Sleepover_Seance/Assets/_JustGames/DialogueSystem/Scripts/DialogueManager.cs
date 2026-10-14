using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static event Action<string, string> OnDialogueEvent;
    public static Func<string, bool> ConditionEvaluator;

    [Header("Data Source")]
    [SerializeField] private DialogueContainer activeDialogue;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image speakerIconImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Input System Actions")]
    [SerializeField] private InputActionReference advanceDialogueAction;

    private DialogueNodeData _currentNode;

    private void OnEnable()
    {
        if (advanceDialogueAction != null && advanceDialogueAction.action != null)
        {
            advanceDialogueAction.action.Enable();
            advanceDialogueAction.action.performed += OnAdvanceInput;
        }
    }

    private void OnDisable()
    {
        if (advanceDialogueAction != null && advanceDialogueAction.action != null)
        {
            advanceDialogueAction.action.performed -= OnAdvanceInput;
            advanceDialogueAction.action.Disable();
        }
    }

    public void StartDialogue(DialogueContainer dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("[DialogueManager] Cannot start dialogue: DialogueContainer is NULL!");
            return;
        }

        if (dialogue.NodeData == null || dialogue.NodeData.Count == 0)
        {
            Debug.LogError($"[DialogueManager] Asset '{dialogue.name}' has no saved node data! Did you hit 'Save Graph' in the editor window?");
            return;
        }

        activeDialogue = dialogue;

        // Fetch Root Node or default to index 0
        DialogueNodeData rootNode = activeDialogue.GetNodeByID(activeDialogue.RootNodeID);
        if (rootNode == null)
        {
            rootNode = activeDialogue.NodeData[0];
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        DisplayNode(rootNode);
    }

    private void DisplayNode(DialogueNodeData node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        _currentNode = node;

        // 1. Process Event Node
        if (node.NodeType == DialogueNodeType.Event)
        {
            if (OnDialogueEvent == null)
            {
                Debug.LogWarning($"[DialogueManager] Event node fired (ID: '{node.EventID}', Param: '{node.EventParameter}') but no listeners are subscribed to OnDialogueEvent.");
            }
            else
            {
                Debug.Log($"[DialogueManager] Firing event — ID: '{node.EventID}', Param: '{node.EventParameter}'");
                OnDialogueEvent.Invoke(node.EventID, node.EventParameter);
            }

            if (node.Choices != null && node.Choices.Count > 0 && !string.IsNullOrEmpty(node.Choices[0].TargetNodeID))
            {
                DisplayNode(activeDialogue.GetNodeByID(node.Choices[0].TargetNodeID));
            }
            else
            {
                EndDialogue();
            }
            return;
        }

        // 2. Process Condition Node
        if (node.NodeType == DialogueNodeType.Condition)
        {
            if (ConditionEvaluator == null)
            {
                Debug.LogWarning($"[DialogueManager] Condition node '{node.ConditionKey}' evaluated but no ConditionEvaluator is assigned. Add a DialogueConditionProvider component to your scene. Defaulting to false.");
            }

            bool conditionMet = ConditionEvaluator != null && ConditionEvaluator.Invoke(node.ConditionKey);
            string targetID = conditionMet ? node.TargetNodeIfTrue : node.TargetNodeIfFalse;

            DisplayNode(activeDialogue.GetNodeByID(targetID));
            return;
        }

        // 3. Process Dialogue Node
        if (speakerNameText != null) speakerNameText.text = node.SpeakerName;
        if (speakerIconImage != null)
        {
            speakerIconImage.sprite = node.SpeakerIcon;
            speakerIconImage.gameObject.SetActive(node.SpeakerIcon != null);
        }
        if (dialogueText != null) dialogueText.text = node.DialogueText;

        if (choicesContainer != null)
        {
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            if (node.Choices != null && node.Choices.Count > 0)
            {
                foreach (var choice in node.Choices)
                {
                    if (string.IsNullOrEmpty(choice.TargetNodeID)) continue;

                    GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                    var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = choice.ChoiceText;

                    string targetID = choice.TargetNodeID;
                    btnObj.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(targetID));
                }
            }
        }
    }

    private void OnAdvanceInput(InputAction.CallbackContext context)
    {
        if (_currentNode != null && _currentNode.NodeType == DialogueNodeType.Dialogue)
        {
            if (_currentNode.Choices == null || _currentNode.Choices.Count == 0)
            {
                EndDialogue();
            }
        }
    }

    private void OnChoiceSelected(string targetNodeID)
    {
        DisplayNode(activeDialogue.GetNodeByID(targetNodeID));
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicesContainer.childCount > 0)
        {
            Button[] btn = choicesContainer.GetComponentsInChildren<Button>();

            foreach (Button item in btn)
            {
                item.gameObject.SetActive(false);
            }
        }
            _currentNode = null;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueNodeType
{
    Dialogue,
    Event,
    Condition
}

[Serializable]
public class DialogueNodeData
{
    public string NodeID = Guid.NewGuid().ToString();
    public DialogueNodeType NodeType = DialogueNodeType.Dialogue;

    // Dialogue Fields
    public string SpeakerName;
    public Sprite SpeakerIcon;
    [TextArea(3, 5)] public string DialogueText;

    // Event Fields
    public string EventID;
    public string EventParameter;

    // Condition Fields
    public string ConditionKey;
    public string TargetNodeIfTrue;
    public string TargetNodeIfFalse;

    public Vector2 NodePosition;
    public List<DialogueChoice> Choices = new List<DialogueChoice>();
}
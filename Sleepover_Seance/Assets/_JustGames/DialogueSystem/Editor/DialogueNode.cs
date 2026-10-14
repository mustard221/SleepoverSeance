#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueNode : Node
{
    public string NodeID;
    public DialogueNodeType NodeType = DialogueNodeType.Dialogue;

    // Dialogue UI Controls
    public TextField SpeakerField;
    public ObjectField SpeakerIconField;
    public TextField DialogueField; 

    // Event UI Controls
    public TextField EventIDField;
    public TextField EventParamField;

    // Condition UI Controls
    public TextField ConditionKeyField;
}
#endif
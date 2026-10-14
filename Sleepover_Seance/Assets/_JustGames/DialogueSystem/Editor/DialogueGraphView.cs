#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public DialogueGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public DialogueNode CreateDialogueNode(string title, string nodeID = null)
    {
        var node = BuildBaseNode(title, DialogueNodeType.Dialogue, nodeID);
        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.6f));

        node.SpeakerField = new TextField("Speaker") { value = "Speaker Name" };
        node.mainContainer.Add(node.SpeakerField);

        node.SpeakerIconField = new ObjectField("Speaker Icon") { objectType = typeof(UnityEngine.Sprite) };
        node.SpeakerIconField.style.maxWidth = 240;
        node.mainContainer.Add(node.SpeakerIconField);

        node.DialogueField = new TextField("Dialogue") { multiline = true, value = "Dialogue text..." };
        node.DialogueField.style.maxWidth = 240;
        node.mainContainer.Add(node.DialogueField);

        var addChoiceBtn = new Button(() => AddChoicePort(node)) { text = "Add Choice" };
        node.titleButtonContainer.Add(addChoiceBtn);

        AddChoicePort(node, "Next");
        return FinishNodeSetup(node);
    }

    public DialogueNode CreateEventNode(string title, string nodeID = null)
    {
        var node = BuildBaseNode(title, DialogueNodeType.Event, nodeID);
        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.2f, 0.6f));

        node.EventIDField = new TextField("Event ID") { value = "GiveItem" };
        node.mainContainer.Add(node.EventIDField);

        node.EventParamField = new TextField("Param") { value = "Gold_10" };
        node.mainContainer.Add(node.EventParamField);

        CreateStaticPort(node, "Next", Direction.Output);
        return FinishNodeSetup(node);
    }

    public DialogueNode CreateConditionNode(string title, string nodeID = null)
    {
        var node = BuildBaseNode(title, DialogueNodeType.Condition, nodeID);
        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.4f, 0.1f));

        node.ConditionKeyField = new TextField("Condition Key") { value = "HasGold" };
        node.mainContainer.Add(node.ConditionKeyField);

        CreateStaticPort(node, "True", Direction.Output);
        CreateStaticPort(node, "False", Direction.Output);

        return FinishNodeSetup(node);
    }

    private DialogueNode BuildBaseNode(string title, DialogueNodeType type, string nodeID)
    {
        var node = new DialogueNode
        {
            title = title,
            NodeType = type,
            NodeID = string.IsNullOrEmpty(nodeID) ? System.Guid.NewGuid().ToString() : nodeID
        };

        CreateStaticPort(node, "Input", Direction.Input);
        return node;
    }

    private DialogueNode FinishNodeSetup(DialogueNode node)
    {
        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(100, 100, 250, 200));
        AddElement(node);
        return node;
    }

    public Port AddChoicePort(DialogueNode node, string defaultChoiceText = "New Choice")
    {
        var choicePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

        var choiceTextField = new TextField { value = defaultChoiceText };
        choiceTextField.style.width = 100;
        choiceTextField.RegisterValueChangedCallback(evt => choicePort.portName = evt.newValue);

        choicePort.portName = defaultChoiceText;
        choicePort.contentContainer.Add(choiceTextField);

        var deleteBtn = new Button(() =>
        {
            var targetEdges = edges.ToList().FindAll(e => e.output == choicePort);
            foreach (var edge in targetEdges) RemoveElement(edge);
            node.outputContainer.Remove(choicePort);
            node.RefreshExpandedState();
            node.RefreshPorts();
        })
        { text = "X" };

        choicePort.contentContainer.Add(deleteBtn);
        node.outputContainer.Add(choicePort);
        node.RefreshExpandedState();
        node.RefreshPorts();
        return choicePort;
    }

    private void CreateStaticPort(DialogueNode node, string portName, Direction direction)
    {
        var port = Port.Create<Edge>(Orientation.Horizontal, direction, direction == Direction.Input ? Port.Capacity.Multi : Port.Capacity.Single, typeof(bool));
        port.portName = portName;
        if (direction == Direction.Input) node.inputContainer.Add(port);
        else node.outputContainer.Add(port);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => startPort != p && startPort.node != p.node && startPort.direction != p.direction).ToList();
    }
}
#endif
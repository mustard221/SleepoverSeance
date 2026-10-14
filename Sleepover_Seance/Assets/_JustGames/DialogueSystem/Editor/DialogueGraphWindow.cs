#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView _graphView;
    [SerializeField] private DialogueContainer _currentContainer;

    [MenuItem("Window/Dialogue Graph")]
    public static void OpenWindow()
    {
        GetWindow<DialogueGraphWindow>("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();

        if (_currentContainer != null)
        {
            LoadDataFromContainer();
        }
    }

    private void OnDisable()
    {
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var containerField = new ObjectField("Dialogue Asset")
        {
            objectType = typeof(DialogueContainer),
            value = _currentContainer
        };
        containerField.RegisterValueChangedCallback(evt =>
        {
            _currentContainer = evt.newValue as DialogueContainer;
            if (_currentContainer != null)
            {
                LoadDataFromContainer();
            }
        });
        toolbar.Add(containerField);

        var saveBtn = new Button(() => SaveData()) { text = "Save Graph" };
        toolbar.Add(saveBtn);

        var createNodeBtn = new Button(() => _graphView.CreateDialogueNode("Dialogue Node")) { text = "+ Dialogue Node" };
        toolbar.Add(createNodeBtn);

        var createEventBtn = new Button(() => _graphView.CreateEventNode("Event Node")) { text = "+ Event Node" };
        toolbar.Add(createEventBtn);

        var createConditionBtn = new Button(() => _graphView.CreateConditionNode("Condition Node")) { text = "+ Condition Node" };
        toolbar.Add(createConditionBtn);

        rootVisualElement.Add(toolbar);
    }

    private void SaveData()
    {
        if (_currentContainer == null)
        {
            Debug.LogError("[DialogueGraphWindow] No DialogueContainer assigned in toolbar!");
            return;
        }

        Undo.RecordObject(_currentContainer, "Save Dialogue Graph");
        _currentContainer.NodeData.Clear();

        var allNodes = _graphView.nodes.OfType<DialogueNode>().ToList();
        var edges = _graphView.edges.ToList();

        // Sort nodes in tree order via BFS from the root
        var nodes = GetNodesInTreeOrder(allNodes, edges);

        foreach (var node in nodes)
        {
            var nodeData = new DialogueNodeData
            {
                NodeID = node.NodeID,
                NodeType = node.NodeType,
                NodePosition = node.GetPosition().position
            };

            if (node.NodeType == DialogueNodeType.Dialogue)
            {
                nodeData.SpeakerName = node.SpeakerField.value;
                nodeData.SpeakerIcon = node.SpeakerIconField.value as UnityEngine.Sprite;
                nodeData.DialogueText = node.DialogueField.value;
            }
            else if (node.NodeType == DialogueNodeType.Event)
            {
                nodeData.EventID = node.EventIDField.value;
                nodeData.EventParameter = node.EventParamField.value;
            }
            else if (node.NodeType == DialogueNodeType.Condition)
            {
                nodeData.ConditionKey = node.ConditionKeyField.value;
            }

            var outputPorts = node.outputContainer.Children().OfType<Port>();
            foreach (var port in outputPorts)
            {
                var connectedEdge = edges.FirstOrDefault(e => e.output == port);
                if (connectedEdge != null && connectedEdge.input.node is DialogueNode targetNode)
                {
                    if (node.NodeType == DialogueNodeType.Condition)
                    {
                        if (port.portName == "True") nodeData.TargetNodeIfTrue = targetNode.NodeID;
                        else if (port.portName == "False") nodeData.TargetNodeIfFalse = targetNode.NodeID;
                    }
                    else if (node.NodeType == DialogueNodeType.Event)
                    {
                        // Event node has a single static "Next" port
                        nodeData.Choices.Add(new DialogueChoice
                        {
                            ChoiceText = "Next",
                            TargetNodeID = targetNode.NodeID
                        });
                    }
                    else
                    {
                        nodeData.Choices.Add(new DialogueChoice
                        {
                            ChoiceText = port.portName,
                            TargetNodeID = targetNode.NodeID
                        });
                    }
                }
            }

            _currentContainer.NodeData.Add(nodeData);
        }

        if (_currentContainer.NodeData.Count > 0)
        {
            _currentContainer.RootNodeID = _currentContainer.NodeData[0].NodeID;
        }

        EditorUtility.SetDirty(_currentContainer);
        AssetDatabase.SaveAssets();
        Debug.Log($"[DialogueGraphWindow] Successfully saved {_currentContainer.NodeData.Count} nodes to {_currentContainer.name}!");
    }

    private void LoadDataFromContainer()
    {
        if (_currentContainer == null) return;

        foreach (var node in _graphView.nodes.ToList()) _graphView.RemoveElement(node);
        foreach (var edge in _graphView.edges.ToList()) _graphView.RemoveElement(edge);

        var nodeMap = new Dictionary<string, DialogueNode>();

        foreach (var nodeData in _currentContainer.NodeData)
        {
            DialogueNode node = null;
            if (nodeData.NodeType == DialogueNodeType.Dialogue)
            {
                node = _graphView.CreateDialogueNode("Dialogue Node", nodeData.NodeID);
                node.SpeakerField.value = nodeData.SpeakerName;
                node.SpeakerIconField.value = nodeData.SpeakerIcon;
                node.DialogueField.value = nodeData.DialogueText;
            }
            else if (nodeData.NodeType == DialogueNodeType.Event)
            {
                node = _graphView.CreateEventNode("Event Node", nodeData.NodeID);
                node.EventIDField.value = nodeData.EventID;
                node.EventParamField.value = nodeData.EventParameter;
            }
            else if (nodeData.NodeType == DialogueNodeType.Condition)
            {
                node = _graphView.CreateConditionNode("Condition Node", nodeData.NodeID);
                node.ConditionKeyField.value = nodeData.ConditionKey;
            }

            if (node != null)
            {
                node.SetPosition(new Rect(nodeData.NodePosition, new Vector2(240, 200)));
                nodeMap[nodeData.NodeID] = node;
            }
        }

        foreach (var nodeData in _currentContainer.NodeData)
        {
            if (!nodeMap.TryGetValue(nodeData.NodeID, out var sourceNode)) continue;

            if (nodeData.NodeType == DialogueNodeType.Condition)
            {
                ConnectConditionPort(sourceNode, "True", nodeData.TargetNodeIfTrue, nodeMap);
                ConnectConditionPort(sourceNode, "False", nodeData.TargetNodeIfFalse, nodeMap);
            }
            else if (nodeData.NodeType == DialogueNodeType.Event)
            {
                // Event node has a single static "Next" port
                if (nodeData.Choices != null && nodeData.Choices.Count > 0)
                {
                    ConnectConditionPort(sourceNode, "Next", nodeData.Choices[0].TargetNodeID, nodeMap);
                }
            }
            else
            {
                var outputPorts = sourceNode.outputContainer.Children().OfType<Port>().ToList();
                for (int i = 0; i < nodeData.Choices.Count; i++)
                {
                    var choice = nodeData.Choices[i];
                    Port choicePort = (i < outputPorts.Count) ? outputPorts[i] : _graphView.AddChoicePort(sourceNode, choice.ChoiceText);
                    choicePort.portName = choice.ChoiceText;

                    if (!string.IsNullOrEmpty(choice.TargetNodeID) && nodeMap.TryGetValue(choice.TargetNodeID, out var targetNode))
                    {
                        var edge = choicePort.ConnectTo(targetNode.inputContainer.Q<Port>());
                        _graphView.AddElement(edge);
                    }
                }
            }
        }
    }

    private void ConnectConditionPort(DialogueNode sourceNode, string portName, string targetID, Dictionary<string, DialogueNode> nodeMap)
    {
        if (string.IsNullOrEmpty(targetID) || !nodeMap.TryGetValue(targetID, out var targetNode)) return;
        var port = sourceNode.outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.portName == portName);
        if (port != null)
        {
            var edge = port.ConnectTo(targetNode.inputContainer.Q<Port>());
            _graphView.AddElement(edge);
        }
    }

    /// <summary>
    /// Returns nodes ordered by BFS tree traversal starting from the root.
    /// Root is the node with no incoming edges. Disconnected nodes are appended at the end.
    /// </summary>
    private List<DialogueNode> GetNodesInTreeOrder(List<DialogueNode> allNodes, List<Edge> edges)
    {
        if (allNodes.Count == 0) return allNodes;

        // Build a set of nodes that have incoming edges
        var nodesWithIncoming = new HashSet<DialogueNode>();
        foreach (var edge in edges)
        {
            if (edge.input?.node is DialogueNode inputNode)
            {
                nodesWithIncoming.Add(inputNode);
            }
        }

        // Root = node with no incoming edges; fallback to leftmost node by X position
        DialogueNode root = allNodes.FirstOrDefault(n => !nodesWithIncoming.Contains(n));
        if (root == null)
        {
            root = allNodes.OrderBy(n => n.GetPosition().x).First();
        }

        // BFS from root
        var ordered = new List<DialogueNode>();
        var visited = new HashSet<string>();
        var queue = new Queue<DialogueNode>();

        queue.Enqueue(root);
        visited.Add(root.NodeID);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            ordered.Add(current);

            // Find all nodes connected to this node's output ports
            var outputPorts = current.outputContainer.Children().OfType<Port>();
            foreach (var port in outputPorts)
            {
                var connectedEdge = edges.FirstOrDefault(e => e.output == port);
                if (connectedEdge?.input?.node is DialogueNode targetNode && !visited.Contains(targetNode.NodeID))
                {
                    visited.Add(targetNode.NodeID);
                    queue.Enqueue(targetNode);
                }
            }
        }

        // Append any disconnected nodes so they aren't lost
        foreach (var node in allNodes)
        {
            if (!visited.Contains(node.NodeID))
            {
                ordered.Add(node);
            }
        }

        return ordered;
    }
}
#endif
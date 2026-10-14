using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Container", menuName = "Dialogue/Container")]
public class DialogueContainer : ScriptableObject
{
    public string RootNodeID;
    public List<DialogueNodeData> NodeData = new List<DialogueNodeData>();

    public DialogueNodeData GetNodeByID(string id)
    {
        if (NodeData == null || NodeData.Count == 0) return null;
        return NodeData.Find(node => node.NodeID == id);
    }
}
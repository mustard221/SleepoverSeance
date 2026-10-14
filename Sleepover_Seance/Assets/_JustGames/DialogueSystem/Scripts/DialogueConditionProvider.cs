using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueConditionProvider : MonoBehaviour
{
    [Serializable]
    public class ConditionEntry
    {
        public string Key;
        public bool Value;
    }

    [Header("Define conditions that Dialogue Condition Nodes can evaluate")]
    [SerializeField] private List<ConditionEntry> conditions = new List<ConditionEntry>();

    private Dictionary<string, bool> _conditionLookup = new Dictionary<string, bool>();

    private void Awake()
    {
        RebuildLookup();
        DialogueManager.ConditionEvaluator = EvaluateCondition;
    }

    private void OnDestroy()
    {
        if (DialogueManager.ConditionEvaluator == (Func<string, bool>)EvaluateCondition)
        {
            DialogueManager.ConditionEvaluator = null;
        }
    }

    private void RebuildLookup()
    {
        _conditionLookup.Clear();
        foreach (var entry in conditions)
        {
            if (!string.IsNullOrEmpty(entry.Key))
            {
                _conditionLookup[entry.Key] = entry.Value;
            }
        }
    }

    /// <summary>
    /// Evaluates a condition key. Returns false if the key is not found.
    /// </summary>
    public bool EvaluateCondition(string key)
    {
        if (_conditionLookup.TryGetValue(key, out bool value))
        {
            return value;
        }

        Debug.LogWarning($"[DialogueConditionProvider] Condition key '{key}' not found. Returning false.");
        return false;
    }

    /// <summary>
    /// Set a condition at runtime (e.g. after the player picks up an item).
    /// </summary>
    public void SetCondition(string key, bool value)
    {
        _conditionLookup[key] = value;

        // Keep the serialized list in sync for Inspector visibility
        var entry = conditions.Find(e => e.Key == key);
        if (entry != null)
        {
            entry.Value = value;
        }
        else
        {
            conditions.Add(new ConditionEntry { Key = key, Value = value });
        }
    }

    /// <summary>
    /// Check a condition value without the warning log.
    /// </summary>
    public bool GetCondition(string key)
    {
        return _conditionLookup.TryGetValue(key, out bool value) && value;
    }
}

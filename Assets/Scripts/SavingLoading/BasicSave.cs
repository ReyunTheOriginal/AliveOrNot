using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicSave : MonoBehaviour, SaveableObject
{
    // Static registry lives only for the current session — that's fine, it's just for runtime dedup
    public static Dictionary<string, SaveableObject> UsedGUIDs = new Dictionary<string, SaveableObject>();
    public static Dictionary<int, SaveableObject> UsedTypeIDs = new Dictionary<int, SaveableObject>();

    [SerializeField] private string IDDraft;

    virtual public string InstanceID
    {
        get => IDDraft;
        set => IDDraft = value;
    }

    public int TypeIDDraft;
    virtual public int TypeID => TypeIDDraft;
    virtual public bool DoNotInstantiate => false;
    virtual public string ObjectType => "BasicObjects";
    virtual public bool SaveTransform => true;
    private bool _SavedByParent = false;
    virtual public bool SavedByParent{
        get => _SavedByParent;
        set => _SavedByParent = value;
    }

    private bool _CanBeSaved = false;
    virtual public bool CanBeSave{
        get => _CanBeSaved;
        set => _CanBeSaved = value;
    }

#if UNITY_EDITOR
    private void OnValidate(){
        // In editor, only assign if truly empty (i.e. a brand new object)
        EnsureUniqueID();
        if (TypeIDDraft == 0)
            DraftTypeID();
    }

    // Called by Unity when a prefab instance is created in the scene
    private void Reset(){
        // Force a fresh GUID whenever this component is first added or the object is reset
        IDDraft = string.Empty;
        EnsureUniqueID();
    }
#endif

    private void Awake(){
        EnsureUniqueID();
        if (TypeIDDraft == 0)
            DraftTypeID();
    }

    /// <summary>
    /// Assigns a new GUID only if the slot is empty or already claimed by a different object.
    /// Otherwise registers the existing ID as-is.
    /// </summary>
    private void EnsureUniqueID(){
        // No ID yet — generate one
        if (string.IsNullOrEmpty(IDDraft))
        {
            IDDraft = Guid.NewGuid().ToString();
            UsedGUIDs[IDDraft] = this;
            MarkDirty();
            return;
        }

        // ID exists — check if another object owns it
        if (UsedGUIDs.TryGetValue(IDDraft, out var existing) && !ReferenceEquals(existing, this))
        {
            // Duplicate detected (e.g. a prefab copy that kept the same serialized GUID)
            IDDraft = Guid.NewGuid().ToString();
            MarkDirty();
        }

        UsedGUIDs[IDDraft] = this;
    }

    // Marks the object dirty in the editor so the new GUID is saved to the scene file
    private void MarkDirty(){
        #if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public void DraftTypeID(){
        int candidate = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        while (UsedTypeIDs.TryGetValue(candidate, out var existing) && !ReferenceEquals(existing, this))
            candidate = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        TypeIDDraft = candidate;
        UsedTypeIDs[TypeIDDraft] = this;
        MarkDirty();
    }

    public virtual string Save() => "";
    public virtual void Load(string json) { }
}
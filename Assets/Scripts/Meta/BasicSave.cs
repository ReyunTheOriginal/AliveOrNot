using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicSave : MonoBehaviour, SaveableObject
{
    public static Dictionary<string, SaveableObject> UsedGUIDs = new Dictionary<string, SaveableObject>();
    public string IDDraft;
    public string InstanceID
    {
        get => IDDraft;
        set => IDDraft = value;
    }
    public int TypeIDDraft;
    public int TypeID => TypeIDDraft;
    public bool DoNotInstantiate => true;
    public string ObjectType => "BasicObjects";
    public bool SaveTransform => true;

    #if UNITY_EDITOR
    private void OnValidate(){
        DraftID();
    }
    #endif

    private void Awake() {
        DraftID();
    }

    public void DraftID(){
        if (string.IsNullOrEmpty(IDDraft)){
            IDDraft = Guid.NewGuid().ToString();
        }

        if (UsedGUIDs.TryGetValue(IDDraft, out var existing)){
            if ((object)existing != (object)this){
                IDDraft = Guid.NewGuid().ToString();
            }
        }

        UsedGUIDs[IDDraft] = this;
    }

    public string Save(){
        return "";
    }

    // Update is called once per frame
    public void Load(string Json){

    }

}

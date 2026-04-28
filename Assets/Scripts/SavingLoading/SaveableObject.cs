using UnityEngine;

public interface SaveableObject
{
    int TypeID {get;}
    string InstanceID {get; set;}
    string ObjectType {get;}
    bool SaveTransform {get;}
    bool DoNotInstantiate {get;}
    bool SavedByParent {get; set;}
    bool CanBeSave {get; set;}
    GameObject gameObject{get;}
    string Save();
    void Load(string Json);
}

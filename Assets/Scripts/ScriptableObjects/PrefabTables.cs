using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PrefabTables)), CanEditMultipleObjects]
public class PrefabTablesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PrefabTables table = (PrefabTables)target;
        if (GUILayout.Button("Update Game Objects")){
            table.BuildGameObjectDictionary();
            table.BuildItemDictionary();  
        }
        DrawDefaultInspector();
    }
}
#endif

[CreateAssetMenu(menuName = "ScriptableObjects/PrefabTables")]
public class PrefabTables : ScriptableObject
{
    [SerializeField]public List<ItemProperties> Items = new List<ItemProperties>();

    private Dictionary<int, ItemProperties> _itemSaves;

    public Dictionary<int, ItemProperties> ItemSaves{
        get{
            BuildItemDictionary();
            return _itemSaves;
        }
    }

    private Dictionary<int, SaveableObject> _GameObjectSaves;

    public Dictionary<int, SaveableObject> GameObjectSaves{
        get{
            BuildGameObjectDictionary();
            return _GameObjectSaves;
        }
    }

    public void BuildItemDictionary(){
        _itemSaves = new Dictionary<int, ItemProperties>();
        foreach (ItemProperties prop in Items){
            if (!_itemSaves.ContainsKey(prop.ID))
                _itemSaves.Add(prop.ID, prop);
            else
                Debug.LogWarning($"Duplicate Item ID {prop.ID}: {prop.ItemName}");
        }
    }

    public void BuildGameObjectDictionary(){
        _GameObjectSaves = new Dictionary<int, SaveableObject>();

    #if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids){
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            SaveableObject saveable =
                prefab.GetComponents<MonoBehaviour>()
                    .OfType<SaveableObject>()
                    .FirstOrDefault();

            if (saveable == null)
                continue;

            if (!_GameObjectSaves.ContainsKey(saveable.TypeID))
                _GameObjectSaves.Add(saveable.TypeID, saveable);
            else
                Debug.LogWarning($"Duplicate TypeID {saveable.TypeID} on prefab {prefab.name}");
        }
    #endif
    }

    private void OnEnable(){
        BuildGameObjectDictionary();
        BuildItemDictionary();
    }
}
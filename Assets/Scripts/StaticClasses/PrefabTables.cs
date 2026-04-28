using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PrefabTables
{
    private static Dictionary<int, ItemProperties> _itemSaves;

    public static Dictionary<int, ItemProperties> ItemSaves{
        get{
            BuildItemDictionary();
            return _itemSaves;
        }
    }

    private static Dictionary<int, SaveableObject> _GameObjectSaves;

    public static Dictionary<int, SaveableObject> GameObjectSaves{
        get{
            BuildGameObjectDictionary();
            return _GameObjectSaves;
        }
    }

    public static void BuildItemDictionary(){
        _itemSaves = new Dictionary<int, ItemProperties>();
        #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids){
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                ItemProperties saveableItems = prefab.GetComponents<MonoBehaviour>()
                    .OfType<ItemProperties>()
                    .FirstOrDefault();

                if (saveableItems == null) continue;

                if (!_itemSaves.ContainsKey(saveableItems.ID))
                    _itemSaves.Add(saveableItems.ID, saveableItems);
                else
                    Debug.LogWarning($"Duplicate TypeID {saveableItems.ID} on prefab {prefab.name} with {_itemSaves[saveableItems.ID].name}");
            }
        #endif
    }

    public static void BuildGameObjectDictionary(){
        _GameObjectSaves = new Dictionary<int, SaveableObject>();
        #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids){
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                SaveableObject saveable = prefab.GetComponents<MonoBehaviour>()
                    .OfType<SaveableObject>()
                    .FirstOrDefault();

                if (saveable == null) continue;

                if (!_GameObjectSaves.ContainsKey(saveable.TypeID))
                    _GameObjectSaves.Add(saveable.TypeID, saveable);
                else
                    Debug.LogWarning($"Duplicate TypeID {saveable.TypeID} on prefab {prefab.name}");
            }
        #endif
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnGameStart(){
       BuildGameObjectDictionary();
        BuildItemDictionary();
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Saving
{   
    public static Dictionary<string, SaveableObject> AlreadyExistingGameObjects = new Dictionary<string, SaveableObject>();
    public static int SaveFileIndex;

    public static void UpdateGameObjectList(bool IncludeInactive = true){
        AlreadyExistingGameObjects.Clear();

        SaveableObject[] Saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            IncludeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        ).OfType<SaveableObject>().ToArray();

        foreach(SaveableObject saveable in Saveables)
            if(AlreadyExistingGameObjects != null && saveable != null && saveable.InstanceID != null && !AlreadyExistingGameObjects.ContainsKey(saveable.InstanceID))
                AlreadyExistingGameObjects.Add(saveable.InstanceID, saveable);
    }

    public static bool SaveFileAvailable(int index){
        string dirpath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}");
        return Directory.Exists(dirpath) && Directory.GetDirectories(dirpath).Length > 0;
    }
///////////////////////////////////////////////////////////////////////
#region <Saving>
    public static void SaveAll(int index){
        GameUtils.StartIndependentCoroutine(() => _SaveAll(index));
    }
    public static IEnumerator _SaveAll(int FileIndex){
        UIManager.CloseAllMenus();
        FileIndex = Mathf.Clamp(FileIndex, 0, 3);

        UpdateGameObjectList();

        if (SaveFileAvailable(FileIndex))
            Directory.Delete(Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}"), true);

        foreach(SaveableObject Saveable in AlreadyExistingGameObjects.Values){
            // skip objects that are saved by their Parent
            if (Saveable != null && ((Saveable.SavedByParent && Saveable.gameObject.transform.parent) || !Saveable.CanBeSave)) continue;

            SavedGameObject SavedGameObject = new SavedGameObject();

            SavedGameObject.Position = Saveable.gameObject.transform.position;
            SavedGameObject.Rotation = Saveable.gameObject.transform.rotation;
            SavedGameObject.Scale = Saveable.gameObject.transform.localScale;
            SavedGameObject.name = Saveable.gameObject.name;
            SavedGameObject.CustonSave = Saveable.Save();
            SavedGameObject.TypeID = Saveable.TypeID;
            SavedGameObject.InstanceID = Saveable.InstanceID;

            string SavedGameObjectJson = JsonUtility.ToJson(SavedGameObject);

            string path = Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}");

            Directory.CreateDirectory(Path.Combine(path, Saveable.ObjectType));

            File.WriteAllText(Path.Combine(path, Saveable.ObjectType, $"SavedGameObject_{Saveable.InstanceID}_{Saveable.TypeID}.json"), SavedGameObjectJson);

            //Debug.Log($"Saved: {Saveable.gameObject.name}, {Saveable.InstanceID}, {Saveable.TypeID}");
        }

        SaveSaveFileInformation(FileIndex);

        Debug.Log($"Saved SaveFile: {FileIndex}");
        yield return null;
    }

    private static void SaveSaveFileInformation(int index){
        string MetaPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}");

        SavefileInfo info = new SavefileInfo();

        info.PlayTime = GameServices.GlobalTimer;
        info.Seed = GameServices.WorldGenerationBase.Seed;

        string json = JsonUtility.ToJson(info);

        File.WriteAllText(Path.Combine(MetaPath, "Core_meta.json"), json);
    }
#endregion <Saving End>
/////////////////////////////////////////////////////////////////////////////////////////////////
#region <Loading>
    public static void LoadAll(int index){
        GameUtils.StartIndependentCoroutine(() => _LoadAll(index));
    }
    public static IEnumerator _LoadAll(int FileIndex){
        UIManager.CloseAllMenus();
        FileIndex = Mathf.Clamp(FileIndex, 0, 3);

        if (!SaveFileAvailable(FileIndex))
            yield break;

        UpdateGameObjectList();
        PrefabTables.BuildGameObjectDictionary();

        LoadSaveFileInformation(FileIndex);

        string path = Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}");

        foreach (string dir in Directory.GetDirectories(path).Where(d => !Path.GetFileName(d).StartsWith("Core_"))){
            foreach (string file in Directory.GetFiles(Path.Combine(path, dir))){
                string json = File.ReadAllText(file);
                LoadGameObject(json);
                yield return null;
            }
        }

        Debug.Log($"Loaded SaveFile: {FileIndex}");
    }

    public static GameObject LoadGameObject(string Json){
        // SavedObject_abc123_5
        SavedGameObject GameObject = JsonUtility.FromJson<SavedGameObject>(Json);

        string InstanceID = GameObject.InstanceID;
        int TypeID = GameObject.TypeID;

        SaveableObject existing = null;

        if (AlreadyExistingGameObjects.ContainsKey(InstanceID))
            existing = AlreadyExistingGameObjects[InstanceID];

        if (AlreadyExistingGameObjects.ContainsKey(InstanceID) && existing is UnityEngine.Object unityObj && unityObj != null){
            AlreadyExistingGameObjects[InstanceID].gameObject.transform.position =   GameObject.Position;
            AlreadyExistingGameObjects[InstanceID].gameObject.transform.rotation =   GameObject.Rotation;
            AlreadyExistingGameObjects[InstanceID].gameObject.transform.localScale = GameObject.Scale;
            AlreadyExistingGameObjects[InstanceID].gameObject.name =                 GameObject.name;

            AlreadyExistingGameObjects[InstanceID].Load(GameObject.CustonSave);

            return AlreadyExistingGameObjects[InstanceID].gameObject;
        }else if (PrefabTables.GameObjectSaves.ContainsKey(TypeID) && !PrefabTables.GameObjectSaves[TypeID].DoNotInstantiate){
            GameObject obj = UnityEngine.Object.Instantiate(PrefabTables.GameObjectSaves[TypeID].gameObject, new Vector2(0,0), Quaternion.identity);
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName("OverWorld"));

            SaveableObject Saveable = obj.GetComponents<MonoBehaviour>()
                                         .OfType<SaveableObject>()
                                         .FirstOrDefault();

            if (Saveable != null){
                Saveable.gameObject.transform.position =   GameObject.Position;
                Saveable.gameObject.transform.rotation =   GameObject.Rotation;
                Saveable.gameObject.transform.localScale = GameObject.Scale;
                Saveable.gameObject.name =                 GameObject.name;
                Saveable.InstanceID = InstanceID;

                Saveable.Load(GameObject.CustonSave);

                return obj;
            }
        }else{
            Debug.Log($"No ID Detected for Object: InstanceID: {InstanceID}, TypeID: {TypeID}");
            return null;
        }

        return null;
    }

    public static SavefileInfo LoadSaveFileInformation(int index){
        if (SaveFileAvailable(index)){
            string MetaPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}" , "Core_meta.json");

            SavefileInfo info = new SavefileInfo();

            info = JsonUtility.FromJson<SavefileInfo>(File.ReadAllText(MetaPath));

            GameServices.GlobalTimer = info.PlayTime;

            return info;
        }
        return null;
    }
#endregion <Loading End>
/////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class SavedGameObject{
        public string name;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public string CustonSave;

        public int TypeID;
        public string InstanceID;
    }

    [System.Serializable]
    public class SavefileInfo{
        public float PlayTime;
        public int Seed;
    }
}

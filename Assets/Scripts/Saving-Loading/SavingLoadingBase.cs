using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavingLoadingBase : MonoBehaviour
{   
    public Dictionary<string, SaveableObject> AlreadyExistingGameObjects = new Dictionary<string, SaveableObject>();
    public PrefabTables prefabTables;
    public int SaveFileIndex;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateGameObjectList(){
        AlreadyExistingGameObjects.Clear();

        SaveableObject[] Saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<SaveableObject>().ToArray();

        foreach(SaveableObject saveable in Saveables)
            if(!AlreadyExistingGameObjects.ContainsKey(saveable.InstanceID))
                AlreadyExistingGameObjects.Add(saveable.InstanceID, saveable);
    }

    public bool SaveFileAvailable(int index){
        string dirpath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}");
        return Directory.Exists(dirpath) && Directory.GetDirectories(dirpath).Length > 0;
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.L)){
            SaveAll(SaveFileIndex);
        }
        if (Input.GetKeyDown(KeyCode.K)){
            LoadAll(SaveFileIndex);
        }
    }

    public void SaveAll(int FileIndex){
        FileIndex = Mathf.Clamp(FileIndex, 0, 3);

        SaveableObject[] Saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<SaveableObject>().ToArray();

        UpdateGameObjectList();

        if (SaveFileAvailable(FileIndex))
            Directory.Delete(Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}"), true);

        foreach(SaveableObject Saveable in Saveables){
            SavedTransform SaveableTransform = new SavedTransform();

            SaveableTransform.Position = Saveable.gameObject.transform.position;
            SaveableTransform.Rotation = Saveable.gameObject.transform.rotation;
            SaveableTransform.Scale = Saveable.gameObject.transform.localScale;

            string Json = Saveable.Save();
            string SavedTransformJson = JsonUtility.ToJson(SaveableTransform);

            string path = Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}" , Saveable.ObjectType.EndsWith("s")? Saveable.ObjectType : Saveable.ObjectType + "s");
            string TransformPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}" , ".Transforms");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(TransformPath);

            File.WriteAllText(Path.Combine(path, $"SavedObject_{Saveable.InstanceID}_{Saveable.TypeID}.json"), Json);
            File.WriteAllText(Path.Combine(TransformPath, $"SavedTransform_{Saveable.InstanceID}_{Saveable.TypeID}.json"), SavedTransformJson);
        }

        Debug.Log($"Saved SaveFile: {FileIndex}");
    }

    public void LoadAll(int FileIndex){
        FileIndex = Mathf.Clamp(FileIndex, 0, 3);
        UIManager.CloseAllMenus();

        if (!SaveFileAvailable(FileIndex)){
            SaveAll(FileIndex);
            return;
        }

        UpdateGameObjectList();
        prefabTables.BuildGameObjectDictionary();

        LoadCustomSaves(FileIndex);
        LoadTransforms(FileIndex);

        Debug.Log($"Loaded SaveFile: {FileIndex}");
    }

    private void LoadCustomSaves(int Index){

        string path = Path.Combine(Application.persistentDataPath, $"SaveFile_{Index}");

        foreach (string dir in Directory.GetDirectories(path).Where(d => !Path.GetFileName(d).StartsWith("."))){
            foreach (string file in Directory.GetFiles(Path.Combine(path, dir))){
                string fileName = Path.GetFileNameWithoutExtension(file);
                // SavedObject_abc123_5

                string dataPart = fileName.Replace("SavedObject_", "");
                string[] parts = dataPart.Split('_');

                string InstanceID = parts[0];
                int TypeID = int.Parse(parts[1]);

                if (AlreadyExistingGameObjects.ContainsKey(InstanceID)){
                    AlreadyExistingGameObjects[InstanceID].Load(File.ReadAllText(file));
                }else{
                    if (prefabTables.GameObjectSaves.ContainsKey(TypeID)){
                        GameObject obj = Instantiate(prefabTables.GameObjectSaves[TypeID].gameObject, transform.position, Quaternion.identity);
                        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName("OverWorld"));

                        SaveableObject Saveable = obj.GetComponent<SaveableObject>();

                        if (Saveable != null){
                            Saveable.InstanceID = InstanceID;
                            Saveable.Load(File.ReadAllText(file));
                        }
                    }else{
                        Debug.Log($"No ID Detected for Object: InstanceID: {InstanceID}, TypeID: {TypeID}");
                    }
                }
            }
        }

        UpdateGameObjectList();

    }

    private void LoadTransforms(int Index){
        string TransformPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{Index}" , ".Transforms");

        foreach (string file in Directory.GetFiles(TransformPath)){
           string fileName = Path.GetFileNameWithoutExtension(file);
            // SavedTransform_abc123_5

            string dataPart = fileName.Replace("SavedTransform_", "");
            string[] parts = dataPart.Split('_');

            string InstanceID = parts[0];

            if (AlreadyExistingGameObjects.ContainsKey(InstanceID)){
                SavedTransform LoadedTransform = JsonUtility.FromJson<SavedTransform>(File.ReadAllText(file));

                AlreadyExistingGameObjects[InstanceID].gameObject.transform.position = LoadedTransform.Position;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.rotation = LoadedTransform.Rotation;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.localScale = LoadedTransform.Scale;
            }
        }
    }

    private void LoadSaveFileInformation(int Index){
        string TransformPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{Index}" , ".Transforms");

        foreach (string file in Directory.GetFiles(TransformPath)){
           string fileName = Path.GetFileNameWithoutExtension(file);
            // SavedTransform_abc123_5

            string dataPart = fileName.Replace("SavedTransform_", "");
            string[] parts = dataPart.Split('_');

            string InstanceID = parts[0];
            int TypeID = int.Parse(parts[1]);

            if (AlreadyExistingGameObjects.ContainsKey(InstanceID)){
                SavedTransform LoadedTransform = JsonUtility.FromJson<SavedTransform>(File.ReadAllText(file));

                AlreadyExistingGameObjects[InstanceID].gameObject.transform.position = LoadedTransform.Position;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.rotation = LoadedTransform.Rotation;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.localScale = LoadedTransform.Scale;
            }
        }
    }

    
    
    [System.Serializable]
    public class SavedTransform{
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }
}

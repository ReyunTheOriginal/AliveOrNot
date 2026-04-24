using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavingLoadingBase : MonoBehaviour
{   
    public Dictionary<string, SaveableObject> AlreadyExistingGameObjects = new Dictionary<string, SaveableObject>();
    public Dictionary<int, SavefileInfo> SavefileInformation = new Dictionary<int, SavefileInfo>();
    public PrefabTables prefabTables;
    public int SaveFileIndex;
    public float Timer;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }


    public void UpdateGameObjectList(bool IncludeInactive = true){
        AlreadyExistingGameObjects.Clear();

        SaveableObject[] Saveables = FindObjectsByType<MonoBehaviour>((IncludeInactive? FindObjectsInactive.Include : FindObjectsInactive.Exclude), FindObjectsSortMode.None).OfType<SaveableObject>().ToArray();

        foreach(SaveableObject saveable in Saveables)
            if(AlreadyExistingGameObjects != null && saveable != null && saveable.InstanceID != null && !AlreadyExistingGameObjects.ContainsKey(saveable.InstanceID))
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

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("OverWorld")){
            Timer += Time.deltaTime;
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
            string TransformPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{FileIndex}" , "Core_Transforms");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(TransformPath);

            File.WriteAllText(Path.Combine(path, $"SavedObject_{Saveable.InstanceID}_{Saveable.TypeID}.json"), Json);
            File.WriteAllText(Path.Combine(TransformPath, $"SavedTransform_{Saveable.InstanceID}_{Saveable.TypeID}.json"), SavedTransformJson);

            Debug.Log($"Saved: {Saveable.gameObject.name}, {Saveable.InstanceID}, {Saveable.TypeID}");
        }

        SaveSaveFileInformation(FileIndex);

        Debug.Log($"Saved SaveFile: {FileIndex}");
    }

    public void LoadAll(int FileIndex){
        FileIndex = Mathf.Clamp(FileIndex, 0, 3);

        UIManager.CloseAllMenus();

        if (!SaveFileAvailable(FileIndex)){
            GameServices.WorldGenerationBase.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            SaveAll(FileIndex);
            return;
        }

        UpdateGameObjectList();
        prefabTables.BuildGameObjectDictionary();

        LoadCustomSaves(FileIndex);
        LoadTransforms(FileIndex);
        LoadSaveFileInformation(FileIndex);

        Debug.Log($"Loaded SaveFile: {FileIndex}");
    }

    private void LoadCustomSaves(int Index){

        string path = Path.Combine(Application.persistentDataPath, $"SaveFile_{Index}");

        foreach (string dir in Directory.GetDirectories(path).Where(d => !Path.GetFileName(d).StartsWith("Core_"))){
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
                    if (prefabTables.GameObjectSaves.ContainsKey(TypeID) && !prefabTables.GameObjectSaves[TypeID].DoNotInstantiate){
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
        string TransformPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{Index}" , "Core_Transforms");

        foreach (string file in Directory.GetFiles(TransformPath)){
           string fileName = Path.GetFileNameWithoutExtension(file);
            // SavedTransform_abc123_5

            string dataPart = fileName.Replace("SavedTransform_", "");
            string[] parts = dataPart.Split('_');

            string InstanceID = parts[0];

            if (AlreadyExistingGameObjects.ContainsKey(InstanceID) && AlreadyExistingGameObjects[InstanceID].SaveTransform){
                SavedTransform LoadedTransform = JsonUtility.FromJson<SavedTransform>(File.ReadAllText(file));

                AlreadyExistingGameObjects[InstanceID].gameObject.transform.position = LoadedTransform.Position;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.rotation = LoadedTransform.Rotation;
                AlreadyExistingGameObjects[InstanceID].gameObject.transform.localScale = LoadedTransform.Scale;
            }
        }
    }


    private void SaveSaveFileInformation(int index){
        string MetaPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}");

        SavefileInfo info = new SavefileInfo();

        info.PlayTime = Timer;
        info.Seed = GameServices.WorldGenerationBase.Seed;

        string json = JsonUtility.ToJson(info);

        File.WriteAllText(Path.Combine(MetaPath, "Core_meta.json"), json);
    }

    public void LoadSaveFileInformation(int index){
        if (SaveFileAvailable(index)){
            string MetaPath = Path.Combine(Application.persistentDataPath, $"SaveFile_{index}" , "Core_meta.json");

            SavefileInfo info = new SavefileInfo();

            info = JsonUtility.FromJson<SavefileInfo>(File.ReadAllText(MetaPath));

            Timer = info.PlayTime;
            GameServices.GlobalTimer = info.PlayTime;

            GameServices.WorldGenerationBase.Seed = info.Seed;

            SavefileInformation[index] = info;
        }
    }

    
    
    [System.Serializable]
    public class SavedTransform{
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    [System.Serializable]
    public class SavefileInfo{
        public float PlayTime;
        public int Seed;
    }
}

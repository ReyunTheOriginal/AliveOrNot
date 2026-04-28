using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Chunk)), CanEditMultipleObjects]
public class ChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Chunk chunk = (Chunk)target;
        if (GUILayout.Button("Toggle")){
            if (chunk.gameObject.activeSelf){
                chunk.Deactivate();
            }else{
                chunk.Activate();
            }
        }
        DrawDefaultInspector();
    }
}
#endif

public class Chunk : BasicSave
{
    public override bool DoNotInstantiate => false;
    public override string ObjectType => "Chunk";
    public override bool SaveTransform => true;

    public override bool CanBeSave => Modified;
    public override bool SavedByParent => false;

    public override string Save(){
        SavedChunk Chunk = new SavedChunk();

        Chunk.AlwaysActive = AlwaysActive;
        Chunk.NeverActive = NeverActive;
        Chunk.ChunkPos = ChunkPos;
        Chunk.HasWater = HasWater;

        HashSet<SaveableObject> saveables = ObjectsInChunk
        .Where(obj => obj != null)
        .SelectMany(obj => obj.GetComponents<MonoBehaviour>())
        .OfType<SaveableObject>()
        .ToHashSet();

        foreach(SaveableObject saveable in saveables){
            if (saveable != null) saveable.SavedByParent = true;

            Saving.SavedGameObject saved = new Saving.SavedGameObject();
            saved.Position = saveable.gameObject.transform.position;
            saved.Rotation = saveable.gameObject.transform.rotation;
            saved.Scale = saveable.gameObject.transform.localScale;
            saved.name = saveable.gameObject.name;
            saved.CustonSave = saveable.Save();
            saved.TypeID = saveable.TypeID;
            saved.InstanceID = saveable.InstanceID;
            Chunk.ObjectsSavedInChunk.Add(JsonUtility.ToJson(saved));
        }

        TileMap.CompressBounds(); // tighten bounds to only filled area first

        foreach (Vector3Int pos in TileMap.cellBounds.allPositionsWithin){
            TileBase tile = TileMap.GetTile(pos);
            if (tile == null) continue;

            int ID = GameServices.GlobalVariables.Tiles.FirstOrDefault(kvp => kvp.Value == tile).Key;

            Chunk.TileIDs.Add(ID);
            Chunk.TilePositions.Add(pos);
        }

        foreach (WorldGenerationBase.WaterTile water in WaterTiles){
            Chunk.WaterDepth.Add(water.Depth);
            Chunk.WaterPositions.Add(water.Position);
        }

        return JsonUtility.ToJson(Chunk);
    }

    public override void Load(string json){
        GameUtils.StartIndependentCoroutine(() => _Load(json));
    }
    public IEnumerator _Load(string Json){
        SavedChunk Chunk = JsonUtility.FromJson<SavedChunk>(Json);

        ChunkPos = Chunk.ChunkPos;
        AlwaysActive = Chunk.AlwaysActive;
        NeverActive = Chunk.NeverActive;

        Modified = true;
        
        if (HasWater)
            WaterTilesScript = gameObject.AddComponent<WaterTiles>();

        foreach (string json in Chunk.ObjectsSavedInChunk){
            GameObject Loaded = Saving.LoadGameObject(json);
            if (Loaded){
                Loaded.transform.parent = transform.parent;
                ObjectsInChunk.Add(Loaded);
                Loaded.SetActive(false);
            }
        }

        if (GameServices.WorldGenerationBase)
            GameServices.WorldGenerationBase.AddChunkToSystem(this);
        
        Tile[] TilesToSet = new Tile[Chunk.TileIDs.Count];
        Vector3Int[] TilePositionsToSet = new Vector3Int[Chunk.TilePositions.Count];

        for (int i=0; i< Chunk.TileIDs.Count;i++){
            TilesToSet[i] = GameServices.GlobalVariables.Tiles[Chunk.TileIDs[i]];
            TilePositionsToSet[i] = Chunk.TilePositions[i];
        }

        for (int i=0; i< Chunk.WaterDepth.Count;i++){
            WorldGenerationBase.WaterTile WaterTile = new WorldGenerationBase.WaterTile();

            WaterTile.Depth    =  Chunk.WaterDepth[i];
            WaterTile.Position =  Chunk.WaterPositions[i];

            WaterTiles.Add(WaterTile);
        }

        TileMap.SetTiles(TilePositionsToSet, TilesToSet);

        yield return null;
    }

    [System.Serializable]
    public class SavedChunk{
        public Vector2Int ChunkPos;
        public bool AlwaysActive = false;
        public bool NeverActive = false;
        public List<string> ObjectsSavedInChunk = new List<string>();

        public List<int> TileIDs = new List<int>();
        public List<Vector3Int> TilePositions = new List<Vector3Int>();

        public List<int> WaterDepth = new List<int>();
        public List<Vector2Int> WaterPositions = new List<Vector2Int>();

        public bool HasWater = false;
    }

    /////////////////////////////////////////////////

    public Tilemap TileMap;
    public TilemapRenderer TilemapRenderer;
    public bool Modified = false;
    public Vector2Int ChunkPos;
    public WaterTiles WaterTilesScript;

    public bool HasWater = false;

    public bool _AlwaysActive = false;
    public bool _NeverActive = false;

    public bool AlwaysActive{get => _AlwaysActive; set {_AlwaysActive = value; if (value) _NeverActive = false;}}
    public bool NeverActive{get => _NeverActive; set {_NeverActive = value; if (value) _AlwaysActive = false;}}

    public HashSet<GameObject> ObjectsInChunk = new HashSet<GameObject>();
    public HashSet<WorldGenerationBase.WaterTile> WaterTiles = new HashSet<WorldGenerationBase.WaterTile>();

    private void Start() {
        if (!GameServices.WorldGenerationBase)return;

        GameServices.WorldGenerationBase.AddChunkToSystem(this);
        Deactivate();

        InstanceID = ChunkPos.ToString();
    }

    public void Activate(){
        if (GameServices.WorldGenerationBase && !NeverActive){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);

            if (!gameObject.activeSelf)gameObject.SetActive(true);
            
            foreach (GameObject obj in setSnapshot)
                if (obj && !obj.activeSelf)obj.SetActive(true);
        }
    }
    public void Deactivate(){
        if (GameServices.WorldGenerationBase && !AlwaysActive){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);
            foreach (GameObject obj in setSnapshot){
                if (obj && obj.activeSelf)obj.SetActive(false);
            }
            if (gameObject.activeSelf)gameObject.SetActive(false);
        }
    }
    public void Delete(bool force = false){
        if (GameServices.WorldGenerationBase && !Modified || force){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);

            foreach (GameObject obj in setSnapshot)
                if (obj)Destroy(obj);
            
            ObjectsInChunk.Clear();

            if (GameServices.WorldGenerationBase.ChunkDict.ContainsKey(ChunkPos))
                GameServices.WorldGenerationBase.ChunkDict.Remove(ChunkPos);

            Destroy(gameObject);
        }
    }
}

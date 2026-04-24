using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public Tilemap TileMap;
    public TilemapRenderer TilemapRenderer;
    public bool Modified = false;
    public Vector2Int ChunkPos;
    public WorldGenerationBase Parent;

    
    public bool AlwaysActive = false;
    public bool NeverActive = false;

    public HashSet<GameObject> ObjectsInChunk = new HashSet<GameObject>();
    public HashSet<WorldGenerationBase.WaterTile> WaterTiles = new HashSet<WorldGenerationBase.WaterTile>();

    public void Activate(){
        if (!NeverActive){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);

            if (!gameObject.activeSelf)gameObject.SetActive(true);
            
            foreach (GameObject obj in setSnapshot)
                if (!obj.activeSelf)obj.SetActive(true);
        }
    }
    public void Deactivate(){
        if (!AlwaysActive){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);
            foreach (GameObject obj in setSnapshot){
                if (obj.activeSelf)obj.SetActive(false);
            }
            if (gameObject.activeSelf)gameObject.SetActive(false);
        }
    }
    public void Delete(bool force = false){
        if (!Modified || force){
            HashSet<GameObject> setSnapshot = new HashSet<GameObject>(ObjectsInChunk);

            foreach (GameObject obj in setSnapshot)
                if (obj)Destroy(obj);
            
            ObjectsInChunk.Clear();

            if (Parent.ChunkDict.ContainsKey(ChunkPos))
                Parent.ChunkDict.Remove(ChunkPos);

            Destroy(gameObject);
        }
    }
}

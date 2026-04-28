using System.Collections.Generic;
using UnityEngine;

public class KeepInChunkList : MonoBehaviour
{
    public Vector2Int ChunkPos;
    public Vector2Int LastChunkPos;

    public bool active = true;

    void Update(){
        if (active && GameServices.WorldGenerationBase){
            LastChunkPos = ChunkPos;
            HandleListTransfering();
        }
    }

    public void HandleListTransfering(){
        Dictionary<Vector2Int, Chunk> dict = GameServices.WorldGenerationBase.ChunkDict;
        ChunkPos = GameUtils.GetChunkPos(transform.position);

        if (dict.TryGetValue(LastChunkPos, out Chunk lastChunk) && lastChunk && lastChunk.ObjectsInChunk != null)
            lastChunk.ObjectsInChunk.Remove(gameObject);

        if (dict.TryGetValue(ChunkPos, out Chunk newChunk) && lastChunk && lastChunk.ObjectsInChunk != null)
            newChunk.ObjectsInChunk.Add(gameObject);
    }

    public void Disable() {
        active = false;
        if (GameServices.WorldGenerationBase.ChunkDict.TryGetValue(LastChunkPos, out Chunk lastChunk))
            lastChunk.ObjectsInChunk.Remove(gameObject);

        if (GameServices.WorldGenerationBase.ChunkDict.TryGetValue(ChunkPos, out Chunk Chunk))
            Chunk.ObjectsInChunk.Remove(gameObject);
    }

    public void Enable() {
        active = true;
    }
}

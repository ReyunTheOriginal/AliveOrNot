using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRendering : MonoBehaviour
{
    public WorldGenerationBase Base;
    public int RenderDistance;
    private WorldGenerationBase.Chunk CurrentChunk;
    private WorldGenerationBase.Chunk LastChunk;
    private HashSet<Vector2Int> LastFrameRenderedChunks = new HashSet<Vector2Int>();
    private  HashSet<Vector2Int> RenderRadius = new HashSet<Vector2Int>();
    public Vector2Int ChunkPos;

    void Start(){
        
    }

    void Update(){
        RenderRadius.Clear();
        ChunkPos = Base.GetChunkPos(transform.position);
        
        for (int x=-RenderDistance;x<=RenderDistance;x++){
            for (int y=-RenderDistance;y<=RenderDistance;y++){
                Vector2Int pos = ChunkPos + new Vector2Int(x,y);
                RenderRadius.Add(pos);
                if (!Base.ChunkDict.ContainsKey(pos))
                    StartCoroutine(Base.GenerateChunk(pos));
            }
        }

        //if (Base.ChunkDict.ContainsKey(ChunkPos))
        //    CurrentChunk = Base.ChunkDict[ChunkPos];

        //if (CurrentChunk != LastChunk){
        //    LastChunk = CurrentChunk;
            HandleRendering();
            HandleDeleting();
        //}
    }

    public void HandleRendering(){
        HashSet<Vector2Int> ActivatedRadius = new HashSet<Vector2Int>();
        HashSet<Vector2Int> DeactivatedRadius = new HashSet<Vector2Int>();

        foreach (Vector2Int pos in RenderRadius){
            ActivatedRadius.Add(pos);
        }

        foreach (Vector2Int pos in LastFrameRenderedChunks){
            DeactivatedRadius.Add(pos);
        }

        foreach (Vector2Int pos in ActivatedRadius){
            if (DeactivatedRadius.Contains(pos))DeactivatedRadius.Remove(pos);

            if (Base.ChunkDict.ContainsKey(pos) && Base.ChunkDict[pos] != null)
                Base.ChunkDict[pos].Activate();
        }

        foreach (Vector2Int pos in DeactivatedRadius){
            if (Base.ChunkDict.ContainsKey(pos) && Base.ChunkDict[pos] != null)
                Base.ChunkDict[pos].Deactivate();
        }

        LastFrameRenderedChunks = ActivatedRadius;
    }
    public void HandleDeleting(){
        int deleteDistance = RenderDistance + 5;

        HashSet<Vector2Int> nonDeleteRadius = new HashSet<Vector2Int>();
        for (int x = -deleteDistance; x <= deleteDistance; x++)
            for (int y = -deleteDistance; y <= deleteDistance; y++)
                nonDeleteRadius.Add(ChunkPos + new Vector2Int(x, y));

        List<WorldGenerationBase.Chunk> toDelete = new List<WorldGenerationBase.Chunk>();
        foreach (var entry in Base.ChunkDict)
            if (entry.Value != null && !nonDeleteRadius.Contains(entry.Key))
                toDelete.Add(entry.Value);

        foreach (var chunk in toDelete)
            chunk.Delete();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DecorGeneration : MonoBehaviour
{

    private void Update() {
        
    }

    
    public void DecorateChunk(WorldGenerationBase.Chunk chunk, WorldGenerationBase.ValueCache Cache){
        for (int i = 0; i < Cache.Positions.Length; i++){
            Vector2Int worldPos = (Vector2Int)Cache.Positions[i] + Vector2Int.RoundToInt(chunk.ChunkPos * GameServices.WorldGenerationBase.ChunkSize);
            float roll = GameUtils.GetDeterministicRandom(worldPos, GameServices.WorldGenerationBase.Seed);

            float cumulative = 0f;
            foreach (WorldGenerationBase.Decor Decor in Cache.Biomes[i].Decor){
                cumulative += Decor.Frequancy;
                if (roll < cumulative){
                    Vector2 Pos = (Vector2)(Vector3)Cache.Positions[i] + (chunk.ChunkPos * GameServices.WorldGenerationBase.ChunkSize) + new Vector2(0.5f, 0.5f);
                    Instantiate(Decor.Object, Pos, Quaternion.identity, chunk.WorldObject.transform);
                    break;
                }
            }
        }
    }
}

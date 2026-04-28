using UnityEngine;


public class DecorGeneration : MonoBehaviour
{
    public void DecorateChunk(Chunk chunk, WorldGenerationBase.ValueCache Cache){
        for (int i = 0; i < Cache.Positions.Length; i++){
            Vector2Int worldPos = (Vector2Int)Cache.Positions[i] + Vector2Int.RoundToInt(chunk.ChunkPos * GameServices.WorldGenerationBase.ChunkSize);
            float roll = GameUtils.GetDeterministicRandom(worldPos, GameServices.WorldGenerationBase.Seed);

            float cumulative = 0f;
            foreach (WorldGenerationBase.Decor Decor in Cache.Biomes[i].Decor){
                cumulative += Decor.Frequancy;
                if (roll < cumulative){
                    Vector2 Pos = (Vector2)(Vector3)Cache.Positions[i] + (chunk.ChunkPos * GameServices.WorldGenerationBase.ChunkSize) + new Vector2(0.5f, 0.5f);
                    GameObject newObject = Instantiate(Decor.Object, Pos, Quaternion.identity, GameServices.WorldGenerationBase.Grid.transform);
                    newObject.name = $"{Decor.Object.name}_{Pos}";
                    chunk.ObjectsInChunk.Add(newObject);
                    break;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerationBase : MonoBehaviour
{
    public int Seed = 69420; //Seed for Deterministic Generation
    public Dictionary<Vector2Int, Chunk> ChunkDict = new Dictionary<Vector2Int, Chunk>(); //a list of all Chunks
    public int ChunkResolution = 64; // the Resolution of Chunks (64x64) tiles
    public Grid Grid; // the grid the chunks are children of
    public Transform DebugParent; // the object the DebugOverlays are children of
    public int ChunkSize = 16; //the size of Chunks
    public bool Debug = false; //if it should generate Debug stuff
     public List<Biome> Biomes; //Biomes it can Generate
    public PerlinConfig ElevationConfig; 
    public PerlinConfig TemperatureConfig;
    public PerlinConfig HumidityConfig;
    [Header("Debug")]
    public float ResolutionMultiplier;

    void Awake(){
        GameServices.WorldGenerationBase = this;
        //calculate the Resolution
        ResolutionMultiplier = (float)ChunkSize / (float)ChunkResolution;
        Grid.cellSize = new Vector3(ResolutionMultiplier, ResolutionMultiplier, 0);
    }
    
    //Get what Chunk any position is in
    public Vector2Int GetChunkPos(Vector2 pos){
        int chunkX = Mathf.FloorToInt(pos.x / (ChunkSize * ResolutionMultiplier));
        int chunkY = Mathf.FloorToInt(pos.y / (ChunkSize * ResolutionMultiplier));

        return new Vector2Int(chunkX, chunkY);
    }

    //Generate Chunks
    public IEnumerator GenerateChunk(Vector2Int ChunkPos){
        //check if the chunk already exists
        if (ChunkDict.ContainsKey(ChunkPos) == false){
            Chunk NewChunk = new Chunk(); //new Chunk
            ChunkDict.Add(ChunkPos, null); //cache it's position

            // snapshot everything the thread will need — no Unity objects
            List<Biome> biomesSnapshot = new List<Biome>(Biomes);
            Biome fallback = Biomes[0];
            int chunkSize = ChunkSize;
            int seed = Seed;
            PerlinConfig elevCfg = ElevationConfig;
            PerlinConfig tempCfg = TemperatureConfig;
            PerlinConfig humCfg = HumidityConfig;

            // --- THREAD ---
            ValueCache Cache = new ValueCache(chunkSize * chunkSize);

            bool done = false;

            System.Threading.Tasks.Task.Run(() => {
                Vector2 ElevOffset = elevCfg.Offsit(Seed);
                Vector2 TempOffset = tempCfg.Offsit(Seed);
                Vector2 HumiOffset = humCfg.Offsit(Seed);

                for (int x = 0; x < chunkSize; x++){
                    for (int y = 0; y < chunkSize; y++){
                        float Elevation = GetPerlin(ChunkPos, new Vector2Int(x, y), elevCfg, ElevOffset);
                        float Temperature = GetPerlin(ChunkPos, new Vector2Int(x, y), tempCfg, TempOffset);
                        float Humidity = GetPerlin(ChunkPos, new Vector2Int(x, y), humCfg, HumiOffset);

                        Biome matched = fallback;
                        for (int i = 0; i < biomesSnapshot.Count; i++){
                            if (biomesSnapshot[i].Matches(Elevation, Temperature, Humidity)){
                                matched = biomesSnapshot[i];
                                break;
                            }
                        }

                        int l = x * chunkSize + y;
                        Cache.Elevation[l]   = Elevation;
                        Cache.Temperature[l] = Temperature;
                        Cache.Humidity[l]    = Humidity;
                        Cache.Positions[l]   = new Vector3Int(x, y);
                        Cache.Biomes[l]      = matched;
                        Cache.Tiles[l]     = matched.Block;
                    }
                }
                done = true;
            });

            yield return new WaitUntil(() => done);
            // --- BACK ON MAIN THREAD ---

            //Configure Chunk Object
            GameObject NewWorldObject = new GameObject(); //create it
            NewWorldObject.name = $"Chunk[{ChunkPos.x}, {ChunkPos.y}]"; //name it: "Chunk[0, 0]"
            NewWorldObject.transform.parent = Grid.transform; //set its parent to the Grid
            NewWorldObject.transform.position = (Vector2)ChunkPos * ChunkSize * ResolutionMultiplier; // set its position
            Tilemap TileMap = NewWorldObject.AddComponent<Tilemap>(); //add a tilemap
            TilemapRenderer Ren = NewWorldObject.AddComponent<TilemapRenderer>(); //render the tilemap
            Ren.sortingLayerName = "Ground"; //add it to the Ground Render Layer
            Ren.material = GameServices.GlobalVariables.SpriteLitDefault; // game it work with lighting

            // set the tiles of the chunk all at once 
            TileMap.SetTiles(Cache.Positions, Cache.Tiles); 

            //apply Configured stuff to the Chunk
            NewChunk.WorldObject = NewWorldObject;
            NewChunk.ChunkPos = ChunkPos;
            NewChunk.TileMap = TileMap;
            NewChunk.TilemapRenderer = Ren;
            NewChunk.Parent = this;

            // now fill the real chunk in
            ChunkDict[ChunkPos] = NewChunk;
            NewChunk.Deactivate(); 

            //Make a Debug Overlay
            if (Debug){
                DebugOverlay(Cache.Elevation, Cache.Positions, Color.white, ChunkPos, "ElevationDebug");
                DebugOverlay(Cache.Temperature, Cache.Positions, Color.red, ChunkPos, "TemperatureDebug");
                DebugOverlay(Cache.Humidity, Cache.Positions, Color.blue, ChunkPos, "HumidityDebug");
            }
        }
        yield return null;
    }

    public void DebugOverlay(float[] Perlins, Vector3Int[] Positions, Color color, Vector2Int ChunkPos, string Name = "DebugOverlay"){
        //Create a new Texture
        Texture2D Tex = new Texture2D(ChunkSize, ChunkSize);

        //Create a new Overlay Object
        GameObject NewOverlay = new GameObject();

        //Configure it
        NewOverlay.name = $"{Name}[{ChunkPos.x}, {ChunkPos.y}]";
        NewOverlay.transform.position = (Vector2)ChunkPos * ChunkSize * ResolutionMultiplier;
        NewOverlay.transform.parent = DebugParent;
        SpriteRenderer ren = NewOverlay.AddComponent<SpriteRenderer>();
        ren.sortingLayerName = "Overlay";

        //set the pixels
        for (int i=0;i<Perlins.Length;i++){
            Color PlaceColor = color;
            PlaceColor.a = Perlins[i] * 0.2f;

            Tex.SetPixel(Positions[i].x,Positions[i].y,PlaceColor);
        }

        //apply it and make a sprite
        Tex.Apply();
        Sprite sprite = Sprite.Create(
            Tex,
            new Rect(0, 0, ChunkSize, ChunkSize),
            new Vector2(0f, 0f), 
            1f / ResolutionMultiplier                 
        );
        //set the sprite
        ren.sprite = sprite;
    }


    //calculate Perlin noise
    public float GetPerlin(Vector2Int ChunkPos, Vector2Int TilePos, PerlinConfig config, Vector2 Offset){
        float WorldX = ChunkPos.x * ChunkSize + TilePos.x;
        float WorldY = ChunkPos.y * ChunkSize + TilePos.y;

        float value     = 0f;
        float amplitude = 1f;
        float max       = 0f;
        float frequency = config.scale;

        for (int i = 0; i < config.Octaves; i++){
            float nx = (WorldX + Offset.x) * frequency;
            float ny = (WorldY + Offset.y) * frequency;
            value     += Mathf.PerlinNoise(nx, ny) * amplitude;
            max       += amplitude;
            amplitude *= config.Persistence;
            frequency *= 2f;
        }

        return Mathf.Clamp01(value / max);
    }

    //Add Decorations to a chunk
    public void DecorateChunk(Chunk chunk){

    }

    [System.Serializable]
    public class Chunk{
        public GameObject WorldObject;
        public Tilemap TileMap;
        public TilemapRenderer TilemapRenderer;
        public bool Modified = false;
        public Vector2Int ChunkPos;
        public WorldGenerationBase Parent;

        
        public bool AlwaysActive = false;
        public bool NeverActive = false;

        public HashSet<GameObject> ObjectsInChunk = new HashSet<GameObject>();

        public void Activate(){
            if (!NeverActive){
                if (!WorldObject.activeSelf)WorldObject.SetActive(true);
                foreach (GameObject obj in ObjectsInChunk){
                    if (!obj.activeSelf)obj.SetActive(true);
                }
            }
        }
        public void Deactivate(){
            if (!AlwaysActive){
                foreach (GameObject obj in ObjectsInChunk){
                    if (obj.activeSelf)obj.SetActive(false);
                }
                if (WorldObject.activeSelf)WorldObject.SetActive(false);
            }
        }
        public void Delete(bool force = false){
            if (!Modified || force){
                foreach (GameObject obj in ObjectsInChunk){
                    if (obj)Destroy(obj);
                }
                ObjectsInChunk.Clear();

                if (Parent.ChunkDict.ContainsKey(ChunkPos))
                    Parent.ChunkDict.Remove(ChunkPos);

                Destroy(WorldObject);
            }
        }
    }
    [System.Serializable]
    public class ValueCache{
        public Vector3Int[] Positions;
        public Biome[]      Biomes;
        public TileBase[]   Tiles;
        public float[]      Elevation;
        public float[]      Temperature;
        public float[]      Humidity;

        public ValueCache(int size){
            Positions   = new Vector3Int[size];
            Biomes      = new Biome[size];
            Tiles       = new TileBase[size];
            Elevation   = new float[size];
            Temperature = new float[size];
            Humidity    = new float[size];
        }
    }
    [System.Serializable]
    public class PerlinConfig{
       public float scale;
        public int Octaves = 4;
        [Range(0f, 1f)]
        public float Persistence = 0.5f;

        public Vector2 OffsitMultiplier;
        public Vector2 Offsit(int seed){
            return new Vector2(
                Mathf.Sin(seed) * OffsitMultiplier.x * 1000f,
                Mathf.Cos(seed) * OffsitMultiplier.y * 1000f
            );
        }
    }
    [System.Serializable]
    public class Biome{
        public string Name;
        public Tile Block;
        public List<Decor> Decor;
        public Color color;

        public Range Elevation;
        public Range Temperature;
        public Range Humidity;

        public bool Matches(float elevation, float temperature, float humidity) {
            return Elevation.Contains(elevation) &&
                Temperature.Contains(temperature) &&
                Humidity.Contains(humidity);
        }
    }
    [System.Serializable]
    public class Range{
        public float Min;
        public float Max;
        public bool Contains(float value) => value >= Min && value <= Max;
    }
    [System.Serializable]
    public class Decor{
        [Range(0f, 1f)]
        public float Frequancy;
        public GameObject Object;
    }
}

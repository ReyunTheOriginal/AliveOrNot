using UnityEngine;
using UnityEngine.Tilemaps;

public class SpriteSorter : MonoBehaviour
{
    static int AntiZFighting = 0;
    private int CurrentAntiZFighting;
    public float Offsit;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private TilemapRenderer TileRenderer;
    [SerializeField] private int OrderOffsit;

    private void Awake() {
        CurrentAntiZFighting = AntiZFighting;
        AntiZFighting++;
    }

    void OnValidate(){
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

        if (TileRenderer == null)
            TileRenderer = GetComponent<TilemapRenderer>();
    }

    void Update()
    {
        int order = (int)(-(transform.position.y + Offsit) * 100) + OrderOffsit;// + CurrentAntiZFighting;

        if (SpriteRenderer)SpriteRenderer.sortingOrder = order;
        if (TileRenderer)TileRenderer.sortingOrder = order;
    }
}

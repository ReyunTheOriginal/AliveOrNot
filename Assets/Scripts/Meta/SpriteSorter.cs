using UnityEngine;
using UnityEngine.Tilemaps;

public class SpriteSorter : MonoBehaviour
{
    public float YOffsit;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private TilemapRenderer TileRenderer;
    [SerializeField] private int OrderOffsit;
    private Vector3 lastPos;
    public SpriteSorter AlwaysOnTopOf;

    void OnValidate(){
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

        if (TileRenderer == null)
            TileRenderer = GetComponent<TilemapRenderer>();
    }

    void Update()
    {
        Vector3 pos = transform.position + new Vector3(0, YOffsit, 0);
        if (pos != lastPos){
            UpdateSorting(pos);
            lastPos = transform.position;
        }
    }

    private void UpdateSorting(Vector3 pos){
        int order;
        
        if (!AlwaysOnTopOf)
            order = Mathf.RoundToInt(-(pos.y + YOffsit) * 100) + OrderOffsit;
        else
            order = AlwaysOnTopOf.SpriteRenderer.sortingOrder + 1;

        if (SpriteRenderer)SpriteRenderer.sortingOrder = order;
        if (TileRenderer)TileRenderer.sortingOrder = order;
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SpriteSorter)), CanEditMultipleObjects]
public class SpriteSorterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpriteSorter Sorter = (SpriteSorter)target;
        if (GUILayout.Button("set to be under its parent")){
            Sorter.AlwaysUnder = Sorter.transform.parent.GetComponent<SpriteSorter>();
        }
        DrawDefaultInspector();
    }
}
#endif

public class SpriteSorter : MonoBehaviour
{
    public float YOffsit;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private TilemapRenderer TileRenderer;
    [SerializeField] private ParticleSystem ParticleSystem;
    [SerializeField] private ParticleSystemRenderer ParticleSystemRenderer;
    [SerializeField] private int OrderOffsit;
    private Vector3 lastPos;
    public SpriteSorter AlwaysOnTopOf;
    public SpriteSorter AlwaysUnder;
    private int order;

    #if UNITY_EDITOR
    void OnValidate(){
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

        if (ParticleSystem == null)
            ParticleSystem = GetComponent<ParticleSystem>();

        if (ParticleSystem && ParticleSystemRenderer == null)
            ParticleSystemRenderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();

        if (TileRenderer == null)
            TileRenderer = GetComponent<TilemapRenderer>();
    }
    #endif

    private void Awake() {
        if (AlwaysOnTopOf)return;

        Vector3 pos = transform.position + new Vector3(0, YOffsit, 0);
        if (pos != lastPos){
            UpdateSorting(pos);
            lastPos = transform.position;
        }
    }

    private void Start() {
        if (AlwaysOnTopOf)return;

        Vector3 pos = transform.position + new Vector3(0, YOffsit, 0);
        if (pos != lastPos){
            UpdateSorting(pos);
            lastPos = transform.position;
        }
    }

    void Update(){
        if (AlwaysOnTopOf)return;

        Vector3 pos = transform.position + new Vector3(0, YOffsit, 0);
        if (pos != lastPos){
            UpdateSorting(pos);
            lastPos = transform.position;
        }
    }

    private void LateUpdate() {
        if (!AlwaysOnTopOf)return;
        
        Vector3 pos = transform.position + new Vector3(0, YOffsit, 0);
        if (pos != lastPos){
            UpdateSorting(pos);
            lastPos = transform.position;
        }
    }

    private void UpdateSorting(Vector3 pos){
        if (!AlwaysOnTopOf && !AlwaysUnder){
            order = Mathf.RoundToInt(-(pos.y + YOffsit) * 100) + OrderOffsit;

            if (SpriteRenderer)SpriteRenderer.sortingOrder = order;
            if (TileRenderer)TileRenderer.sortingOrder = order;
            if (ParticleSystemRenderer)ParticleSystemRenderer.sortingOrder = order;
        }else{
            if (AlwaysOnTopOf){
                order = AlwaysOnTopOf.SpriteRenderer.sortingOrder + 1;

                if (SpriteRenderer)SpriteRenderer.sortingOrder = order;
                if (TileRenderer)TileRenderer.sortingOrder = order;
                if (ParticleSystemRenderer)ParticleSystemRenderer.sortingOrder = order;
            }
            if (AlwaysUnder){
                order = AlwaysUnder.SpriteRenderer.sortingOrder - 1;

                if (SpriteRenderer)SpriteRenderer.sortingOrder = order;
                if (TileRenderer)TileRenderer.sortingOrder = order;
                if (ParticleSystemRenderer)ParticleSystemRenderer.sortingOrder = order;
            }
        }
    }
}

using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    static int AntiZFighting = 0;
    private int CurrentAntiZFighting;
    public float Offsit;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private int OrderOffsit;

    private void Awake() {
        CurrentAntiZFighting = AntiZFighting;
        AntiZFighting++;
    }

    void OnValidate(){
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        SpriteRenderer.sortingOrder = (int)(-(transform.position.y + Offsit) * 100) + OrderOffsit + CurrentAntiZFighting;
    }
}

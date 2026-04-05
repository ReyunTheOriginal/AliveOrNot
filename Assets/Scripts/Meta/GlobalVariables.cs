using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public GameObject Player;
    public Hand PrimaryHandObject;
    public Hand OffHandObject;
    public Camera Camera;
    public Material SpriteLitDefault;

    private void Awake() {
        GameServices.GlobalVariables = this;
    }


    [System.Serializable]
    public class Hand{
        public GameObject Center;
        public GameObject MuzzleFlashLocation;
        public AudioSource AudioSource;
        public GameObject Object;
        public GameObject RightHand;
        public GameObject LeftHand;
        public SpriteRenderer ObjectRenderer;
    }
}

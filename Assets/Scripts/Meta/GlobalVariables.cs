using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public PlayerData Player;
    public HandData PrimaryHandObject;
    public HandData OffHandObject;
    public Camera Camera;
    public CameraScript cameraScript;
    public Material SpriteLitDefault;
    public CursorTextures Cursors;
    public HashSet<EnemyProperties> AllEnemies = new HashSet<EnemyProperties>();
    public HashSet<ItemProperties> AllItems = new HashSet<ItemProperties>();
    public AmmoData AmmoUI;

    private void Awake() {
        GameServices.GlobalVariables = this;
    }

    [System.Serializable]
    public class PlayerData{
        public GameObject GameObject;
        public HealthSystem PlayerHealth;
        public Rigidbody2D rig;
        public EquiptmentScript Equiptment;
        public Animator Animator;
        public SpriteRenderer SpriteRenderer;
        public PlayerMovement MovementScript;
        public SpriteMask SpriteMask;
    }

    [System.Serializable]
    public class AmmoData{
        public CanvasGroup WholeUI;
        public RectTransform Rect;
        public TMP_Text Text;
    }

    [System.Serializable]
    public class HandData{
        public GameObject Center;
        public GameObject MuzzleFlashLocation;
        public AudioSource AudioSource;
        public Collider2D Collider;
        public AnimationPlayer AnimationPlayer;
        public GameObject Object;
        public GameObject RightHand;
        public GameObject LeftHand;
        public SpriteRenderer ObjectRenderer;
    }

    [System.Serializable]
    public class CursorTextures{
        public Texture2D Default;
        //public Texture2D Gun;
    }
}

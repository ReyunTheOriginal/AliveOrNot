using System.Collections.Generic;
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
    public CanvasGroup CustomItemUICanvasGroup;
    public List<ChunkRendering> ChunkRenderers;

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
    public class HandData{
        public GameObject CenterObject;
        public GameObject RightHand;
        public SpriteSorter RightHandSorter;
        public SpriteSorter RightHandPropSorter;
        /////////////////
        public GameObject LeftHand;
        public SpriteSorter LeftHandSorter;
        public SpriteSorter LeftHandPropSorter;
    }

    [System.Serializable]
    public class CursorTextures{
        public Texture2D Default;
        //public Texture2D Gun;
    }
}

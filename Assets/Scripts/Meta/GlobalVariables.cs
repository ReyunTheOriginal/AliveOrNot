using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public PlayerData Player;
    public HandData PrimaryHandObject;
    public HandData OffHandObject;
    public Camera Camera;
    public Material SpriteLitDefault;
    public HashSet<EnemyProperties> AllEnemies = new HashSet<EnemyProperties>();
    public HashSet<ItemProperties> AllItems = new HashSet<ItemProperties>();

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
}

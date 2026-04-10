using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProperties : MonoBehaviour{

    [SerializeField]private float MaxHealth;
    [SerializeField]private float Health;
    public float Damage;
    public float RandomDamageOffsit;

    [SerializeField]private float LightAttackCooldown;
    [SerializeField]private float HeavyAttackCooldown;

    [SerializeField]private float SpecialAttackCooldown;

    public float KnockBack;
    public float StunLength;

    public float WalkSpeed;

    [SerializeField]private GameObject HealthBarPrefab;

    [SerializeField]private float HealthBarHeight;
    [SerializeField]private List<Drop> Drops;

    private HealthBar healthBar = new HealthBar(); 


    public Dictionary<Stat, bool> CurrentStats = new Dictionary<Stat, bool>{
        {Stat.Chasing, false},
        {Stat.Wandering, false},
        {Stat.Idle, false},
        {Stat.Attacking, false},
        {Stat.Spawning, false},
        {Stat.Stunned, false},
    };
    [Header("Componenets")]
    public Rigidbody2D rig;
    [SerializeField]private EnemyBehavior behavior;

    private void Awake() {
        if (behavior)behavior.Properties = this;
    }

    private void Start(){
        behavior.AtSpawn();
        GameServices.GlobalVariables.AllEnemies.Add(this);

        GameObject NewHealthBar = Instantiate(HealthBarPrefab, GameServices.UI.WorldCanvas.transform);
        healthBar.FullUI = NewHealthBar.GetComponent<RectTransform>();
        healthBar.Bar = NewHealthBar.transform.GetChild(0).GetComponent<Image>();
        NewHealthBar.SetActive(false);

    }

    private void Update() {
        if (behavior){
            if (CurrentStats[Stat.Chasing])
                behavior.Chasing();
             if (CurrentStats[Stat.Wandering])
                behavior.Wandering();
             if (CurrentStats[Stat.Idle])
                behavior.Idling();
        }

        healthBar.Bar.fillAmount = Health/MaxHealth;
        healthBar.FullUI.position = gameObject.transform.position + new Vector3(0,HealthBarHeight,0);
    }

    private void OnDestroy() {
        if (healthBar.FullUI)Destroy(healthBar.FullUI.gameObject);
    }

    public void HitEnemy(float DamageDealt, Vector2 KnockBack){
        if (DamageDealt > 0){
            if (behavior) behavior.OnHit();
            ChangeHealth(-DamageDealt);

            rig.AddForce(KnockBack, ForceMode2D.Impulse);

            if (Health <= 0){
                GameServices.GlobalVariables.AllEnemies.Remove(this);
                DropItems();
                behavior.OnDeath();
            }
        }
    }

    private void DropItems(){
        foreach (Drop chance in Drops){
            float num = UnityEngine.Random.Range(0f, 1f);
            if (chance.Matches(num)){
                Instantiate(chance.Object, transform.position, Quaternion.identity);
            }
        }
    }

    public void ChangeHealth(float Amount){
        Health += Amount;
        if (Amount < 0){
            if (healthBar.FullUI.gameObject.activeSelf == false) healthBar.FullUI.gameObject.SetActive(true);
        }else{
            if (healthBar.FullUI.gameObject.activeSelf) healthBar.FullUI.gameObject.SetActive(false);
        }
    }

    [System.Serializable]
    public class Drop{
        [Range(0f, 1f)]
        public float Frequancy;
        public GameObject Object;

        public bool Matches(float num){
            return num <= Frequancy;
        }
    }

    public enum Stat{
        Chasing,
        Wandering,
        Idle,
        Attacking,
        Spawning,
        Stunned,
    }
    public enum Direction{
        Up,
        Down,
        Left,
        Right
    }
    public class HealthBar{
        public RectTransform FullUI;
        public Image Bar;
    }
}

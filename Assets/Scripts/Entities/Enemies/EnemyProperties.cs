using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProperties : MonoBehaviour{

    public float MaxHealth;
    public float Health;
    public float Damage;
    public float RandomDamageOffsit;

    public float LightAttackCooldown;
    public float HeavyAttackCooldown;

    public float SpecialAttackCooldown;

    public float WalkSpeed;

    public GameObject HealthBarPrefab;

    public float HealthBarHeight;

    private HealthBar healthBar = new HealthBar(); 


    public Stat CurrentStat;
    [Header("Componenets")]
    public Rigidbody2D rig;
    public EnemyBehavior behavior;

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

        CurrentStat = Stat.Spawning;
    }

    private void Update() {
        if (behavior){
            switch (CurrentStat){
                case Stat.Chasing:
                    behavior.Chasing();
                    break;
                case Stat.Wandering:
                    behavior.Wandering();
                    break;
                case Stat.Idle:
                    behavior.Idling();
                    break;
            }
        }

        healthBar.Bar.fillAmount = Health/MaxHealth;
        healthBar.FullUI.position = gameObject.transform.position + new Vector3(0,HealthBarHeight,0);
    }

    private void OnDestroy() {
        Destroy(healthBar.FullUI.gameObject);
    }

    public void HitEnemy(float DamageDealt){
        if (DamageDealt > 0){
            if (behavior) behavior.OnHit();
            ChangeHealth(-DamageDealt);

            if (Health <= 0){
                GameServices.GlobalVariables.AllEnemies.Remove(this);
                behavior.OnDeath();
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

    public enum Stat{
        Chasing,
        Wandering,
        Idle,
        Attacking,
        Spawning,
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

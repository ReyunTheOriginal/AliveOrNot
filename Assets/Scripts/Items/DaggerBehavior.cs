using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerBehavior : ItemBehavior
{
    public WeaponPropertiesHolder WeaponProperties;
    [Header("Settings")]
    public bool Automatic;
    public float Range;
    public float StunLength;
    public float Recoil;

    public AudioClip SlashAudio;
    public AnimationClip SlashAnimation;

    [Header("Debug")]
    public bool DebugMode;
    public float Timer;
    public HashSet<EnemyProperties> HitEnemies = new HashSet<EnemyProperties>(); 
    public bool Hitting = false;
    public Vector2 HitDir;

    private void Start() {
        Properties.Damage = WeaponProperties.Damage;
        Properties.AttackSpeed = 1.0f / WeaponProperties.Cooldown;
    }

    private void OnEnable() {
        if (Properties)Properties.Damage = WeaponProperties.Damage;
        if (Properties)Properties.AttackSpeed = 1.0f / WeaponProperties.Cooldown;
    }

    public override void Equipped(){
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = false;
        GameServices.GlobalVariables.OffHandObject.LeftHand.SetActive(false);
    }

    public override void UnEquipped(){
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = true;
    }

    public override void LateHold(){
        if (!GameServices.UI.AMenuIsOpened())
            if (!Hitting)
                GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.Center.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);
    }

    public override void Hold(){
        if (!GameServices.UI.AMenuIsOpened()){
            Timer += Time.deltaTime;

            if (Timer >= WeaponProperties.Cooldown && Input.GetMouseButtonDown(1)){
                PlayAudio();
                HitCheck();
                Timer = 0;
                HitEnemies.Clear();

                Vector2 dir = GameUtils.DirFromAToB(GameServices.GlobalVariables.OffHandObject.Center.transform.position, GameUtils.QuickWorldMousePosition());
                GameServices.GlobalVariables.Player.rig.AddForce(Recoil * -dir, ForceMode2D.Impulse);

                GameServices.GlobalVariables.OffHandObject.AnimationPlayer.RunAfterAnimationIsOver = AfterHit;
                GameServices.GlobalVariables.OffHandObject.AnimationPlayer.PlayClip(SlashAnimation);
                
            }

            Vector2 pos = GameServices.GlobalVariables.PrimaryHandObject.Object.transform.position;

            if (Hitting){
                HashSet<Collider2D> hits = GameUtils.SquareHitDetection(GameServices.GlobalVariables.PrimaryHandObject.Center.transform.position, 
                    HitDir, 
                    Range, 
                    5, 
                    0.35f, 
                    "Enemy", 
                    GameUtils.LayerMaskFromNumbers(-4), true
                );

                List<Collider2D> sorted = new List<Collider2D>(hits);

                if (WeaponProperties.MaxHits > 1)
                    sorted.Sort((a, b) =>{
                        float da = ((Vector2)a.ClosestPoint(pos) - pos).sqrMagnitude;
                        float db = ((Vector2)b.ClosestPoint(pos) - pos).sqrMagnitude;
                        return da.CompareTo(db);
                    });

                if (WeaponProperties.MaxHits == 0){
                    foreach(Collider2D col in sorted){
                        Hit(col, HitDir); //Register the Attack
                    }
                }else{
                    int i = 0;
                    foreach(Collider2D col in sorted){
                        Hit(col, HitDir); //Register the Attack
                        i++;
                        if (i >= WeaponProperties.MaxHits){
                            Hitting = false;
                            break;
                        }
                    }
                }

            }
        }
    }

    public void Hit(Collider2D EnemyCol, Vector2 KnockBackDir){
        EnemyProperties enemyProperties = EnemyCol.gameObject.GetComponent<EnemyProperties>();
        if (enemyProperties && !HitEnemies.Contains(enemyProperties)){
            HitEnemies.Add(enemyProperties);
            
            float DamageOffsit = UnityEngine.Random.Range(-WeaponProperties.RandomDamageOffsit, WeaponProperties.RandomDamageOffsit);
            enemyProperties.HitEnemy(WeaponProperties.Damage + DamageOffsit, WeaponProperties.KnockBack * KnockBackDir, WeaponProperties);
            enemyProperties.CurrentEffects[EnemyProperties.Effects.Stunned] = StunLength;
            Properties.Durability -= 1;
        }
    }

    IEnumerator AfterHit(){
        Hitting = false;
        yield return null;
    }

    private void HitCheck(){
        HitEnemies.Clear();
        Hitting = true;
        HitDir = GameUtils.DirFromAToB(GameServices.GlobalVariables.PrimaryHandObject.Center.transform.position, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition));
    }

    public void PlayAudio(){
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(0.8f, 1.2f);
        GameServices.GlobalVariables.OffHandObject.AudioSource.pitch = RanPitch;

        //finally play the audio clip
        GameServices.GlobalVariables.OffHandObject.AudioSource.PlayOneShot(SlashAudio);
    }
}

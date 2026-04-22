using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerBehavior : ItemBehavior
{
    public WeaponPropertiesHolder WeaponProperties;
    [Header("Settings")]
    public bool Automatic;
    public float Range;
    public AudioClip SlashAudio;
    public AnimationClip SlashAnimation;
    public GameObject SlashParticle;

    [Header("Debug")]
    public bool DebugMode;
    public float Timer;
    public HashSet<EnemyProperties> HitEnemies = new HashSet<EnemyProperties>(); 
    public bool Hitting = false;
    public Vector2 HitDir;

    public override void Equipped(){
        Properties.ItemRenderer.enabled = false;
        GameServices.GlobalVariables.OffHandObject.LeftHand.SetActive(false);
    }

    private void Update() {
        if (SlashParticle.activeSelf != Hitting)SlashParticle.SetActive(Hitting);
    }

    public override void UnEquipped(){
        Properties.ItemRenderer.enabled = true;
    }

    public override void LateHold(){
        if (!UIManager.AMenuIsOpened())
            if (!Hitting)
                GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.CenterObject.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);
    }

    public override void Hold(){
        if (!UIManager.AMenuIsOpened()){
            Timer += Time.deltaTime;

            if (Timer >= WeaponProperties.Cooldown && Input.GetMouseButtonDown(1)){
                PlayAudio();
                HitCheck();
                Timer = 0;
                HitEnemies.Clear();

                Vector2 dir = GameUtils.DirFromAToB(GameServices.GlobalVariables.OffHandObject.CenterObject.transform.position, GameUtils.QuickWorldMousePosition());
                GameServices.GlobalVariables.Player.rig.AddForce(WeaponProperties.Recoil * -dir, ForceMode2D.Impulse);

                Properties.AnimationPlayer.RunAfterAnimationIsOver = AfterHit;
                Properties.AnimationPlayer.PlayClip(SlashAnimation);
                
            }

            Vector2 pos = transform.position;

            if (Hitting){
                HashSet<Collider2D> hits = GameUtils.SquareHitDetection(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform.position, 
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
            enemyProperties.CurrentEffects[EnemyProperties.Effects.Stunned] = WeaponProperties.StunLength;
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
        HitDir = GameUtils.DirFromAToB(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform.position, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition));
    }

    public void PlayAudio(){
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(0.8f, 1.2f);

        //finally play the audio clip
       GameUtils.PlayAudio(SlashAudio, GameServices.GlobalVariables.Player.GameObject.transform.position, 0, 99, RanPitch);
    }
}

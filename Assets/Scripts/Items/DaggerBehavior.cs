using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerBehavior : ItemBehavior
{
    [Header("Settings")]
    public float Damage;
    public float RandomDamageOffsit;
    public bool Automatic;
    public float Cooldown;
    public float Range;
    public float KnockBack;
    public float StunLength;

    public AudioClip SlashAudio;
    public AnimationClip SlashAnimation;

    [Header("Debug")]
    public bool DebugMode;
    public float Timer;
    public HashSet<EnemyProperties> HitEnemies = new HashSet<EnemyProperties>(); 
    public bool Hitting = false;
    public Vector2 HitDir;

    public override void Equipped(){
        GameServices.GlobalVariables.OffHandObject.Collider.enabled = false;
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = false;
        GameServices.GlobalVariables.OffHandObject.LeftHand.SetActive(false);
    }

    public override void UnEquipped(){
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = true;
    }

    public override void Hold(){
        if (!GameServices.UI.AMenuIsOpened()){
            Timer += Time.deltaTime;

            if (!Hitting){
                GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.Center.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);
            }

            if (Timer >= Cooldown && Input.GetMouseButtonDown(1)){
                PlayAudio();
                HitCheck();
                Timer = 0;

                GameServices.GlobalVariables.OffHandObject.AnimationPlayer.RunAfterAnimationIsOver = AfterHit;
                GameServices.GlobalVariables.OffHandObject.AnimationPlayer.PlayClip(SlashAnimation);
                
            }

            if (Hitting){
                List<Collider2D> HitColliders = GameUtils.SquareHitDetection(GameServices.GlobalVariables.PrimaryHandObject.Center.transform.position, 
                    HitDir, 
                    Range, 
                    5, 
                    0.35f, 
                    "Enemy", 
                    GameUtils.LayerMaskFromNumbers(-4), true
                );

                foreach (Collider2D Col in HitColliders){
                    EnemyProperties enemyProperties = Col.gameObject.GetComponent<EnemyProperties>();
                    if (enemyProperties && !HitEnemies.Contains(enemyProperties)){
                        HitEnemies.Add(enemyProperties);
                        
                        float DamageOffsit = UnityEngine.Random.Range(-RandomDamageOffsit, RandomDamageOffsit);
                        enemyProperties.HitEnemy(Damage + DamageOffsit, KnockBack * HitDir);
                        enemyProperties.CurrentStats[EnemyProperties.Stat.Stunned] = true;
                        GameUtils.StartIndependentCoroutine(() => DelayStun(enemyProperties));
                        Properties.Durability -= 1;
                    }
                }
            }
        }
    }

    IEnumerator AfterHit(){
        Hitting = false;
        yield return null;
    }

    IEnumerator DelayStun(EnemyProperties Prop){
        yield return new WaitForSeconds(StunLength);
        Prop.CurrentStats[EnemyProperties.Stat.Stunned] = false;
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

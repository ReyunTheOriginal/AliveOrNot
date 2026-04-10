using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletGun : ItemBehavior
{
    [Header("Settings")]
    public float Damage;
    public int MaxHits;
    public float RandomDamageOffsit;
    public bool Automatic;
    public float Cooldown;
    public float Range;
    public float KnockBack;
    public float Recoil;
    public int amountOfRays;
    public GameObject MuzzleFlash;
    public Vector2 BarrelEndLocation;

    public AudioClip GunShotAudio;
    public AnimationClip GunShotAnimation;

    [Header("Debug")]
    public bool DebugMode;
    public float Timer;

    public override void Equipped(){
        //when the gun is equipped it will assign the end of the barrel position to the primary hand's MuzzeFlashLocation Object
        GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.localPosition = BarrelEndLocation;
        //added this so you don't have to wait for the cooldown to use the gun for the first time after equipping it
        Timer = Cooldown;
    }
    public override void Hold(){
        if (!GameServices.UI.AMenuIsOpened()){
             //make the primary hand face the mouse for aiming
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.PrimaryHandObject.Center.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);

            //increase the timer for the cooldown
            Timer += Time.deltaTime;

            //if Automatic is checked you won't need to spam the mouse button to shoot
            if (Automatic){
                //only run when timer is higher or equel to Cooldown which delays each shot
                if (Timer >= Cooldown && Input.GetMouseButton(0)){
                    Shoot(); 
                    PlayAudio();
                    Timer = 0; //reset Timer
                }
            }else{
                //only run when timer is higher or equel to Cooldown which delays each shot
                if (Timer >= Cooldown && Input.GetMouseButtonDown(0)){
                    Shoot();
                    PlayAudio();
                    Timer = 0; //reset Timer
                }
            }
        }

    }

    public void PlayAudio(){
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(0.8f, 1.2f);
        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.pitch = RanPitch;

        //finally play the audio clip
        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.PlayOneShot(GunShotAudio);
    }

    public void Shoot(){
        //get the screen mouse position
        Vector2 MouseRawPos = Input.mousePosition;
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(MouseRawPos); //convert it to world position

        //get the direction from the barrel position to the mouse world position;
        Vector2 dir = GameUtils.DirFromAToB(GameServices.GlobalVariables.PrimaryHandObject.Center.transform.position, MousePos);

        //get the angle of the direction (Radians)
        float rotate = Mathf.Atan2(dir.y,dir.x);

        //Create a Raycast from the end of the barrel to the mouse's direction that's the length of {Range}
        Vector2 pos = GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position;
        List<Collider2D> hits = GameUtils.SquareHitDetection(pos, dir, Range, amountOfRays, 0.5f ,"Enemy", GameUtils.LayerMaskFromNumbers(-4), DebugMode);

       if (MaxHits == 0){
            foreach(Collider2D col in hits){
                Hit(col, dir); //Register the Attack
            }
       }else{
            int i = 0;
            foreach(Collider2D col in hits){
                Hit(col, dir); //Register the Attack
                i++;
                if (i >= MaxHits)break;
            }
       }

        //Create a MuzzleFlash
        GameObject flash = Instantiate(MuzzleFlash, GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position, Quaternion.Euler(0,0,rotate * Mathf.Rad2Deg - 90));
        
        //Rotate the MuzzleFlash to face the Mouse
        ParticleSystem ps = flash.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startRotation = -rotate;
        
        GameServices.GlobalVariables.PrimaryHandObject.AnimationPlayer.PlayClip(GunShotAnimation);

        GameServices.GlobalVariables.Player.rig.AddForce(-dir * Recoil, ForceMode2D.Impulse);

        if (Properties.HasDurability)Properties.Durability -= 1;
    }

    public void Hit(Collider2D Col, Vector2 KnockBackDir){
        if (Col != null){
            if (Col.gameObject.CompareTag("Enemy")){
                EnemyProperties Properties = Col.gameObject.GetComponent<EnemyProperties>();

                float dam = Damage + UnityEngine.Random.Range(-RandomDamageOffsit, RandomDamageOffsit);
                Properties.HitEnemy(dam, KnockBackDir * KnockBack);
            }
        }
    }
}

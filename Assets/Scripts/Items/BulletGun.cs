using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGun : ItemBehavior
{
    [Header("Settings")]
    public float Damage;
    public float RandomDamageOffsit;
    public bool Automatic;
    public float Cooldown;
    public float Range;
    public float KnockBack;
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
    }
    public override void Hold(){
        //make the primary hand face the mouse for aiming
        GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.PrimaryHandObject.Center, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0, true);

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

        //get the direction from the Player position to the mouse world position;
        Vector2 dir = (MousePos - (Vector2)GameServices.GlobalVariables.Player.transform.position).normalized;

        //get the angle of the direction (Radians)
        float rotate = Mathf.Atan2(dir.y,dir.x);

        //Create a Raycast from the end of the barrel to the mouse's direction that's the length of {Range}
        RaycastHit2D hit = Physics2D.Raycast(GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position, dir, Range);
        Hit(hit, dir); //Register the Attack

        //if Debug Mode is on visualize the Raycast
        if(DebugMode)Debug.DrawRay(GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position, dir*Range,Color.red);

        //Create a MuzzleFlash
        GameObject flash = Instantiate(MuzzleFlash, GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position, Quaternion.Euler(0,0,rotate * Mathf.Rad2Deg - 90));
        
        //Rotate the MuzzleFlash to face the Mouse
        ParticleSystem ps = flash.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startRotation = -rotate;
        
        GameServices.GlobalVariables.PrimaryHandObject.AnimationPlayer.PlayClip(GunShotAnimation);
    }

    public void Hit(RaycastHit2D Ray, Vector2 KnockBackDir){
        if (Ray.collider != null){
            if (Ray.collider.gameObject.CompareTag("Enemy")){
                EnemyProperties Properties = Ray.collider.gameObject.GetComponent<EnemyProperties>();

                float dam = Damage + UnityEngine.Random.Range(-RandomDamageOffsit, RandomDamageOffsit);
                Properties.HitEnemy(dam);
                Properties.rig.AddForce(KnockBackDir * KnockBack, ForceMode2D.Impulse);
            }
        }
    }
}

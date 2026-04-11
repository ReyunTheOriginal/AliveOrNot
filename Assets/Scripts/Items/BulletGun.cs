using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGun : ItemBehavior
{
    [Header("Settings")]
    public AnimationCurve DamageFalloff;
    public float Damage;
    public int MaxHits;
    public float RandomDamageOffsit;
    public bool Automatic;
    public float Cooldown;
    public float KnockBack;
    public float Recoil;
    public GameObject MuzzleFlash;
    public Vector2 BarrelEndLocation;
    [Space]
    public float Range;
    public int amountOfRays;
    public float AttackRadius;
    [Space]
    public AudioClip GunShotAudio;
    public float BasePitch;
    public float PitchChangeRange;
    public AnimationClip GunShotAnimation;
    public AudioClip EmptyChamberClick;
    [Space]
    public int BulletID;
    public int ChamberSize;
    public float ReloadSpeed;

    [Header("Debug")]
    public int BulletsInChamber;
    public bool DebugMode;
    public float Timer;

    private void Start() {
        Properties.Damage = Damage;
        Properties.Range = Range;
        Properties.AttackSpeed = 1.0f / Cooldown;
    }

    private void OnEnable() {
        if (Properties)Properties.Damage = Damage;
        if (Properties)Properties.Range = Range;
        if (Properties)Properties.AttackSpeed = 1.0f / Cooldown;
    }

    public override void Equipped(){
        //when the gun is equipped it will assign the end of the barrel position to the primary hand's MuzzeFlashLocation Object
        GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.localPosition = BarrelEndLocation;
        //added this so you don't have to wait for the cooldown to use the gun for the first time after equipping it
        Timer = Cooldown;

        GameUtils.SetCursor(GameServices.GlobalVariables.Cursors.Gun, new Vector2(GameServices.GlobalVariables.Cursors.Gun.width /2, GameServices.GlobalVariables.Cursors.Gun.height/2 ));
    }

    public override void UnEquipped(){
        GameUtils.ResetCursor();
    }

    public override void LateHold(){
        if (!GameServices.UI.AMenuIsOpened())
            //make the primary hand face the mouse for aiming
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.PrimaryHandObject.Center.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);

    }

    public override void Hold(){
        if (!GameServices.UI.AMenuIsOpened()){
            //increase the timer for the cooldown
            Timer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.R)){
                GameUtils.StartIndependentCoroutine(() => Reload());
            }

            
            //if Automatic is checked you won't need to spam the mouse button to shoot
            if (Automatic){
                    //only run when timer is higher or equel to Cooldown which delays each shot
                    if (Timer >= Cooldown && Input.GetMouseButton(0)){
                        if (BulletsInChamber > 0){
                            Shoot(); 
                            PlayAudio();
                            Timer = 0; //reset Timer
                        }else{
                            //play the EmptyChamber Click Audio Clip
                            GameServices.GlobalVariables.PrimaryHandObject.AudioSource.PlayOneShot(EmptyChamberClick);
                            Timer = 0; //reset Timer
                        }
                    }
            }else{
                //only run when timer is higher or equel to Cooldown which delays each shot
                if (Timer >= Cooldown && Input.GetMouseButtonDown(0)){
                    if (BulletsInChamber > 0){
                        Shoot(); 
                        PlayAudio();
                        Timer = 0; //reset Timer
                    }else{
                        //play the EmptyChamber Click Audio Clip
                        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.PlayOneShot(EmptyChamberClick);
                        Timer = 0; //reset Timer
                    }
                }
            }
        }
    }

    public IEnumerator Reload(){
        InventoryScript.Item Bullet = GameServices.Inventory.GetItemWithID(BulletID);
        if (Bullet != null){
            int needed = ChamberSize - BulletsInChamber;
            int available = Bullet.Amount;
            int toLoad = Mathf.Min(needed, available);

            GameServices.Inventory.ChangeItemAmount(Bullet, -toLoad);
            BulletsInChamber += toLoad;
        }
        
        yield return null;
    }

    public void PlayAudio(){
        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.pitch = BasePitch;
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(-PitchChangeRange, PitchChangeRange);
        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.pitch += RanPitch;

        //finally play the audio clip
        GameServices.GlobalVariables.PrimaryHandObject.AudioSource.PlayOneShot(GunShotAudio);
    }

    public void Shoot(){
        BulletsInChamber--;

        //get the screen mouse position
        Vector2 MouseRawPos = Input.mousePosition;
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(MouseRawPos); //convert it to world position

        //get the direction from the barrel position to the mouse world position;
        Vector2 dir = GameUtils.DirFromAToB(GameServices.GlobalVariables.PrimaryHandObject.Center.transform.position, MousePos);

        //get the angle of the direction (Radians)
        float rotate = Mathf.Atan2(dir.y,dir.x);

        //Create a Raycast from the end of the barrel to the mouse's direction that's the length of {Range}
        Vector2 pos = GameServices.GlobalVariables.PrimaryHandObject.MuzzleFlashLocation.transform.position;
        HashSet<Collider2D> hits = GameUtils. AngledHitDetection(pos, dir, Range, amountOfRays, AttackRadius, "Enemy", GameUtils.LayerMaskFromNumbers(-4), true);

        List<Collider2D> sorted = new List<Collider2D>(hits);

        if (MaxHits > 1)
            sorted.Sort((a, b) =>{
                float da = ((Vector2)a.ClosestPoint(pos) - pos).sqrMagnitude;
                float db = ((Vector2)b.ClosestPoint(pos) - pos).sqrMagnitude;
                return da.CompareTo(db);
            });

       if (MaxHits == 0){
            foreach(Collider2D col in sorted){
                Hit(col, dir); //Register the Attack
            }
       }else{
            int i = 0;
            foreach(Collider2D col in sorted){
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

        if (GunShotAnimation)GameServices.GlobalVariables.PrimaryHandObject.AnimationPlayer.PlayClip(GunShotAnimation);

        GameServices.GlobalVariables.Player.rig.AddForce(-dir * Recoil, ForceMode2D.Impulse);

        if (Properties.HasDurability)Properties.Durability -= 1;
    }

    public void Hit(Collider2D Col, Vector2 KnockBackDir){
        if (Col != null){
            if (Col.gameObject.CompareTag("Enemy")){
                EnemyProperties Properties = Col.gameObject.GetComponent<EnemyProperties>();

                float distance = Vector2.Distance(
                    Col.ClosestPoint((Vector2)GameServices.GlobalVariables.Player.GameObject.transform.position),
                    (Vector2)GameServices.GlobalVariables.Player.GameObject.transform.position
                );

                float t = Mathf.Clamp01(distance / Range);
                float multiplier = DamageFalloff.Evaluate(t);

                float dam = Damage + UnityEngine.Random.Range(-RandomDamageOffsit, RandomDamageOffsit);
                Properties.HitEnemy(dam * multiplier, KnockBackDir * KnockBack);
            }
        }
    }
}

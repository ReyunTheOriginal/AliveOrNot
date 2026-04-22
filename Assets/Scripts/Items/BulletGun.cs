using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BulletGun : ItemBehavior
{
    public WeaponPropertiesHolder WeaponProperties;
    [Header("Settings")]
    public AnimationCurve DamageFalloff;
    public FireModes FireMode;
    public GameObject MuzzleFlash;
    public Transform BarrelEndLocation;
[Header("UI")]
    public AmmoDataUI AmmoUI;
[Header("Raycast Settings")]
    public float Range;
    public int amountOfRays;
    public float AttackRadius;
    [Range(0,1)]
    public float Accuracy;
[Header("Audio And Animation Settings")]
    public AudioClip GunShotAudio;
    public float BasePitch;
    public float PitchChangeRange;
    public AnimationClip GunShotAnimation;
[Header("Reloading Settings")]
    public AnimationClip ReloadAnimation;
    public ItemProperties BulletProperty;
    public int ChamberSize;
    public AudioClip EmptyChamberClick;
[Header("Burst Settings")]
    public float BurstCoolDown;
    public int ShotsPerBurst;
[Header("Debug")]
    public int CurrentBurstShots;
    public bool Reloading;
    public int BulletsInChamber;
    public bool DebugMode;
    public float Timer;
    public float BurstTimer;
    public float autoReloadTimer;
    public GameUtils.IndependentCoroutine ReloadCoroutine;


    public enum FireModes{
        Automatic,
        Burst,
        SingleShot
    }

    public override void Equipped(){
        Reloading = false;
        //added this so you don't have to wait for the cooldown to use the gun for the first time after equipping it
        Timer = WeaponProperties.Cooldown;

        AmmoUI.WholeUI = Properties.CustomUICanvasGroup;
        AmmoUI.Text = Properties.CustomUICanvasGroup.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public override void UnEquipped(){
        GameUtils.ResetCursor();
        if (ReloadCoroutine != null)ReloadCoroutine.Stop(); 
    }

    public override void LateHold(){
        if (!UIManager.AMenuIsOpened() && !Reloading)
            //make the primary hand face the mouse for aiming
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);

    }

    public override void Hold(){
        AmmoUI.Text.text = $"{BulletsInChamber}/{GameServices.Inventory.GetItemAmountWithID(BulletProperty.ID)}";

        if (!UIManager.AMenuIsOpened()){
            //increase the timer for the cooldown
            Timer += Time.deltaTime;
            BurstTimer += Time.deltaTime;

            if (ReloadAnimation && BulletsInChamber < ChamberSize && GameServices.Inventory.AlreadyHasItemWithID(BulletProperty.ID) && !Reloading && Input.GetKeyDown(KeyCode.R)){
                //GameUtils.StartIndependentCoroutine(() => Reload());
                Reloading = true;

                ReloadCoroutine = GameUtils.StartIndependentCoroutine(() => Reload());
                Properties.AnimationPlayer.PlayClip(ReloadAnimation);
            }

            if (ReloadAnimation && BulletsInChamber < ChamberSize && GameServices.Inventory.AlreadyHasItemWithID(BulletProperty.ID) && !Reloading && BulletsInChamber <= 0){
                autoReloadTimer += Time.deltaTime;

                if (autoReloadTimer >= 0.25f){
                    autoReloadTimer = 0;

                    //GameUtils.StartIndependentCoroutine(() => Reload());
                    Reloading = true;

                    ReloadCoroutine = GameUtils.StartIndependentCoroutine(() => Reload());
                    Properties.AnimationPlayer.PlayClip(ReloadAnimation);
                }
            }

            if (!Reloading){
                //if Automatic is checked you won't need to spam the mouse button to shoot
                if (FireMode == FireModes.Automatic){
                        //only run when timer is higher or equel to Cooldown which delays each shot
                        if (Timer >= WeaponProperties.Cooldown && Input.GetMouseButton(0)){
                            if (BulletsInChamber > 0){
                                Shoot(); 
                                PlayAudio();
                                Timer = 0; //reset Timer
                            }else{
                                //play the EmptyChamber Click Audio Clip
                                GameUtils.PlayAudio(EmptyChamberClick, GameServices.GlobalVariables.Player.GameObject.transform.position, 0, 99);
                                Timer = 0; //reset Timer
                            }
                        }
                }else if (FireMode == FireModes.SingleShot){
                    //only run when timer is higher or equel to Cooldown which delays each shot
                    if (Timer >= WeaponProperties.Cooldown && Input.GetMouseButtonDown(0) && !Reloading){
                        if (BulletsInChamber > 0){
                            Shoot(); 
                            PlayAudio();
                            Timer = 0; //reset Timer
                        }else{
                            //play the EmptyChamber Click Audio Clip
                            GameUtils.PlayAudio(EmptyChamberClick, GameServices.GlobalVariables.Player.GameObject.transform.position, 0, 99);
                            Timer = 0; //reset Timer
                        }
                    }
                }else if (FireMode == FireModes.Burst){
                    //only run when timer is higher or equel to Cooldown which delays each shot
                    if (Timer >= WeaponProperties.Cooldown && Input.GetMouseButton(0)){
                        if (BulletsInChamber > 0){
                           if (BurstTimer >= BurstCoolDown){
                                Shoot(); 
                                PlayAudio();
                                BurstTimer = 0;
                                CurrentBurstShots++;
                                if (CurrentBurstShots >= ShotsPerBurst){
                                    Timer = 0; //reset Timer
                                    CurrentBurstShots = 0;
                                }

                           }
                        }else{
                            //play the EmptyChamber Click Audio Clip
                            GameUtils.PlayAudio(EmptyChamberClick, GameServices.GlobalVariables.Player.GameObject.transform.position, 0, 99);
                            Timer = 0; //reset Timer
                        }
                    }

                    if (Input.GetMouseButtonUp(0))
                        CurrentBurstShots = 0;
                }

            }


        }
    }

    public IEnumerator Reload(){
        yield return new WaitForSeconds(ReloadAnimation.length);
        ItemProperties Bullet = GameServices.Inventory.GetItemWithID(BulletProperty.ID);
        if (Bullet != null){
            int needed = ChamberSize - BulletsInChamber;
            int available = Bullet.Amount;
            int toLoad = Mathf.Min(needed, available);

            GameServices.Inventory.ChangeItemAmount(Bullet, -toLoad);
            BulletsInChamber += toLoad;
        }
        
        CurrentBurstShots = 0;
        Reloading = false;
        ReloadCoroutine = null;
        yield return null;
    }

    public void PlayAudio(){
        float Pitch = BasePitch;
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(-PitchChangeRange, PitchChangeRange);
        Pitch += RanPitch;

        //finally play the audio clip
        GameUtils.PlayAudio(GunShotAudio, GameServices.GlobalVariables.Player.GameObject.transform.position, 0, 99, Pitch);
    }

    public void Shoot(){
        BulletsInChamber--;

        //get the screen mouse position
        Vector2 MouseRawPos = Input.mousePosition;
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(MouseRawPos); //convert it to world position

        //get the direction from the barrel position to the mouse world position;
        Vector2 dir = GameUtils.DirFromAToB(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform.position, MousePos);

        // accuracy: 0 → bad, 1 → perfect
        float maxAngle = Mathf.Lerp(30f, 0f, Accuracy); // tweak 30f as you like

        float angleOffset = Random.Range(-maxAngle, maxAngle);

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + angleOffset;

        Vector2 finalDir = new Vector2(
            Mathf.Cos(finalAngle * Mathf.Deg2Rad),
            Mathf.Sin(finalAngle * Mathf.Deg2Rad)
        );

        //get the angle of the direction (Radians)
        float rotate = Mathf.Atan2(dir.y,dir.x);

        //Create a Raycast from the end of the barrel to the mouse's direction that's the length of {Range}
        Vector2 pos = BarrelEndLocation.position;
        HashSet<Collider2D> hits = GameUtils. AngledHitDetection(pos, finalDir, Range, amountOfRays, AttackRadius, "Enemy", GameUtils.LayerMaskFromNumbers(-4), true);

        List<Collider2D> sorted = new List<Collider2D>(hits);

        if (WeaponProperties.MaxHits > 1)
            sorted.Sort((a, b) =>{
                float da = ((Vector2)a.ClosestPoint(pos) - pos).sqrMagnitude;
                float db = ((Vector2)b.ClosestPoint(pos) - pos).sqrMagnitude;
                return da.CompareTo(db);
            });

       if (WeaponProperties.MaxHits == 0){
            foreach(Collider2D col in sorted){
                Hit(col, dir); //Register the Attack
            }
       }else{
            int i = 0;
            foreach(Collider2D col in sorted){
                Hit(col, dir); //Register the Attack
                i++;
                if (i >= WeaponProperties.MaxHits)break;
            }
       }

        //Create a MuzzleFlash
        GameObject flash = Instantiate(MuzzleFlash, BarrelEndLocation.position, Quaternion.Euler(0,0,rotate * Mathf.Rad2Deg - 90));

        //Rotate the MuzzleFlash to face the Mouse
        ParticleSystem ps = flash.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startRotation = -rotate;

        if (GunShotAnimation)Properties.AnimationPlayer.PlayClip(GunShotAnimation);
        GameServices.GlobalVariables.Player.rig.AddForce(-dir * WeaponProperties.Recoil, ForceMode2D.Impulse);
        if (Properties.HasDurability)Properties.Durability -= 1;
    }

    public void Hit(Collider2D Col, Vector2 KnockBackDir){
        if (Col != null){
            if (Col.gameObject.CompareTag("Enemy")){
                EnemyProperties Properties = Col.gameObject.GetComponent<EnemyProperties>();

                Properties.CurrentEffects[EnemyProperties.Effects.Stunned] = WeaponProperties.StunLength;

                float distance = Vector2.Distance(
                    Col.ClosestPoint((Vector2)GameServices.GlobalVariables.Player.GameObject.transform.position),
                    (Vector2)GameServices.GlobalVariables.Player.GameObject.transform.position
                );

                float t = Mathf.Clamp01(distance / Range);
                float multiplier = DamageFalloff.Evaluate(t);

                float dam = WeaponProperties.Damage + UnityEngine.Random.Range(-WeaponProperties.RandomDamageOffsit, WeaponProperties.RandomDamageOffsit);
                Properties.HitEnemy(dam * multiplier, KnockBackDir * WeaponProperties.KnockBack, WeaponProperties);
            }
        }
    }

    [System.Serializable]
    public class AmmoDataUI{
        public GameObject WholeUI;
        public TMP_Text Text;
    }
}

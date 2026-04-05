using System.Collections;
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

    public AudioClip SlashAudio;
    public AnimationClip SlashAnimation;

    [Header("Debug")]
    public bool DebugMode;
    public float Timer;

    public override void Equipped(){
        GameServices.GlobalVariables.OffHandObject.Collider.enabled = false;
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = false;
        GameServices.GlobalVariables.OffHandObject.LeftHand.SetActive(false);
    }

    public override void UnEquipped(){
        GameServices.GlobalVariables.OffHandObject.ObjectRenderer.enabled = true;
    }

    public override void Hold(){

        GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.Center, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0, true);

        if (Input.GetMouseButtonDown(1)){
            PlayAudio();
            HitCheck();

            GameServices.GlobalVariables.OffHandObject.AnimationPlayer.RunAfterAnimationIsOver = AfterHit;
            GameServices.GlobalVariables.OffHandObject.AnimationPlayer.PlayClip(SlashAnimation);
            
        }
    }

    IEnumerator AfterHit(){
        Debug.Log("After Hit");
        GameServices.GlobalVariables.OffHandObject.Collider.enabled = false;
        yield return null;
    }

    private void HitCheck(){
        GameServices.GlobalVariables.OffHandObject.Collider.enabled = true;
    }

    public void PlayAudio(){
        //randomly change the pitch of the audio slightly
        float RanPitch = UnityEngine.Random.Range(0.8f, 1.2f);
        GameServices.GlobalVariables.OffHandObject.AudioSource.pitch = RanPitch;

        //finally play the audio clip
        GameServices.GlobalVariables.OffHandObject.AudioSource.PlayOneShot(SlashAudio);
    }
}
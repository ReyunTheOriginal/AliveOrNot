using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour{

    public float MaxHealth = 100;
    public float Health = 100;
    public float BackBarDelay;
    public float Armor;
    public Sprite StunSprite;
    public HealthBarUI HealthBar;
    public float RegenerationSpeed = 0.1f;
    public float IFramesLength = 0.1f;

    [Header("Debug")]
    public float HealthBarDelayTimer = 0;
    public float IFrameTimer = 0;
    public Coroutine DelayCoroutine;

    void Update(){
        IFrameTimer += Time.deltaTime;

        //reset the delaybar Timer
        if (HealthBar.DelayedBar.fillAmount == HealthBar.Bar.fillAmount)HealthBarDelayTimer = 0;

        if (HealthBar.DelayedBar.fillAmount > HealthBar.Bar.fillAmount){
            HealthBarDelayTimer += Time.deltaTime;

            if (HealthBarDelayTimer >= BackBarDelay && DelayCoroutine == null){
                DelayCoroutine = StartCoroutine(ShortenDelayedBar());
                HealthBarDelayTimer = 0;
            }
        }else{
            HealthBar.DelayedBar.fillAmount = HealthBar.Bar.fillAmount;
        }

        float healthPercent = Health/MaxHealth;
        HealthBar.Bar.fillAmount = healthPercent;
        HealthBar.Icon.fillAmount = healthPercent;

        if (GameServices.GlobalVariables.Player.MovementScript.CurrentStates[PlayerMovement.State.Dead]){
            if (Input.GetKeyDown(KeyCode.Space)){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                Time.timeScale = 1;
            }
        }else{
            RegenerateHealth();
        }
    }

    public void RegenerateHealth(){
        Health += Time.deltaTime * RegenerationSpeed;
    }

    public void TakeDamage(float Damage, Vector2 KnockBack, float StunLength){
        if (IFrameTimer >= IFramesLength){
            IFrameTimer = 0;
            float FinalDamage = Damage * (100 / (100 + Armor));
            Health -= FinalDamage;
            GameServices.GlobalVariables.Player.rig.AddForce(KnockBack, ForceMode2D.Impulse);
            StartCoroutine(DamageEffects(StunLength));

            GameServices.GlobalVariables.Player.SpriteRenderer.transform.rotation = Quaternion.Euler(0,KnockBack.x > 0? 180 : 0,0);

            if (Health <= 0){
                GameServices.GlobalVariables.Player.MovementScript.CurrentStates[PlayerMovement.State.Dead] = true;
                Time.timeScale = 0;
            }

            //Reduce the Durability of all armor worn at the time of the hit
            /*if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item != null)
                if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item.ItemProperties)
                    GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item.ItemProperties.Durability--;

            if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item != null)
                if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item.ItemProperties)
                    GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item.ItemProperties.Durability--;

            if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item != null)
                if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item.ItemProperties)
                    GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item.ItemProperties.Durability--;*/
        }
    }
    
    IEnumerator ShortenDelayedBar(){
        while (HealthBar.DelayedBar.fillAmount > HealthBar.Bar.fillAmount + 0.035){
            yield return null;

            HealthBar.DelayedBar.fillAmount = Mathf.Lerp(
                HealthBar.DelayedBar.fillAmount,
                HealthBar.Bar.fillAmount,
                Time.deltaTime * HealthBar.lerpSpeed
            );
        }

        HealthBar.DelayedBar.fillAmount = HealthBar.Bar.fillAmount;
        DelayCoroutine = null;
    }

    IEnumerator DamageEffects(float timer){
        GameServices.GlobalVariables.Player.MovementScript.CurrentEffects[PlayerMovement.Effects.Stunned] = timer;
        GameServices.GlobalVariables.Player.Animator.enabled = false;
        GameServices.GlobalVariables.Player.SpriteRenderer.sprite = StunSprite;
        yield return new WaitForSeconds(timer);
        GameServices.GlobalVariables.Player.Animator.enabled = true;
    }


    [System.Serializable]
    public class HealthBarUI{
      public Image DelayedBar;
      public Image Icon;
      public Image Bar;  
      public float lerpSpeed = 3f;
    }

}

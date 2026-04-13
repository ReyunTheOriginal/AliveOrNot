using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour{

    public float MaxHealth = 100;
    public float Health = 100;
    public float BackBarDelay;
    public float Armor;
    public Sprite StunSprite;
    public HealthBarUI HealthBar;

    [Header("Debug")]
    public float Timer;
    public Coroutine DelayCoroutine;

    void Update(){
        if (HealthBar.DelayedBar.fillAmount == HealthBar.Bar.fillAmount){
            Timer = 0;
        }

        if (HealthBar.DelayedBar.fillAmount > HealthBar.Bar.fillAmount){
            Timer += Time.deltaTime;

            if (Timer >= BackBarDelay && DelayCoroutine == null){
                DelayCoroutine = StartCoroutine(ShortenDelayedBar());
                Timer = 0;
            }
        }else{
            HealthBar.DelayedBar.fillAmount = HealthBar.Bar.fillAmount;
        }

        float healthPercent = Health/MaxHealth;
        HealthBar.Bar.fillAmount = healthPercent;
        HealthBar.Icon.fillAmount = healthPercent;
    }

    public void TakeDamage(float Damage, Vector2 KnockBack, float StunLength){
        float FinalDamage = Damage * (100 / (100 + Armor));
        Health -= FinalDamage;
        GameServices.GlobalVariables.Player.rig.AddForce(KnockBack, ForceMode2D.Impulse);
        StartCoroutine(DamageEffects(StunLength));

        GameServices.GlobalVariables.Player.SpriteRenderer.transform.rotation = Quaternion.Euler(0,KnockBack.x > 0? 180 : 0,0);

        if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item != null)
            if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item.ItemProperties)
                GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Head].Item.ItemProperties.Durability--;

        if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item != null)
            if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item.ItemProperties)
                GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Torso].Item.ItemProperties.Durability--;

        if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item != null)
            if (GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item.ItemProperties)
                GameServices.GlobalVariables.Player.Equiptment.Equipment.Slots[ItemProperties.EquipSlot.Feet].Item.ItemProperties.Durability--;
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

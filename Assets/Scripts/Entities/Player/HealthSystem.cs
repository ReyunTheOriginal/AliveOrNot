using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour{

    public float MaxHealth = 100;
    public float Health = 100;
    public float BackBarDelay;
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

    public void TakeDamage(float Damage){
        Health -= Damage;
    }
    
    IEnumerator ShortenDelayedBar(){
        while (HealthBar.DelayedBar.fillAmount > HealthBar.Bar.fillAmount + 0.001){
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


    [System.Serializable]
    public class HealthBarUI{
      public Image DelayedBar;
      public Image Icon;
      public Image Bar;  
      public float lerpSpeed = 3f;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
[Header("Settings")]
    public float DayLengthInSeconds = 1200;
    [Range(0,1f)]
    //from 0 to the float's value is DayTime, from the Float's value to 1 is NightTime
    public float DayAndNightLength = 0.5f;
    public float transitionSize = 0.05f; // 5% of day length for dusk/dawn

[Header("Componenets")]
    public Light2D GlobalLight;

[Header("Debug")]
    public float LightIntinsity;
    public float CurrentPercentageOfDay;
    public float DayTimer = 0;
    public bool IsDayTime = true;
    public int CurrentDay = 0;

    private void Update(){
        IsDayTime = GameServices.IsDayTime;
        CurrentDay = GameServices.CurrentDay;

        DayTimer += Time.deltaTime;
        float t = DayTimer / DayLengthInSeconds; // 0 to 1 across full day

        GameServices.IsDayTime = t <= DayAndNightLength;
        CurrentPercentageOfDay = t * 100f;

        if (DayTimer >= DayLengthInSeconds){
            DayTimer = 0;
            GameServices.CurrentDay++;
        }

        // transition duration as fraction of day
        float transitionSize = 0.05f; // 5% of day length for dusk/dawn

        float dusk  = DayAndNightLength - transitionSize;
        float dawn  = 1f - transitionSize;

        if (t < dusk)
            LightIntinsity = 1f; // full day
        else if (t < DayAndNightLength)
            LightIntinsity = Mathf.Lerp(1f, 0f, (t - dusk) / transitionSize); // dusk
        else if (t < dawn)
            LightIntinsity = 0f; // full night
        else
            LightIntinsity = Mathf.Lerp(0f, 1f, (t - dawn) / transitionSize); // dawn

        GlobalLight.intensity = LightIntinsity;
    }
}

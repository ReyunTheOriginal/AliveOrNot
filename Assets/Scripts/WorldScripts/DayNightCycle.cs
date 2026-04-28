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
    public bool IsDayTime = true;

    private void Update(){
        GameServices.GlobalTimer += Time.deltaTime;
    
        float t = (GameServices.GlobalTimer % DayLengthInSeconds) / DayLengthInSeconds; // 0 to 1 within current day
        
        GameServices.CurrentDay = (int)(GameServices.GlobalTimer / DayLengthInSeconds);
        GameServices.IsDayTime = t <= DayAndNightLength;
        
        IsDayTime = GameServices.IsDayTime;
        CurrentPercentageOfDay = t * 100f;

        float dusk = DayAndNightLength - transitionSize;
        float dawn = 1f - transitionSize;

        if (t < dusk)
            LightIntinsity = 1f;
        else if (t < DayAndNightLength)
            LightIntinsity = Mathf.Lerp(1f, 0f, (t - dusk) / transitionSize);
        else if (t < dawn)
            LightIntinsity = 0f;
        else
            LightIntinsity = Mathf.Lerp(0f, 1f, (t - dawn) / transitionSize);

        GlobalLight.intensity = LightIntinsity;
    }
}

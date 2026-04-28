using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashLightBehavior : ItemBehavior
{
    public Light2D Light;
    public Light2D ambLight;

    public float LightIntensity;

    public override void LateHold()
    {
        if (!UIManager.AMenuIsOpened())
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.CenterObject.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition));
    }

    public override void Hold(){
        if (Input.GetKeyDown(KeyCode.F)){
            Light.intensity = (Light.intensity == 0? LightIntensity : 0);
            ambLight.intensity = (ambLight.intensity == 0? 0.09f : 0);
        }
    }
}

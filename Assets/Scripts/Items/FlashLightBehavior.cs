using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashLightBehavior : ItemBehavior
{
    public GameObject Light;

    public override void LateHold()
    {
        if (!UIManager.AMenuIsOpened())
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.CenterObject.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition));
    }

    public override void Hold(){
        if (Input.GetKeyDown(KeyCode.F)){
            Light.gameObject.SetActive(!Light.gameObject.activeInHierarchy);
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashLightBehavior : ItemBehavior
{
    public GameObject Light;

    public override void LateHold()
    {
        if (!GameServices.UI.AMenuIsOpened())
            GameUtils.MakeObjectLookAt(GameServices.GlobalVariables.OffHandObject.Center.transform, GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), 0);
    }

    public override void Hold(){
        if (Input.GetKeyDown(KeyCode.F)){
            Light.gameObject.SetActive(!Light.gameObject.activeInHierarchy);
        }
    }
}

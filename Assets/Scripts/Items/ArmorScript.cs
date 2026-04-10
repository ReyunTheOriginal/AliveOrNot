using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorScript : ItemBehavior{
    public float ArmorValue;
    public override void Equipped(){
        GameServices.GlobalVariables.Player.PlayerHealth.Armor += ArmorValue;
    }
    public override void UnEquipped(){
        GameServices.GlobalVariables.Player.PlayerHealth.Armor -= ArmorValue;
    }
}

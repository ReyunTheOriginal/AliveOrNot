using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorScript : ItemBehavior{
    public float ArmorValue;

    private void Start() {
        Properties.ArmorValue = ArmorValue;
    }

    private void OnEnable() {
        if (Properties)Properties.ArmorValue = ArmorValue;
    }
    
    public override void Equipped(){
        GameServices.GlobalVariables.Player.PlayerHealth.Armor += ArmorValue;
    }
    public override void UnEquipped(){
        GameServices.GlobalVariables.Player.PlayerHealth.Armor -= ArmorValue;
    }
}

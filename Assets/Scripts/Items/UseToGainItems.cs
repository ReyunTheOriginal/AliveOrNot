using UnityEngine;

public class UseToGainItems : ItemBehavior
{
    public GameObject ItemToGain;
    public int AmountToGain;
    public override void Use(){
        GameObject NewItem = Instantiate(ItemToGain, GameServices.GlobalVariables.Player.GameObject.transform.position, Quaternion.identity);
        ItemProperties prop = NewItem.GetComponent<ItemProperties>();
        NewItem.SetActive(false);
        if (prop){
            prop.Amount = AmountToGain;
            GameServices.Inventory.AddItem(prop);
        }
    }
}

using UnityEngine;

public class ItemBehavior : MonoBehaviour
{
    [HideInInspector] public ItemProperties Properties; 
    //Runs one time when used with the "Use" button in the inventory
    public virtual void Use(){

    }
    

    //runs one time when the item is Equipped
    public virtual void Equipped(){

    }
    //runs one time when the item is UnEquipped
    public virtual void UnEquipped(){

    }


    //happens every frame when Equipped
    public virtual void Hold(){

    }
}


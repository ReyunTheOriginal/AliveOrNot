using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : MonoBehaviour
{
    //int = ItemInstanceID
    public Dictionary<int, ItemProperties> Items = new Dictionary<int, ItemProperties>();
    public int MaximumAmountOfItems;

    public void SendItemToContainer(){

    }

    public void MergeItems(ItemProperties ItemA, ItemProperties ItemB){
        if (ItemA.ID == ItemB.ID && !ItemA.Unstackable && !ItemB.Unstackable){
            if (ItemA.Amount + ItemB.Amount <= ItemA.StackSize){
                ItemA.Amount += ItemB.Amount;

                if (Items.ContainsKey(ItemB.ItemInstanceID)) 
                    Items.Remove(ItemB.ItemInstanceID);

                Destroy(ItemB.gameObject);
            }else{
                int AmountToAdd = ItemA.StackSize - ItemA.Amount;
                ItemA.Amount = ItemA.StackSize; 
                ItemB.Amount -= AmountToAdd;
            }

            OnItemsChange();
        }
    }

    public void ChangeItemAmount(ItemProperties Item, int Amount){
        if (Items.ContainsKey(Item.ItemInstanceID)){
            
            if (Items[Item.ItemInstanceID].Amount + Amount > 0){
                //if the item amount won't end up being 0 or less
                Items[Item.ItemInstanceID].Amount += Amount;
            }else{
                //if the item amount will end up being 0 or less
                RemoveItem(Items[Item.ItemInstanceID]);
            }

        }
    }

    public void RemoveItem(ItemProperties Item){
        if (Items.ContainsKey(Item.ItemInstanceID)){
            ItemProperties ItemToDestroy = Item;

            //Destroy Object;
            if (ItemToDestroy.gameObject)
                Destroy(ItemToDestroy.gameObject);
            
            //Remove Item from Items Data
            Items.Remove(Item.ItemInstanceID);
            OnItemsChange();
        }
    }

    public ItemProperties GetItemWithID(int ItemID){
        ItemProperties result = null;

        foreach(var i in Items){
            if (i.Value.ID == ItemID){
                result = i.Value;
                break;
            }
        }

        return result;
    }

    public int GetItemAmountWithID(int ItemInstanceID){
        ItemProperties item = GetItemWithID(ItemInstanceID);
        if (item != null)
            return item.Amount;
        
        return 0;
    }

    public bool AlreadyHasItemWithID(int ItemInstanceID){
        bool result = false;

        foreach(var i in Items){
            if (i.Value.ID == ItemInstanceID){
                result = true;
                break;
            }
        }

        return result;
    }

    public ItemProperties AddItem(ItemProperties NewItem){
        if (Items.Count >= MaximumAmountOfItems && MaximumAmountOfItems < 0) return null;

        ItemProperties AlreadyExistingItem = GetItemWithID(NewItem.ID);
        if (AlreadyExistingItem == null || NewItem.Unstackable){
            //runs if it's a new item;

            //deactivate the scene item GameObject;
            NewItem.gameObject.SetActive(false);

            //add to the Item Data
            Items.Add(NewItem.ItemInstanceID, NewItem);

            OnItemsChange();
            return NewItem;
        }else{
            //deactivate the scene item GameObject;
            NewItem.gameObject.SetActive(false);

            //add to the Item Data
            Items.Add(NewItem.ItemInstanceID, NewItem);

            MergeItems(AlreadyExistingItem, NewItem);

            OnItemsChange();
            return AlreadyExistingItem;
        }
    }

    public virtual void OnItemsChange(){}
}

using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : MonoBehaviour
{
    //int = ItemInstanceID
    public Dictionary<int, ItemProperties> Items = new Dictionary<int, ItemProperties>();
    public int MaximumAmountOfItems;

    public void SendItemToContainer(ItemContainer ContainerToSendTo, ItemProperties ItemToTransfer, int AmountToSend = 1){
        if (!Items.ContainsKey(ItemToTransfer.ItemInstanceID)) return;

        if (ContainerToSendTo.Items.Count >= ContainerToSendTo.MaximumAmountOfItems && ContainerToSendTo.MaximumAmountOfItems < 0){ 
            Debug.Log("Container Full: " + ContainerToSendTo.gameObject.name);
            return;    
        }

        ContainerToSendTo.AddItem(ItemToTransfer, AmountToSend);
        ChangeItemAmount(ItemToTransfer, -AmountToSend, false);

        SafetyCheck(gameObject.name + " Sending To " + ContainerToSendTo.gameObject.name);
    }

   public void MergeItems(ItemProperties ItemA, ItemProperties ItemB){
        if (!Items.ContainsKey(ItemA.ItemInstanceID) || !Items.ContainsKey(ItemB.ItemInstanceID)) return;

        if (ItemA.ID == ItemB.ID && !ItemA.Unstackable && !ItemB.Unstackable){
            ChangeCache Cache = new ChangeCache();

            ItemA.Amount += ItemB.Amount;

            Items.Remove(ItemB.ItemInstanceID);
            Destroy(ItemB.gameObject);

            Cache.ChangedItems = true;
            Cache.InstanceIDsOfChanges.Add(ItemA.ItemInstanceID);

            SafetyCheck($"Merging {ItemA.gameObject.name} and {ItemB.gameObject.name}");
            OnItemsChange(Cache);
        }
    }

    public void ChangeItemAmount(ItemProperties Item, int Amount,  bool destroy = true){
        if (!Items.ContainsKey(Item.ItemInstanceID)) return;
            
        ChangeCache Cache = new ChangeCache();

        if (Items[Item.ItemInstanceID].Amount + Amount > 0){
            //if the item amount won't end up being 0 or less
            Items[Item.ItemInstanceID].Amount += Amount;

            Cache.ChangedItems = true;
            Cache.InstanceIDsOfChanges.Add(Item.ItemInstanceID);
        }else{
            //if the item amount will end up being 0 or less
            RemoveItem(Items[Item.ItemInstanceID], destroy);
        }

        SafetyCheck($"Changing Item: {Item.gameObject.name}");
        OnItemsChange(Cache);
    }

    public void RemoveItem(ItemProperties Item, bool destroy = true){
        if (!Items.ContainsKey(Item.ItemInstanceID)) return;

        
        //Remove Item from Items Data
        Items.Remove(Item.ItemInstanceID);

        ChangeCache Cache = new ChangeCache();
        Cache.RemovedItems = true;
        Cache.InstanceIDsOfRemovals.Add(Item.ItemInstanceID);

         //Destroy Object;
        if (destroy && Item.gameObject)
            Destroy(Item.gameObject);

        SafetyCheck($"Removing Item: {Item.gameObject.name}");
        OnItemsChange(Cache);
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

    public ItemProperties AddItem(ItemProperties NewItem, int Amount = 0){
        if (NewItem.Amount < 1){
            Destroy(NewItem.gameObject);
            return null;
        }

        if (Items.Count >= MaximumAmountOfItems && MaximumAmountOfItems > 0){ 
            Debug.Log("Container Full: " + gameObject.name);
            return null;    
        }

        ChangeCache Cache = new ChangeCache();

        ItemProperties AlreadyExistingItem = GetItemWithID(NewItem.ID);
        if (AlreadyExistingItem == null || NewItem.Unstackable){
            //runs if it's a new item;

            if (NewItem.Amount <= Amount || Amount == 0){
                //deactivate the scene item GameObject;
                NewItem.gameObject.SetActive(false);

                //add to the Item Data
                Items.Add(NewItem.ItemInstanceID, NewItem);

                Cache.AddedItems = true;
                Cache.InstanceIDsOfAdditions.Add(NewItem.ItemInstanceID);

                SafetyCheck($"Adding Item: {NewItem.gameObject.name}");
                OnItemsChange(Cache);
                return NewItem;
            }else{
                GameObject CopyObject = Instantiate(NewItem.gameObject, transform.position, Quaternion.identity);
                ItemProperties CopyItem = CopyObject.GetComponent<ItemProperties>();

                if (CopyItem){
                    NewItem.Amount -= Amount;
                    CopyItem.Amount = Amount;

                    //deactivate the scene item GameObject;
                    CopyObject.gameObject.SetActive(false);

                    //add to the Item Data
                    Items.Add(CopyItem.ItemInstanceID, CopyItem);

                    Cache.AddedItems = true;
                    Cache.InstanceIDsOfAdditions.Add(AlreadyExistingItem.ItemInstanceID);

                }else{
                    Destroy(CopyObject);
                }

                SafetyCheck($"Adding Item: {CopyItem.gameObject.name}");
                OnItemsChange(Cache);
                return CopyItem;
            }
        }else{

            if (NewItem.Amount <= Amount || Amount == 0){
                //deactivate the scene item GameObject;
                NewItem.gameObject.SetActive(false);

                //add to the Item Data
                Items.Add(NewItem.ItemInstanceID, NewItem);

                MergeItems(AlreadyExistingItem, NewItem);

                Cache.ChangedItems = true;
                Cache.InstanceIDsOfChanges.Add(AlreadyExistingItem.ItemInstanceID);

                SafetyCheck($"Adding Item: {AlreadyExistingItem.gameObject.name}");
                OnItemsChange(Cache);
                return AlreadyExistingItem;
            }else{
                GameObject CopyObject = Instantiate(NewItem.gameObject, transform.position, Quaternion.identity);
                ItemProperties CopyItem = CopyObject.GetComponent<ItemProperties>();

                if (CopyItem){
                    NewItem.Amount -= Amount;
                    CopyItem.Amount = Amount;

                    //deactivate the scene item GameObject;
                    CopyObject.gameObject.SetActive(false);

                    //add to the Item Data
                    Items.Add(CopyItem.ItemInstanceID, CopyItem);

                    MergeItems(AlreadyExistingItem, CopyItem);

                    Cache.AddedItems = true;
                    Cache.InstanceIDsOfAdditions.Add(AlreadyExistingItem.ItemInstanceID);
                }else{
                    Destroy(CopyObject);
                }

                SafetyCheck($"Adding Item: {AlreadyExistingItem.gameObject.name}");
                OnItemsChange(Cache);
                return AlreadyExistingItem;
            }
        }
    }

    public virtual void OnItemsChange(ChangeCache Cache){}

    public void SafetyCheck(string Source){
        HashSet<int> IDSToRemove = new HashSet<int>();

        foreach(var Item in Items){
            if (Item.Value == null){
               IDSToRemove.Add(Item.Key);
            }else if (Item.Value.Amount == 0){
                IDSToRemove.Add(Item.Key);
            }
        }

        if (IDSToRemove.Count > 0)
            Debug.LogWarning("Safety Triggered, Source:" + "'" + Source + "'");

        foreach (int i in IDSToRemove)
            Items.Remove(i);
    }


    public class ChangeCache{
        public bool SentItems;
        public bool AddedItems;
        public bool RemovedItems;
        public bool ChangedItems;
        public List<int> InstanceIDsOfChanges = new List<int>();
        public List<int> InstanceIDsOfAdditions = new List<int>();
        public List<int> InstanceIDsOfRemovals = new List<int>();
    }
}

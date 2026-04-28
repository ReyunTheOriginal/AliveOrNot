using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EquiptmentScript : MonoBehaviour
{
    
    public EquipmentData Equipment;
    public AudioClip EquipSoundEffect;

    void Awake(){
        //make the script visible in GameServices
        GameServices.Equipment = this;

        //make the equiptment dictionary from the classes in EquipmentData
        Equipment.InitDictionary();
    }

    void LateUpdate(){
        //go through each Equipped object and run its LateHold() Code, the code that runs every frame if equipped.
        foreach(var E in Equipment.Slots){
            //check if the slot actually has an item
            if (HasAnItemInSlot(E.Key)){
                ItemProperties Properties = E.Value.ItemContainer.Items.FirstOrDefault().Value; //properties script
                ItemBehavior Behavior = Properties.ItemBehavior; //behavior script that has the item's code

               //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItemInSlot(ItemProperties.EquipSlot.BothHands))
                            Behavior.LateHold();//run its code
                    }else{
                        Behavior.LateHold();//run its code
                    }
                }

            }
        }
    }

    void FixedUpdate(){
        //go through each Equipped object and run its FixedHold() Code, the code that runs every frame if equipped.
        foreach(var E in Equipment.Slots){
            //check if the slot actually has an item
            if (HasAnItemInSlot(E.Key)){
                ItemProperties Properties = E.Value.ItemContainer.Items.FirstOrDefault().Value; //properties script
                ItemBehavior Behavior = Properties.ItemBehavior; //behavior script that has the item's code

                //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItemInSlot(ItemProperties.EquipSlot.BothHands))
                            Behavior.FixedHold();//run its code
                    }else{
                        Behavior.FixedHold();//run its code
                    }
                }

            }
        }
    }

    void Update(){   
        keepHandRotation();
        
        //go through each Equipped object and run its Hold() Code, the code that runs every frame if equipped. also update Durability UI
        foreach(var E in Equipment.Slots){
            //check if the slot actually has an item
            if (HasAnItemInSlot(E.Key)){
                ItemProperties Properties = E.Value.ItemProperties; //properties script
                ItemBehavior Behavior = Properties.ItemBehavior; //behavior script that has the item's code

                //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItemInSlot(ItemProperties.EquipSlot.BothHands))
                            Behavior.Hold();//run its code
                    }else{
                        Behavior.Hold();//run its code
                        if (E.Key != ItemProperties.EquipSlot.BothHands)
                            Properties.transform.position = transform.position;
                    }
                }

                if (Properties.HasDurability){
                    if (Properties.Durability <= 0){
                        if (GameServices.Inventory.SelectedItem != null&& GameServices.Inventory.SelectedItem.InstanceID == Properties.ItemInstanceID)
                            GameServices.Inventory.UnSelectItem();

                        UnEquip(E.Value, (int)E.Key);
                        GameServices.Inventory.RemoveItem(GameServices.Inventory.Items[Properties.ItemInstanceID]);
                    }

                    //Update the Text
                    E.Value.HotBarUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("F1") + "/100";
                    E.Value.HotBarUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;

                    E.Value.InvUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("F1") + "/100";
                    E.Value.InvUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;
                }

            }
        }
    }

    //Runs when a Slot is Clicked
    public void EquipButton(int Slot){
            //get the Clicked Slot
            EquipmentSlot ESlot = Equipment.Slots[(ItemProperties.EquipSlot)Slot];
            
            if (HasAnItemInSlot((ItemProperties.EquipSlot)Slot)){
                int ID = ESlot.ItemProperties.ItemInstanceID;
                
                //Runs when an item is already in the slot:
                UnEquip(ESlot, Slot);

                var selected = GameServices.Inventory.SelectedItem;

                if ( 
                    selected != null &&
                    selected.ItemProperties != null &&
                    (int)selected.ItemProperties.equipSlot == Slot &&
                    selected.ItemProperties.Equippable &&
                    selected.ItemProperties.ItemInstanceID != ID
                ){
                    Equip(ESlot, Slot);
                }
                
            }else{ 
                //Runs when the Slot Is Empty:
                Equip(ESlot, Slot);
            }
    }

    public bool HasAnItemInSlot(ItemProperties.EquipSlot Slot){
        EquipmentSlot ESlot = Equipment.Slots[Slot];
        if ( ESlot.ItemContainer)return ESlot.ItemContainer.Items.Count > 0;
        return false;
    }

    public ItemProperties GetItemInSlot(ItemProperties.EquipSlot Slot){
        ItemProperties result = null;

        result = Equipment.Slots[Slot].ItemContainer.Items.FirstOrDefault().Value;

        return result;
    }
    public bool HasItemEquipped(int ItemInstanceID){
        foreach (var slot in Equipment.Slots){
            var i = slot.Value.ItemContainer.Items.FirstOrDefault().Value;
            if (i != null && i.ItemInstanceID == ItemInstanceID)
                return true;
        }

        return false;
    }

    public void UnEquip(EquipmentSlot ESlot, int Slot){
        //run the UnEquippted() Function
        ItemProperties Properties = ESlot.ItemContainer.Items.FirstOrDefault().Value;

        Properties.UnSetUpItem();
        ResetHands();

        //reset the slot icons to the defaults
        ESlot.InvUI.Icon.sprite = ESlot.DefaultIcon;
        ESlot.HotBarUI.Icon.sprite = ESlot.DefaultIcon;

        //hide the Durability UI
        UIManager.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
        UIManager.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);

        Properties.enabled = true;
        if (ESlot.ItemProperties.ItemRenderer)ESlot.ItemProperties.ItemRenderer.sortingLayerName = "Item";
        if (ESlot.ItemProperties.SpriteSorter)ESlot.ItemProperties.SpriteSorter.YOffsit = 0;

        ESlot.ItemContainer.SendItemToContainer( GameServices.Inventory, ESlot.ItemProperties);

        //reset the Equipment Slot Item
        ESlot.ItemContainer.Items.Clear();

         if (Slot == (int)ItemProperties.EquipSlot.BothHands){
            if (HasAnItemInSlot(ItemProperties.EquipSlot.PrimaryHand)){
                Equipment.Slots[ItemProperties.EquipSlot.PrimaryHand].ItemProperties.SetUpItem();
            }else if (HasAnItemInSlot(ItemProperties.EquipSlot.OffHand)){
                Equipment.Slots[ItemProperties.EquipSlot.OffHand].ItemProperties.SetUpItem();
            }
        }

        Properties.transform.localRotation = Quaternion.Euler(0,0,0);
    }

    public void Equip(EquipmentSlot ESlot, int Slot){
        //Check if an Object is Selected
        if (GameServices.Inventory.SelectedItem != null && GameServices.Inventory.SelectedItem.ItemProperties != null && (int)GameServices.Inventory.SelectedItem.ItemProperties.equipSlot == Slot && GameServices.Inventory.SelectedItem.ItemProperties.Equippable){
            //get Item Scripts
            ItemProperties Properties = GameServices.Inventory.SelectedItem.ItemProperties;

            GameUtils.PlayAudio(EquipSoundEffect, transform.position);

            ResetHands();

            //set the Slot Item to the Selected Item
            ESlot.ItemEntry = GameServices.Inventory.SelectedItem;

            GameServices.Inventory.SendItemToContainer(ESlot.ItemContainer, GameServices.Inventory.SelectedItem.ItemProperties);
            
            //Reactivate the Object so you can put it in the player's hand
            ESlot.ItemProperties.enabled = false;//disable the ItemProperties Script to Prevent Picking Up Again
            if (ESlot.ItemProperties.ItemRenderer)ESlot.ItemProperties.ItemRenderer.sortingLayerName = "Default";
            if (ESlot.ItemProperties.SpriteSorter)ESlot.ItemProperties.SpriteSorter.YOffsit = -0.6f;
            
            if (Slot == (int)ItemProperties.EquipSlot.BothHands){
                if (HasAnItemInSlot(ItemProperties.EquipSlot.PrimaryHand)){
                    Equipment.Slots[ItemProperties.EquipSlot.PrimaryHand].ItemProperties.UnSetUpItem();
                }else if (HasAnItemInSlot(ItemProperties.EquipSlot.OffHand)){
                    Equipment.Slots[ItemProperties.EquipSlot.OffHand].ItemProperties.UnSetUpItem();
                }
            }

            if (Slot == (int)ItemProperties.EquipSlot.PrimaryHand || Slot == (int)ItemProperties.EquipSlot.OffHand){
                if (!HasAnItemInSlot(ItemProperties.EquipSlot.BothHands))
                    Properties.SetUpItem();
            }else{
                Properties.SetUpItem();
            }

            //Handle Durability UI
            if (ESlot.ItemProperties.HasDurability){
                //Show Durability UI
                UIManager.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", true);
                UIManager.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", true);
            }else{
                //Hide Durability UI
                UIManager.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
                UIManager.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);
            }

            Properties.transform.localRotation = Quaternion.Euler(0,0,0);

            //Set up UI
            ESlot.InvUI.Icon.sprite = ESlot.ItemProperties.ItemSprite; //set the Inventory Icon
            ESlot.HotBarUI.Icon.sprite = ESlot.ItemProperties.ItemSprite; //set the Hotbar Icon
            GameServices.Inventory.UnSelectItem();
        }
    }

    public void ResetHands(){
        GameObject PrimaryRightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
        GameObject PrimaryLeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

        GameObject OffHandRightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
        GameObject OffHandLeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

        SetActiveChildren(PrimaryRightHand.transform);
        SetActiveChildren(PrimaryLeftHand.transform);

        SetActiveChildren(OffHandRightHand.transform);
        SetActiveChildren(OffHandLeftHand.transform);
    }

    public void keepHandRotation(){
        GameObject PrimaryRightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
        GameObject PrimaryLeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

        GameObject OffHandRightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
        GameObject OffHandLeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;


        PrimaryRightHand.transform.localRotation = Quaternion.Euler(0,0,0);
        PrimaryLeftHand.transform.localRotation = Quaternion.Euler(0,0,0);

        OffHandRightHand.transform.localRotation = Quaternion.Euler(0,0,0);
        OffHandLeftHand.transform.localRotation = Quaternion.Euler(0,0,0);
    }

    private void SetActiveChildren(Transform parent, bool state = false){
        foreach(Transform child in parent)
            if (child.gameObject.activeSelf != state)
                child.gameObject.SetActive(state);
    }

    [System.Serializable]
    public class EquipmentData{
        //Dictionary for the Slots
        public Dictionary<ItemProperties.EquipSlot, EquipmentSlot> Slots = new Dictionary<ItemProperties.EquipSlot, EquipmentSlot>();

        //a list of the Slots, get smake into the Slots Dicitonary ^, only for unity serialization 
        public List<EquipmentSlot> EquipmentList;

        //convert the EquipmentList into the SlotsDictionary
        public void InitDictionary(){
            Slots.Clear();
            foreach (EquipmentSlot E in EquipmentList){
                Slots.Add(E.Slot, E);
            }
        }

    }
    [System.Serializable]
    public class EquipmentSlot{
        public ItemContainer ItemContainer; //Item In Slot
        public InventoryScript.ItemEntry ItemEntry;
        public ItemProperties ItemProperties => ItemContainer.Items.FirstOrDefault().Value;
        public Sprite DefaultIcon; //Default Icon For that Slot
        public ItemProperties.EquipSlot Slot; //the slot the Item Can be Put In
        public EquipmentEditableUI InvUI; //UI in the Inventory
        public EquipmentEditableUI HotBarUI; //UI Outside of the Inventrory
        public Color DefaultColor = Color.white;
    }
    [System.Serializable]
    public class EquipmentEditableUI{
        public Image Outline; //the square the Sprite is in
        public Image Icon; //the Item Sprite
        public CanvasGroup DurabilityUI; //the Durability Bar
        public Image DurabilityBar; //The Durability Image to edit the fillAmount of
        public TMP_Text DurabilityText; //The Text of the Durability
    }
    
}


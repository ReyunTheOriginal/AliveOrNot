using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquiptmentScript : MonoBehaviour
{
    public EquipmentData Equipment;

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
            if (E.Value.Item != null && E.Value.Item.UISlot){
                ItemProperties Properties = E.Value.Item.ItemProperties; //properties script
                ItemBehavior Behavior = E.Value.Item.ItemProperties.ItemBehavior; //behavior script that has the item's code

               //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItem(ItemProperties.EquipSlot.BothHands))
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
            if (E.Value.Item != null && E.Value.Item.UISlot){
                ItemProperties Properties = E.Value.Item.ItemProperties; //properties script
                ItemBehavior Behavior = E.Value.Item.ItemProperties.ItemBehavior; //behavior script that has the item's code

                //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItem(ItemProperties.EquipSlot.BothHands))
                            Behavior.FixedHold();//run its code
                    }else{
                        Behavior.FixedHold();//run its code
                    }
                }

            }
        }
    }

    void Update(){   
        //go through each Equipped object and run its Hold() Code, the code that runs every frame if equipped. also update Durability UI
        foreach(var E in Equipment.Slots){
            //check if the slot actually has an item
            if (HasAnItem(E.Key)){
                ItemProperties Properties = E.Value.Item.ItemProperties; //properties script
                ItemBehavior Behavior = E.Value.Item.ItemProperties.ItemBehavior; //behavior script that has the item's code

                //if there is an item in the BothHands slot don't run the other hands code
                if (Behavior){
                    if (E.Key == ItemProperties.EquipSlot.PrimaryHand || E.Key == ItemProperties.EquipSlot.OffHand){
                        if (!HasAnItem(ItemProperties.EquipSlot.BothHands))
                            Behavior.Hold();//run its code
                    }else{
                        Behavior.Hold();//run its code
                    }
                }

                if (Properties.HasDurability)
                    if (Properties.Durability <= 0){
                        UnEquip(E.Value, (int)E.Key);
                        GameServices.Inventory.RemoveItem(GameServices.Inventory.Inventory[Properties.UniqueItemID]);
                    }

                //keep the Durability UI Updated
                if (Properties.HasDurability){
                    //Update the Text
                    E.Value.InvUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("F1") + "/100";
                    E.Value.HotBarUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("F1") + "/100";

                    //Update the Bar
                    E.Value.InvUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;
                    E.Value.HotBarUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;
                }
            }
        }

        
    }

    //Runs when a Slot is Clicked
    public void EquipButton(int Slot){
            //get the Clicked Slot
            EquipmentSlot ESlot = Equipment.Slots[(ItemProperties.EquipSlot)Slot];
            

            if (HasAnItem((ItemProperties.EquipSlot)Slot)){
                int ID = ESlot.Item.ItemProperties.UniqueItemID;

                if (((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.PrimaryHand && !((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.BothHands)) || !((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.PrimaryHand) && (ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.BothHands){
                    GameServices.GlobalVariables.PrimaryHandObject.AnimationPlayer.StopAnimation();
                }else if ((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.OffHand){
                    GameServices.GlobalVariables.OffHandObject.AnimationPlayer.StopAnimation();
                }
                
                //Runs when an item is already in the slot:
                UnEquip(ESlot, Slot);

                var selected = GameServices.Inventory.SelectedItem;

                if ( 
                    selected != null &&
                    selected.ItemProperties != null &&
                    (int)selected.ItemProperties.equipSlot == Slot &&
                    selected.ItemProperties.Equippable &&
                    selected.ItemProperties.UniqueItemID != ID
                ){
                    Equip(ESlot, Slot);
                }
                
            }else{
                if (((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.PrimaryHand && !GameServices.Equipment.HasAnItem(ItemProperties.EquipSlot.BothHands)) || !((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.PrimaryHand) && !GameServices.Equipment.HasAnItem(ItemProperties.EquipSlot.PrimaryHand)){
                    GameServices.GlobalVariables.PrimaryHandObject.AnimationPlayer.StopAnimation();
                }else if ((ItemProperties.EquipSlot)Slot == ItemProperties.EquipSlot.OffHand){
                    GameServices.GlobalVariables.OffHandObject.AnimationPlayer.StopAnimation();
                }
                
                //Runs when the Slot Is Empty:
                Equip(ESlot, Slot);
            }
    }

    public bool HasAnItem(ItemProperties.EquipSlot Slot){
        EquipmentSlot ESlot = Equipment.Slots[Slot];
        return ESlot?.Item?.UISlot != null && ESlot.Item?.ItemProperties;
    }

    public void UnEquip(EquipmentSlot ESlot, int Slot){
        //reset the slot icons to the defaults
        ESlot.InvUI.Icon.sprite = ESlot.DefaultIcon;
        ESlot.HotBarUI.Icon.sprite = ESlot.DefaultIcon;
        
        ESlot.InvUI.Icon.sprite = ESlot.DefaultIcon;
        ESlot.HotBarUI.Icon.sprite = ESlot.DefaultIcon;

        //hide the Durability UI
        GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
        GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);

        //run the UnEquippted() Function
        ItemBehavior Behavior = ESlot.Item.ItemProperties.ItemBehavior;
        ESlot.Item.ItemProperties.UnSetUpHands();
        if (Behavior)Behavior.UnEquipped();

        if (Slot == (int)ItemProperties.EquipSlot.BothHands){
            if (HasAnItem(ItemProperties.EquipSlot.PrimaryHand)){
                Equipment.Slots[ItemProperties.EquipSlot.PrimaryHand].Item.ItemProperties.SetUpHands();
            }else if (HasAnItem(ItemProperties.EquipSlot.OffHand)){
                Equipment.Slots[ItemProperties.EquipSlot.OffHand].Item.ItemProperties.SetUpHands();
            }
        }

        //show the Item Inventory Slot
        ESlot.Item.UISlot.SetActive(true);

        //reset the Equipment Slot Item
        ESlot.Item = null;
    }

    public void Equip(EquipmentSlot ESlot, int Slot){
        //Check if an Object is Selected
        if (GameServices.Inventory.SelectedItem != null && GameServices.Inventory.SelectedItem.ItemProperties != null && (int)GameServices.Inventory.SelectedItem.ItemProperties.equipSlot == Slot && GameServices.Inventory.SelectedItem.ItemProperties.Equippable){
            //get Item Scripts
            ItemProperties Properties = GameServices.Inventory.SelectedItem.ItemProperties;
            ItemBehavior Behavior = Properties.ItemBehavior;

            //Set Up the Visual hands Positions in the Player's Hand Slots
            Properties.SetUpHands();
            if (Behavior)Behavior.Equipped();//run the Equippted() Function
            
            //set the Slot Item to the Selected Item
            ESlot.Item = GameServices.Inventory.SelectedItem;

            //Handle Durability UI
            if (ESlot.Item.ItemProperties.HasDurability){
                //Show Durability UI
                GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", true);
                GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", true);
            }else{
                //Hide Durability UI
                GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
                GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);
            }

            //Set up UI
            ESlot.InvUI.Icon.sprite = ESlot.Item.ItemProperties.ItemSprite; //set the Inventory Icon
            ESlot.HotBarUI.Icon.sprite = ESlot.Item.ItemProperties.ItemSprite; //set the Hotbar Icon
            GameServices.Inventory.SelectedItem.UISlot.SetActive(false); //Hide the Inventory Item Slot
        }
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
        public InventoryScript.Item Item; //Item In Slot
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


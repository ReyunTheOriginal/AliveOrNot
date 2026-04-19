using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryScript : ItemContainer
{
    public Dictionary<int, ItemEntry> ItemAddOns = new Dictionary<int, ItemEntry>();
    public ItemEntry SelectedItem; //the Currently Selected ItemEntry, Selected by clicking on its inventory slot
    public CanvasGroup FullInventoryUI; //the Whole Items Menu
    public List<CanvasGroup> UnderInventoryUI; //the UI that the Items should close before Opening
    public ItemInfoUI itemInfoUI; //the Window that Shows the ItemEntry Information such as Name, Description, and uses
    public GameObject SlotPrefab; //a prefab of the Slot an ItemEntry is Displayed in, in the inventory
    public Transform ContentTransform; //the parent the Slots Should be Children Of
    public Coroutine PickUpAnimationCoroutine;
    public float DistanceForPickup;
    public ItemProperties ClosestItem;
    public float DistanceAwayFromClosestItem;
    public RectTransform ClickIndicator;
    public ImmediateInfoUI ImmediateInfo;

    public void OnInputEdit(){
        if (int.TryParse(itemInfoUI.InputField.text, out int value)){
            itemInfoUI.DropInt = value;
        }
        if (itemInfoUI.DropInt == 0){
            itemInfoUI.InputField.text = "1";
            itemInfoUI.DropInt = 1;
        }
    }
    public void ConfirmDrop(){
        DropItem(SelectedItem, itemInfoUI.DropInt);
    }
    public void DropButton(){
        GameServices.UI.ToggleCanvasGroup(false, itemInfoUI.ConfirmationMenu, "Menu");
    }
    public void UseButton(){
        if (SelectedItem != null){
            SelectedItem.ItemProperties.ItemBehavior.Use();
            if (SelectedItem.ItemProperties.Consumable)ChangeItemAmount(SelectedItem.ItemProperties, -1);
        }
    }

    private void Awake() {
        GameServices.Inventory = this;
        PickUpAnimationCoroutine = null;
    }

    void Update()
    {   
       CheckForClosestItem();

        //toggle InventoryUI
        if (Input.GetKeyDown(KeyCode.Tab)){
            UnSelectItem();
            GameServices.UI.ToggleCanvasGroup(true, FullInventoryUI, "Items");
            foreach (CanvasGroup Group in UnderInventoryUI){
                GameServices.UI.ToggleCanvasGroup(false, Group, "");
            }
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.ConfirmationMenu, "Menu", false);
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo", false);
            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Slot.Value.DefaultColor;
            }
        }

        if (PickUpAnimationCoroutine != null && GameServices.GlobalVariables.Player.rig.velocity.magnitude > 4.2f){
            StopCoroutine(PickUpAnimationCoroutine);
            GameServices.GlobalVariables.Player.Animator.SetBool("PickingUp", false);
            PickUpAnimationCoroutine = null;
        }

        if (!GameServices.UI.AMenuIsOpened()){
             if (DistanceAwayFromClosestItem <= DistanceForPickup){
                if (!ClickIndicator.gameObject.activeSelf) ClickIndicator.gameObject.SetActive(true);
                ClickIndicator.position = ClosestItem.transform.position + new Vector3(0, 0.5f, 0);
            }else{
                if (ClickIndicator.gameObject.activeSelf) ClickIndicator.gameObject.SetActive(false);
            }

            //picks up items when Close Enough and Clicked E
            if (DistanceAwayFromClosestItem <= DistanceForPickup && PickUpAnimationCoroutine == null && Input.GetKeyDown(KeyCode.E)){
                if(ClosestItem)PickUpAnimationCoroutine = StartCoroutine(PickUpItem(ClosestItem));
            }
        }else{
            if (ClickIndicator.gameObject.activeSelf) ClickIndicator.gameObject.SetActive(false);
        }
        
        //if you click outside of UI, Unselect the ItemEntry
        if (GameServices.UI.IsActiveCanvasGroup(FullInventoryUI)){
            if (Input.GetMouseButtonDown(0)){
                if (!EventSystem.current.IsPointerOverGameObject()){
                    UnSelectItem();
                }
            }
        }

        bool HoveringOverAnySlots = false;

        foreach(var Item in ItemAddOns){
            //attach the inactive item to the player
            if (Item.Value.ItemProperties.gameObject && !GameServices.Equipment.HasItemEquipped(Item.Value.ItemProperties.ItemInstanceID))
                Item.Value.ItemProperties.gameObject.transform.position = transform.position;

            //Update Durability UI
            if (Item.Value.ItemProperties.HasDurability && Item.Value.UISlot){
                Item.Value.DurabilityText.text = (Item.Value.ItemProperties.Durability/Item.Value.ItemProperties.MaxDurability *100).ToString("0") + "/100";
                Item.Value.DurabilityBar.fillAmount = Item.Value.ItemProperties.Durability/Item.Value.ItemProperties.MaxDurability;
            }

            if (Item.Value.hoverDetector.isHovering){
                HoveringOverAnySlots = true;

                if (!GameServices.UI.IsActiveCanvasGroup(ImmediateInfo.WholeUI))
                    GameServices.UI.SetActiveCanvasGroup(false, ImmediateInfo.WholeUI, "", true, true);

                ImmediateInfo.rect.position = Input.mousePosition;

                string FinalText = Item.Value?.ItemProperties.ItemName + "\n";

                if (Item.Value?.ItemProperties.ArmorValue != 0) 
                    FinalText += $"<size=180%><sprite name=\"Armor\"></size>: {Item.Value.ItemProperties.ArmorValue}\n";

                if (Item.Value?.ItemProperties.Damage != 0) 
                    FinalText += $"<size=180%><sprite name=\"Damage\"></size>: {Item.Value.ItemProperties.Damage}\n";

                if (Item.Value?.ItemProperties.KnockBack != 0) 
                    FinalText += $"<size=180%><sprite name=\"KnockBack\"></size>: {Item.Value.ItemProperties.KnockBack}\n";
                    
                if (Item.Value?.ItemProperties.AttackSpeed != 0) 
                    FinalText += "<size=180%><sprite name=\"AttackSpeed\"></size>: " + Item.Value.ItemProperties.AttackSpeed.ToString("F2") + "\n";
                
                if (Item.Value?.ItemProperties.Durability != 0) 
                    FinalText += $"<size=180%><sprite name=\"Durability\"></size>: {Item.Value.ItemProperties.MaxDurability}\n";

                ImmediateInfo.text.text = FinalText;
            }
        }

        if (!HoveringOverAnySlots)
            GameServices.UI.SetActiveCanvasGroup(false, ImmediateInfo.WholeUI, "", false, true);
    }

    public void CheckForClosestItem(){
        float CurrentDistance = float.MaxValue;
        ItemProperties CurrentItem = null;
        foreach (ItemProperties item in GameServices.GlobalVariables.AllItems){
            if (item.enabled){
                float DistanceAttempt = Vector2.Distance(item.transform.position, transform.position);
                if (DistanceAttempt < CurrentDistance){
                    CurrentDistance = DistanceAttempt;
                    CurrentItem = item;
                }
            }
        }
        ClosestItem = CurrentItem;
        DistanceAwayFromClosestItem = CurrentDistance;
    }

    public void UnSelectItem(){
        GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo", false);
        
        SelectedItem = null;
        foreach(var Slot in GameServices.Equipment.Equipment.Slots){
            Slot.Value.InvUI.Outline.color = Slot.Value.DefaultColor;
        }
    }

    public IEnumerator PickUpItem(ItemProperties ItemToPick){
        GameServices.GlobalVariables.Player.Animator.SetBool("PickingUp", true);

        yield return new WaitForSeconds(0.2f);
        AddItem(ItemToPick);

        GameServices.GlobalVariables.Player.Animator.SetBool("PickingUp", false);
        PickUpAnimationCoroutine = null;
        yield return null;
    }

    public override void OnItemsChange(){
        foreach(var entry in ItemAddOns){
            Destroy(entry.Value.UISlot);
        }
        ItemAddOns.Clear();
        
        foreach(var entry in Items){
            ItemProperties properties = entry.Value;
                //runs if it's a new item;

                //create a new ItemEntry entry
                ItemEntry NewItem = new ItemEntry();

                //create the new UI slot;
                GameObject NewSlot = Instantiate(SlotPrefab, ContentTransform);

                //get the components to edit;
                Image ItemIcon = NewSlot.transform.GetChild(0).GetComponent<Image>();
                TMP_Text NumberText = NewSlot.transform.GetChild(1).GetComponent<TMP_Text>();
                Button SlotButton = NewSlot.GetComponent<Button>();
                HoverDetector Hover = NewSlot.GetComponent<HoverDetector>();

                //edit the components
                ItemIcon.sprite = properties.ItemSprite; //edit the slot Icon
                NumberText.text = "X" + properties.Amount.ToString(); //Edit the Text for the amount
                SlotButton.onClick.AddListener(() => SlotClick(properties.ItemInstanceID)); //Make the Slot Work when Clicked, Runs: SlotClick(ID)

                //save the data to the ItemEntry class to enter
                NewItem.ItemProperties = properties;
                NewItem.UIIcon = ItemIcon;
                NewItem.UIButton = SlotButton;
                NewItem.UINumberText = NumberText;
                NewItem.UIButton = SlotButton;
                NewItem.UISlot = NewSlot;
                NewItem.hoverDetector = Hover;

                if (properties.HasDurability){
                    NewItem.DurabilityUI = NewSlot.transform.GetChild(2).gameObject;
                    NewItem.DurabilityBar = NewSlot.transform.GetChild(2).GetChild(1).GetComponent<Image>();
                    NewItem.DurabilityText = NewSlot.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>();
                }else{
                    Destroy(NewItem.DurabilityUI = NewSlot.transform.GetChild(2).gameObject);
                }

                //deactivate the scene item GameObject;
                properties.gameObject.SetActive(false);

                if (properties.Unstackable) NewItem.UINumberText.gameObject.SetActive(false);

                //add to the Items Data
                ItemAddOns.Add(properties.ItemInstanceID, NewItem);
        }
    }

    public void SlotClick(int ItemInstanceID){
        if (Items.ContainsKey(ItemInstanceID)){
            if (itemInfoUI.DropInt == 0){
                itemInfoUI.InputField.text = "1";
                itemInfoUI.DropInt = 1;
            }

            //assign the slot to the Selected ItemEntry Variable
            SelectedItem = ItemAddOns[ItemInstanceID]; 

            //Reset the Equipment Outlines to White
            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Slot.Value.DefaultColor;
            }

            //HighLight the Slot that the selected ItemEntry can be Equipt in
            if (SelectedItem.ItemProperties.Equippable){
                GameServices.Equipment.Equipment.Slots[SelectedItem.ItemProperties.equipSlot].InvUI.Outline.color = Color.green;
            }

            //show the ItemInfo Window
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo");

            //Edit the ItemInfo Window
            itemInfoUI.Name.fontSize = Items[ItemInstanceID].ItemName.Length / 0.31f;
            itemInfoUI.Name.text = Items[ItemInstanceID].ItemName;
            itemInfoUI.Description.text = Items[ItemInstanceID].Description;
            itemInfoUI.UseButtonText.text = Items[ItemInstanceID].UseLabel;

            //Decide whether to show the Use Button
            if (!Items[ItemInstanceID].Useable){
                if (itemInfoUI.UseButton.gameObject.activeSelf)
                    itemInfoUI.UseButton.gameObject.SetActive(false);
            }else{
                if (!itemInfoUI.UseButton.gameObject.activeSelf)
                    itemInfoUI.UseButton.gameObject.SetActive(true);
            }
        }
    }

    public void DropItem(ItemEntry ItemEntry, int Amount){
        if (Items.ContainsKey(ItemEntry.ItemProperties.ItemInstanceID)){
           if (ItemEntry.ItemProperties.Amount - Amount > 0){
                //if the item amount won't end up being 0 or less

                GameObject NewItemDropped = Instantiate(ItemEntry.ItemProperties.gameObject);
                NewItemDropped.transform.position = transform.position;
                NewItemDropped.transform.rotation = Quaternion.Euler(0,0,0);
                NewItemDropped.SetActive(true);

                ItemProperties properties = NewItemDropped.GetComponent<ItemProperties>();
                
                properties.Amount = Amount;
                ItemEntry.ItemProperties.Amount -= Amount;
                ItemEntry.UINumberText.text = "X" + ItemEntry.ItemProperties.Amount.ToString();
            }else{
                //if the item amount will end up being 0 or less

                GameObject NewItemDropped = Instantiate(ItemEntry.ItemProperties.gameObject);
                NewItemDropped.transform.position = transform.position;
                NewItemDropped.transform.rotation = Quaternion.Euler(0,0,0);
                NewItemDropped.SetActive(true);

                ItemProperties properties = NewItemDropped.GetComponent<ItemProperties>();
                properties.Amount = ItemEntry.ItemProperties.Amount;

                RemoveItem(ItemEntry.ItemProperties);
            }
        }
    }
    

    [System.Serializable]
    public class ItemEntry{
        public int InstanceID;
        public ItemProperties ItemProperties;
        public Image UIIcon;
        public Button UIButton;
        public TMP_Text UINumberText;
        public GameObject UISlot;
        public HoverDetector hoverDetector;
        public GameObject DurabilityUI;
        public Image DurabilityBar;
        public TMP_Text DurabilityText;
    }
    [System.Serializable]
    public class ItemInfoUI{
        public CanvasGroup WholeUI;
        public CanvasGroup ConfirmationMenu;
        public TMP_Text Name;
        public TMP_Text Description;
        public Button UseButton;
        public TMP_Text UseButtonText;
        public Button DropButton;
        public Button ConfirmDropButton;
        public TMP_InputField InputField;
        public int DropInt;
    }
   [System.Serializable]
   public class ImmediateInfoUI{
        public CanvasGroup WholeUI;
        public RectTransform rect;
        public TMP_Text text;
   } 
}

using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScript : MonoBehaviour
{
    public Dictionary<int, Item> Inventory = new Dictionary<int, Item>(); //the Dictionary of the Inventory's Items
    public Item SelectedItem; //the Currently Selected Item, Selected by clicking on its inventory slot
    public CanvasGroup FullInventoryUI; //the Whole Inventory Menu
    public List<CanvasGroup> UnderInventoryUI; //the UI that the Inventory should close before Opening
    public ItemInfoUI itemInfoUI; //the Window that Shows the Item Information such as Name, Description, and uses
    public GameObject SlotPrefab; //a prefab of the Slot an Item is Displayed in, in the inventory
    public Transform ContentTransform; //the parent the Slots Should be Children Of

    public void OnInputEdit(){
        if (int.TryParse(itemInfoUI.InputField.text, out int value)){
            itemInfoUI.DropInt = value;
        }
    }
    public void ConfirmDrop(){
        DropItem(SelectedItem, itemInfoUI.DropInt);
    }
    public void DropButton(){
        GameServices.UI.ToggleUI(false, itemInfoUI.ConfirmationMenu, "Menu");
    }
    public void UseButton(){
        if (SelectedItem != null){
            SelectedItem.ItemProperties.ItemBehavior.Use();
            if (SelectedItem.ItemProperties.Consumable)ChangeItemAmount(SelectedItem, -1);
        }
    }

    private void Awake() {
        GameServices.Inventory = this;
    }

    void Update()
    {   
        //toggle InventoryUI
        if (Input.GetKeyDown(KeyCode.E)){
            GameServices.UI.ToggleUI(true, FullInventoryUI, "Inventory");
            foreach (CanvasGroup Group in UnderInventoryUI){
                GameServices.UI.ToggleUI(false, Group, "");
            }
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.ConfirmationMenu, "Menu", false);
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo", false);
            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Color.white;
            }
        }

        //TEMPORARY: picks up items when clicked on
        if (Input.GetMouseButtonDown(0)){
            RaycastHit2D hit = Physics2D.Raycast(GameServices.GlobalVariables.Camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider && hit.collider.CompareTag("Item")){
                ItemProperties ItemPro = hit.collider.GetComponent<ItemProperties>();

                if(ItemPro)AddItem(ItemPro);
            }
        }
        
        //if you click outside of UI, Unselect the Item
        if (GameServices.UI.IsActive(FullInventoryUI)){
            if (Input.GetMouseButtonDown(0)){
                if (!EventSystem.current.IsPointerOverGameObject()){
                    GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.ConfirmationMenu, "Menu", false);
                    GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo", false);
                    
                    SelectedItem = null;
                    foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                        Slot.Value.InvUI.Outline.color = Color.white;
                    }
                }
            }
        }

        //Make the Item GameObjects Follow the Player instead of being stationary
        foreach(var Item in Inventory){
            Item.Value.GameObject.transform.position = transform.position;
        }
    }

    public Item AddItem(ItemProperties properties){
        if (!Inventory.ContainsKey(properties.ID)){
            //runs if it's a new item;

            //create a new Item entry
            Item NewItem = new Item();

            //create the new UI slot;
            GameObject NewSlot = Instantiate(SlotPrefab, ContentTransform);

            //get the components to edit;
            Image ItemIcon = NewSlot.transform.GetChild(0).GetComponent<Image>();
            TMP_Text NumberText = NewSlot.transform.GetChild(1).GetComponent<TMP_Text>();
            Button SlotButton = NewSlot.GetComponent<Button>();

            //edit the components
            ItemIcon.sprite = properties.ItemSprite; //edit the slot Icon
            NumberText.text = "X" + properties.Amount.ToString(); //Edit the Text for the amount
            SlotButton.onClick.AddListener(() => SlotClick(properties.ID)); //Make the Slot Work when Clicked, Runs: SlotClick(ID)

            //save the data to the Item class to enter
            NewItem.Amount = properties.Amount;
            NewItem.ItemProperties = properties;
            NewItem.UIIcon = ItemIcon;
            NewItem.UIButton = SlotButton;
            NewItem.UINumberText = NumberText;
            NewItem.UIButton = SlotButton;
            NewItem.UISlot = NewSlot;
            NewItem.GameObject = properties.gameObject;

            //deactivate the scene item GameObject;
            properties.gameObject.SetActive(false);

            //add to the Inventory Data
            Inventory.Add(properties.ID, NewItem);
            return NewItem;
        }else{
            //runs if the item type already exists;

            //Destroy the scene object of the item
            Destroy(properties.gameObject);
            
            //add the amount to the already existing item entry;
            Item AlreadyExistingItem = Inventory[properties.ID];
            AlreadyExistingItem.Amount += properties.Amount;

            //update the UI;
            AlreadyExistingItem.UINumberText.text = "X" + AlreadyExistingItem.Amount.ToString();

            return AlreadyExistingItem;
        }
    }
    public void SlotClick(int ItemID){
        if (Inventory.ContainsKey(ItemID)){
            //assign the slot to the Selected Item Variable
            SelectedItem = Inventory[ItemID]; 

            //Reset the Equipment Outlines to White
            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Color.white;
            }

            //HighLight the Slot that the selected Item can be Equipt in
            if (SelectedItem.ItemProperties.Equippable){
                GameServices.Equipment.Equipment.Slots[SelectedItem.ItemProperties.equipSlot].InvUI.Outline.color = Color.green;
            }

            //show the ItemInfo Window
            GameServices.UI.SetActiveCanvasGroup(false, itemInfoUI.WholeUI, "ItemInfo");

            //Edit the ItemInfo Window
            itemInfoUI.Name.text = Inventory[ItemID].ItemProperties.ItemName;
            itemInfoUI.Description.text = Inventory[ItemID].ItemProperties.Description;

            //Decide whether to show the Use Button
            if (!Inventory[ItemID].ItemProperties.Useable){
                if (itemInfoUI.UseButton.gameObject.activeSelf)
                    itemInfoUI.UseButton.gameObject.SetActive(false);
            }else{
                if (itemInfoUI.UseButton.gameObject.activeSelf)
                    itemInfoUI.UseButton.gameObject.SetActive(true);
            }
        }
    }
    public void DropItem(Item Item, int Amount){
        if (Inventory.ContainsKey(Item.ItemProperties.ID)){
           if (Item.Amount - Amount > 0){
                //if the item amount won't end up being 0 or less

                GameObject NewItemDropped = Instantiate(Item.GameObject);
                NewItemDropped.transform.position = transform.position;
                NewItemDropped.SetActive(true);

                ItemProperties properties = NewItemDropped.GetComponent<ItemProperties>();
                
                properties.Amount = Amount;
                Item.Amount -= Amount;
                Item.UINumberText.text = "X" + Item.Amount.ToString();
            }else{
                //if the item amount will end up being 0 or less

                GameObject NewItemDropped = Instantiate(Item.GameObject);
                NewItemDropped.transform.position = transform.position;
                NewItemDropped.SetActive(true);

                ItemProperties properties = NewItemDropped.GetComponent<ItemProperties>();
                properties.Amount = Item.Amount;

                RemoveItem(Item);
            }
        }
    }
    public void RemoveItem(Item Item){
        if (Inventory.ContainsKey(Item.ItemProperties.ID)){
            Item ItemToDestroy = Item;

            //Destroy Objects;
            Destroy(ItemToDestroy.UISlot);
            Destroy(ItemToDestroy.GameObject);
            
            //Remove Item from Inventory Data
            Inventory.Remove(Item.ItemProperties.ID);
        }
    }
    public void ChangeItemAmount(Item Item, int Amount){
        if (Inventory.ContainsKey(Item.ItemProperties.ID)){
            
            if (Item.Amount + Amount > 0){
                //if the item amount won't end up being 0 or less
                Item.Amount += Amount;

                Item.UINumberText.text = "X" + Item.Amount.ToString();
            }else{
                //if the item amount will end up being 0 or less
                RemoveItem(Item);
            }
        }
    }

    [System.Serializable]
    public class Item{
        public GameObject GameObject;
        public int Amount = 1;
        public ItemProperties ItemProperties;
        public Image UIIcon;
        public Button UIButton;
        public TMP_Text UINumberText;
        public GameObject UISlot;
    }
    [System.Serializable]
    public class ItemInfoUI{
        public CanvasGroup WholeUI;
        public CanvasGroup ConfirmationMenu;
        public TMP_Text Name;
        public TMP_Text Description;
        public Button UseButton;
        public Button DropButton;
        public Button ConfirmDropButton;
        public TMP_InputField InputField;
        public int DropInt;
    }
    
}

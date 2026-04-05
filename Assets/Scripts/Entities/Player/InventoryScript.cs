using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScript : MonoBehaviour
{
    public Dictionary<int, Item> Inventory = new Dictionary<int, Item>();
    public Item SelectedItem;
    public CanvasGroup FullInventoryUI;
    public List<CanvasGroup> UnderInventoryUI;
    public SelectedItemUI selectedItemUI;
    public GameObject SlotPrefab;
    public Transform ContentTransform;
    public Camera Cam;

    public void OnInputEdit(){
        if (int.TryParse(selectedItemUI.InputField.text, out int value)){
            selectedItemUI.DropInt = value;
        }
    }
    public void ConfirmDrop(){
        DropItem(SelectedItem, selectedItemUI.DropInt);
    }
    public void DropButton(){
        GameServices.UI.ToggleUI(false, selectedItemUI.ConfirmationMenu, "Menu");
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
        if (Input.GetKeyDown(KeyCode.E)){
            GameServices.UI.ToggleUI(true, FullInventoryUI, "Inventory");
            foreach (CanvasGroup Group in UnderInventoryUI){
                GameServices.UI.ToggleUI(false, Group, "");
            }
            GameServices.UI.SetActiveCanvasGroup(false, selectedItemUI.ConfirmationMenu, "Menu", false);
            GameServices.UI.SetActiveCanvasGroup(false, selectedItemUI.WholeUI, "ItemInfo", false);
            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Color.white;
            }
        }

        if (Input.GetMouseButtonDown(0)){
            RaycastHit2D hit = Physics2D.Raycast(Cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider && hit.collider.CompareTag("Item")){
                ItemProperties ItemPro = hit.collider.GetComponent<ItemProperties>();

                if(ItemPro)AddItem(ItemPro);
            }
        }
        
        if (GameServices.UI.IsActive(FullInventoryUI)){
            if (Input.GetMouseButtonDown(0)){
                if (!EventSystem.current.IsPointerOverGameObject()){
                    GameServices.UI.SetActiveCanvasGroup(false, selectedItemUI.ConfirmationMenu, "Menu", false);
                    GameServices.UI.SetActiveCanvasGroup(false, selectedItemUI.WholeUI, "ItemInfo", false);
                    
                    SelectedItem = null;
                    foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                        Slot.Value.InvUI.Outline.color = Color.white;
                    }
                }
            }
        }

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
            ItemIcon.sprite = properties.ItemSprite;
            NumberText.text = "X" + properties.Amount.ToString();
            SlotButton.onClick.AddListener(() => SlotClick(properties.ID));

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
            SelectedItem = Inventory[ItemID];

            foreach(var Slot in GameServices.Equipment.Equipment.Slots){
                Slot.Value.InvUI.Outline.color = Color.white;
            }

            if (SelectedItem.ItemProperties.Equippable){
                GameServices.Equipment.Equipment.Slots[SelectedItem.ItemProperties.equipSlot].InvUI.Outline.color = Color.green;
            }

            GameServices.UI.SetActiveCanvasGroup(false, selectedItemUI.WholeUI, "ItemInfo");

            selectedItemUI.Name.text = Inventory[ItemID].ItemProperties.ItemName;
            selectedItemUI.Description.text = Inventory[ItemID].ItemProperties.Description;

            if (!Inventory[ItemID].ItemProperties.Useable){
                if (selectedItemUI.UseButton.gameObject.activeSelf)
                    selectedItemUI.UseButton.gameObject.SetActive(false);
            }else{
                if (selectedItemUI.UseButton.gameObject.activeSelf)
                    selectedItemUI.UseButton.gameObject.SetActive(true);
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
    public class SelectedItemUI{
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

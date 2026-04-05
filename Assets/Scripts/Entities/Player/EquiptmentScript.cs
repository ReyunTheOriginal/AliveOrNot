using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquiptmentScript : MonoBehaviour
{
    public EquipmentData Equipment;

    void Awake(){
        GameServices.Equipment = this;
        Equipment.InitDictionary();
    }

    void Update()
    {
        foreach(var E in Equipment.Slots){
            if (E.Value.Item != null && E.Value.Item.UISlot){
                ItemProperties Properties = E.Value.Item.ItemProperties;
                ItemBehavior Behavior = E.Value.Item.ItemProperties.ItemBehavior;

                EquipmentSlot MainHand = Equipment.Slots[ItemProperties.EquipSlot.PrimaryHand];

                //if (E.Key == ItemProperties.EquipSlot.OffHand){
                    //if (MainHand.Item != null && MainHand.Item.UISlot != null && MainHand.Item.ItemProperties.equipSlot == ItemProperties.EquipSlot.BothHands)
                    if (Behavior)Behavior.Hold();
                //}

                if (Properties.HasDurability){
                    E.Value.InvUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("0") + "/100";
                    E.Value.HotBarUI.DurabilityText.text = (Properties.Durability/Properties.MaxDurability *100).ToString("0") + "/100";

                    if (Properties.Durability != 0){
                        E.Value.InvUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;
                        E.Value.HotBarUI.DurabilityBar.fillAmount = Properties.Durability/Properties.MaxDurability;
                    }else{
                        E.Value.InvUI.DurabilityBar.fillAmount = 0;
                        E.Value.HotBarUI.DurabilityBar.fillAmount = 0;
                    }
                }


            }
        }
    }

    public void EquipButton(int Slot){
            EquipmentSlot ESlot = Equipment.Slots[(ItemProperties.EquipSlot)Slot];
            
            if (ESlot.Item != null && ESlot.Item.UISlot != null){
                ESlot.InvUI.Icon.sprite = ESlot.DefaultIcon;
                ESlot.HotBarUI.Icon.sprite = ESlot.DefaultIcon;

                GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
                GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);

                ItemBehavior Behavior = ESlot.Item.ItemProperties.ItemBehavior;
                if (Behavior)Behavior.UnEquipped();

                ESlot.Item.UISlot.SetActive(true);
                ESlot.Item = null;
            }else{
                if (GameServices.Inventory.SelectedItem != null && GameServices.Inventory.SelectedItem.ItemProperties != null && (int)GameServices.Inventory.SelectedItem.ItemProperties.equipSlot == Slot && GameServices.Inventory.SelectedItem.ItemProperties.Equippable){
                    ESlot.Item = GameServices.Inventory.SelectedItem;

                    if (ESlot.Item.ItemProperties.HasDurability){
                        GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", true);
                        GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", true);
                    }else{
                        GameServices.UI.SetActiveCanvasGroup(false, ESlot.InvUI.DurabilityUI, "", false);
                        GameServices.UI.SetActiveCanvasGroup(false, ESlot.HotBarUI.DurabilityUI, "", false);
                    }

                    ItemProperties Properties = ESlot.Item.ItemProperties;
                    ItemBehavior Behavior = Properties.ItemBehavior;

                    Properties.SetUpHands();
                    if (Behavior)Behavior.Equipped();

                    ESlot.InvUI.Icon.sprite = ESlot.Item.ItemProperties.ItemSprite;
                    ESlot.HotBarUI.Icon.sprite = ESlot.Item.ItemProperties.ItemSprite;
                    GameServices.Inventory.SelectedItem.UISlot.SetActive(false);
                }
            }
    }

    [System.Serializable]
    public class EquipmentData{
        public Dictionary<ItemProperties.EquipSlot, EquipmentSlot> Slots = new Dictionary<ItemProperties.EquipSlot, EquipmentSlot>();
        
        public CanvasGroup FullUI;

        public List<EquipmentSlot> EquipmentList;

        public void InitDictionary(){
            Slots.Clear();
            foreach (EquipmentSlot E in EquipmentList){
                Slots.Add(E.Slot, E);
            }
        }

    }
    [System.Serializable]
    public class EquipmentSlot{
        public InventoryScript.Item Item;
        public Sprite DefaultIcon;
        public ItemProperties.EquipSlot Slot;
        public EquipmentEditableUI InvUI;
        public EquipmentEditableUI HotBarUI;
    }
    [System.Serializable]
    public class EquipmentEditableUI{
        public Image Outline;
        public Image Icon;
        public CanvasGroup DurabilityUI;
        public Image DurabilityBar;
        public TMP_Text DurabilityText;
    }
}


using UnityEngine;

public class ItemProperties : MonoBehaviour
{
     public static int UniqueItemIDIncr;
     [HideInInspector]public int UniqueItemID;
     public Sprite ItemSprite;
     public string ItemName;
     public string Description;
     public int ID;
     public int Amount = 1;
     public ItemBehavior ItemBehavior;
     public bool Useable;
     public bool Equippable;
     public EquipSlot equipSlot;
     [Space]
     public Vector2 RightHandPosition;
     public Vector2 LeftHandPosition;
     public bool UsesRightHand;
     public bool UsesLeftHand;
     [Space]
     public float MaxDurability;
     public float Durability;
     public bool HasDurability;
     [Space]
     public bool Unstackable;
     public bool Consumable;
     [HideInInspector] public float Damage = 0;
     [HideInInspector] public float ArmorValue = 0;
     [HideInInspector] public float AttackSpeed = 0;
     [HideInInspector] public float Accuracy = 0;

     private void Awake() {
          if (ItemBehavior)ItemBehavior.Properties = this;

          UniqueItemID = UniqueItemIDIncr;
          UniqueItemIDIncr++;

          if (!Unstackable && HasDurability){
               Debug.LogWarning("Item does not have compatible Unstackable and Durability Settings: " + ItemName);
          }
     }

     private void Start() {
          GameServices.GlobalVariables.AllItems.Add(this);
     }

     private void OnDestroy() {
          GameServices.GlobalVariables.AllItems.Remove(this);
     }

     public void SetUpHands(){

          foreach(Transform upperChild in GameServices.GlobalVariables.OffHandObject.Object.transform){
               foreach (Transform Prop in upperChild){
                    if (Prop.gameObject.activeSelf) Prop.gameObject.SetActive(false);
               }
          }

          foreach(Transform upperChild in GameServices.GlobalVariables.PrimaryHandObject.Object.transform){
               foreach (Transform Prop in upperChild){
                    if (Prop.gameObject.activeSelf) Prop.gameObject.SetActive(false);
               }
          }
          
          if ((equipSlot == EquipSlot.PrimaryHand || equipSlot == EquipSlot.BothHands) && !GameServices.Equipment.HasAnItem(EquipSlot.BothHands)){
               GameServices.GlobalVariables.PrimaryHandObject.Object.SetActive(true);

               GameServices.GlobalVariables.PrimaryHandObject.Object.transform.localScale = transform.localScale;

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.PrimaryHandObject.ObjectRenderer;

               HandRen.sprite = ItemSprite;

               
               if (UsesRightHand){
                    if (!RightHand.activeSelf)RightHand.SetActive(true);
                    RightHand.transform.localPosition = RightHandPosition;
               }else{
                    if (RightHand.activeSelf)RightHand.SetActive(false);
               }

               if (UsesLeftHand){
                    if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                    LeftHand.transform.localPosition = LeftHandPosition;
               }else{
                    if (RightHand.activeSelf)LeftHand.SetActive(false);
               }

               if (equipSlot == EquipSlot.BothHands){
                    GameServices.GlobalVariables.OffHandObject.Object.SetActive(false);
                    GameServices.GlobalVariables.OffHandObject.Object.transform.localScale = new Vector3(2,2,1);
               }
          }

          if (equipSlot == EquipSlot.OffHand && !GameServices.Equipment.HasAnItem(EquipSlot.BothHands)){
               GameServices.GlobalVariables.OffHandObject.Object.SetActive(true);

               GameServices.GlobalVariables.OffHandObject.Object.transform.localScale = transform.localScale;

               GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.OffHandObject.ObjectRenderer;

               HandRen.sprite = ItemSprite;

               
               if (UsesRightHand){
                    if (!RightHand.activeSelf)RightHand.SetActive(true);
                    RightHand.transform.localPosition = RightHandPosition;
               }else{
                    RightHand.SetActive(false);
               }

               if (UsesLeftHand){
                    if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                    LeftHand.transform.localPosition = LeftHandPosition;
               }else{
                    LeftHand.SetActive(false);
               }
          }
     
          if (equipSlot == EquipSlot.BothHands){
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].InvUI.Outline.color = Color.black;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].InvUI.Outline.color = Color.black;
          }
     }

     public void UnSetUpHands(){
          if ((equipSlot == EquipSlot.PrimaryHand && !GameServices.Equipment.HasAnItem(EquipSlot.BothHands)) || (equipSlot == EquipSlot.BothHands && !GameServices.Equipment.HasAnItem(EquipSlot.PrimaryHand))){
               GameServices.GlobalVariables.PrimaryHandObject.Object.SetActive(false);
               GameServices.GlobalVariables.PrimaryHandObject.Object.transform.localScale = new Vector3(2,2,1);
          }

          if (equipSlot == EquipSlot.OffHand){
               GameServices.GlobalVariables.OffHandObject.Object.SetActive(false);
               GameServices.GlobalVariables.OffHandObject.Object.transform.localScale = new Vector3(2,2,1);
          }

          if (equipSlot == EquipSlot.BothHands && GameServices.Equipment.HasAnItem(EquipSlot.PrimaryHand)){
               GameServices.GlobalVariables.PrimaryHandObject.Object.SetActive(true);

               GameServices.GlobalVariables.PrimaryHandObject.Object.transform.localScale = GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.transform.localScale;

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.PrimaryHandObject.ObjectRenderer;

               HandRen.sprite = GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.ItemSprite;
               
               if (GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.UsesRightHand){
                    if (!RightHand.activeSelf)RightHand.SetActive(true);
                    RightHand.transform.localPosition = GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.RightHandPosition;
               }else{
                    if (RightHand.activeSelf)RightHand.SetActive(false);
               }

               if (GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.UsesLeftHand){
                    if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                    LeftHand.transform.localPosition = GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.LeftHandPosition;
               }else{
                    if (RightHand.activeSelf)LeftHand.SetActive(false);
               }

               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.ItemBehavior?.Equipped();
          }

          if (equipSlot == EquipSlot.BothHands && GameServices.Equipment.HasAnItem(EquipSlot.OffHand)){
               GameServices.GlobalVariables.OffHandObject.Object.SetActive(true);

               GameServices.GlobalVariables.OffHandObject.Object.transform.localScale = GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.transform.localScale;

               GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.OffHandObject.ObjectRenderer;

               HandRen.sprite = GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.ItemSprite;
               
               if (GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.UsesRightHand){
                    if (!RightHand.activeSelf)RightHand.SetActive(true);
                    RightHand.transform.localPosition = GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.RightHandPosition;
               }else{
                    RightHand.SetActive(false);
               }

               if (GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.UsesLeftHand){
                    if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                    LeftHand.transform.localPosition = GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.LeftHandPosition;
               }else{
                    LeftHand.SetActive(false);
               }

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.ItemBehavior?.Equipped();
          }
          
          if (equipSlot == EquipSlot.BothHands){
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].DefaultColor = Color.white;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].HotBarUI.Outline.color = Color.white;
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].InvUI.Outline.color = Color.white;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].DefaultColor = Color.white;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].HotBarUI.Outline.color = Color.white;
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].InvUI.Outline.color = Color.white;

          }
     }

     public enum EquipSlot{
          Head,
          Torso,
          Feet,
          PrimaryHand,
          OffHand,
          BothHands
     }

}

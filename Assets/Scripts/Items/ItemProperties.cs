using UnityEngine;

public class ItemProperties : MonoBehaviour
{
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
     public float DistanceFromPlayer;
     [Space]
     public float MaxDurability;
     public float Durability;
     public bool HasDurability;

     public bool Unstackable;
     public bool Consumable;

     private void Awake() {
          if (ItemBehavior)ItemBehavior.Properties = this;
     }

     public void SetUpHands(){
          if (equipSlot == EquipSlot.PrimaryHand){
               GameServices.GlobalVariables.PrimaryHandObject.Object.SetActive(true);

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.PrimaryHandObject.ObjectRenderer;

               HandRen.sprite = ItemSprite;
               
               if (UsesRightHand){
                    RightHand.transform.localPosition = RightHandPosition;
               }else{
                    RightHand.SetActive(false);
               }

               if (UsesLeftHand){
                    LeftHand.transform.localPosition = LeftHandPosition;
               }else{
                    LeftHand.SetActive(false);
               }
          }

          if (equipSlot == EquipSlot.OffHand){
               GameServices.GlobalVariables.OffHandObject.Object.SetActive(true);

               GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

               SpriteRenderer HandRen = GameServices.GlobalVariables.OffHandObject.ObjectRenderer;

               HandRen.sprite = ItemSprite;
               
               if (UsesRightHand){
                    RightHand.transform.localPosition = RightHandPosition;
               }else{
                    RightHand.SetActive(false);
               }

               if (UsesLeftHand){
                    LeftHand.transform.localPosition = LeftHandPosition;
               }else{
                    LeftHand.SetActive(false);
               }
          }
     }

     public enum EquipSlot{
          Head,
          Torso,
          Feet,
          PrimaryHand,
          OffHand
     }

}

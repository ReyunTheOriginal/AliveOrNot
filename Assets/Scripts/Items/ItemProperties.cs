using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ItemProperties))]
public class ItemPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemProperties item = (ItemProperties)target;

        if (GUILayout.Button("Generate Unique ID")){
          int newID = EditorPrefs.GetInt("ItemIDCounter", 1);
          item.ID = newID;
          EditorPrefs.SetInt("ItemIDCounter", newID + 1);

          EditorUtility.SetDirty(item);
          AssetDatabase.SaveAssets();
          Debug.Log($"Assigned ID {item.ID} to {item.name}");
     }

        DrawDefaultInspector();
    }
}
#endif

public class ItemProperties : MonoBehaviour
{
     public static int ItemInstanceIDIncr;
///////////////////////////////////////////////////////////
     public string ItemName;
     public int ID;
     public int Amount = 1;
[Header("UI")]
     public string Description;
     public string UseLabel = "Use";
     public Sprite ItemSprite;
     public GameObject CustomUIPrefab;
[Header("Components")]
     public ItemBehavior ItemBehavior;
     public SpriteRenderer ItemRenderer;
     public AnimationPlayer AnimationPlayer;
[Header("Hands")]
     public Vector2 LocalPosition = new Vector2(1f, 0);
     public bool UsesRightHand;
     public bool UsesLeftHand;
     public Vector2 RightHandPosition;
     public Vector2 LeftHandPosition;
[Header("Durability")]
     public float MaxDurability;
     public float Durability;
     public bool HasDurability;
[Header("Special Effects")]
     public bool Useable;
     public bool Unstackable;
     public bool Consumable;
     public bool Equippable;
     public EquipSlot equipSlot;

     [HideInInspector]public int ItemInstanceID;
     [HideInInspector]public CanvasGroup CustomUICanvasGroup;
     [HideInInspector] public float Damage = 0;
     [HideInInspector] public float ArmorValue = 0;
     [HideInInspector] public float AttackSpeed = 0;
     [HideInInspector] public float KnockBack = 0;

     private void OnValidate() {
          if (!AnimationPlayer)AnimationPlayer = GetComponent<AnimationPlayer>();
          if (!ItemRenderer)ItemRenderer = GetComponent<SpriteRenderer>();
     }

     private void Awake() {
          if (ItemBehavior)ItemBehavior.Properties = this;

          ItemInstanceID = ItemInstanceIDIncr;
          ItemInstanceIDIncr++;

          if (!Unstackable && HasDurability)
               Debug.LogWarning("Item does not have compatible Unstackable and Durability Settings: " + ItemName);
     }

     private void Start() {
          if (!GameServices.GlobalVariables.AllItems.Contains(this))
               GameServices.GlobalVariables.AllItems.Add(this);
     }

     private void OnEnable() {

          if (GameServices.GlobalVariables)
               if (GameServices.GlobalVariables.AllItems.Contains(this))
                         GameServices.GlobalVariables.AllItems.Remove(this);
     }

     private void OnDisable() {
          if (GameServices.GlobalVariables.AllItems.Contains(this))
               GameServices.GlobalVariables.AllItems.Remove(this);
     }

     private void OnDestroy() {
          if (GameServices.GlobalVariables.AllItems.Contains(this))
               GameServices.GlobalVariables.AllItems.Remove(this);
     }

     public void SetUpHands(){

          if(CustomUIPrefab)
               CustomUICanvasGroup = Instantiate(CustomUIPrefab, GameServices.GlobalVariables.CustomItemUICanvasGroup.transform).GetComponent<CanvasGroup>();
          
          if (equipSlot == EquipSlot.PrimaryHand){
               transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);

               if (!GameServices.Equipment.HasAnItemInSlot(EquipSlot.BothHands)){
                    gameObject.SetActive(true);

                    GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
                    GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

                    RightHand.transform.SetParent(transform);
                    LeftHand.transform.SetParent(transform);
                    
                    //setting Up Hands
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
                    ///////
               }else{ 
                    gameObject.SetActive(false);
               }
          }

          if (equipSlot == EquipSlot.OffHand){
               transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);

               if (!GameServices.Equipment.HasAnItemInSlot(EquipSlot.BothHands)){
                    gameObject.SetActive(true);

                    GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
                    GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

                    RightHand.transform.SetParent(transform);
                    LeftHand.transform.SetParent(transform);
                    
                    //setting Up Hands
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
                    ///////
               }else{
                    gameObject.SetActive(false);
               }
          }
     
          if (equipSlot == EquipSlot.BothHands){
               gameObject.SetActive(true);

               transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               RightHand.transform.SetParent(transform);
               LeftHand.transform.SetParent(transform);
               
               //setting Up Hands
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
               ///////

               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.OffHand))
                    GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.GameObject?.SetActive(false);

               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.PrimaryHand))
                    GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.GameObject?.SetActive(false);

               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].InvUI.Outline.color = Color.black;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].InvUI.Outline.color = Color.black;
          }

          transform.localPosition = LocalPosition;
     }

     public void UnSetUpHands(){
          transform.parent = null;

          if (CustomUICanvasGroup)
            Destroy(CustomUICanvasGroup.gameObject);

          if (equipSlot == EquipSlot.PrimaryHand){
               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               RightHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
               LeftHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
          
               if (RightHand.activeSelf)RightHand.SetActive(false);
               if (LeftHand.activeSelf)LeftHand.SetActive(false);
          }

          if (equipSlot == EquipSlot.OffHand){
               GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

               RightHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);
               LeftHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);
          
               if (RightHand.activeSelf)RightHand.SetActive(false);
               if (LeftHand.activeSelf)LeftHand.SetActive(false);
          }
          
          if (equipSlot == EquipSlot.BothHands){

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               RightHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
               LeftHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);

               if (RightHand.activeSelf)RightHand.SetActive(false);
               if (LeftHand.activeSelf)LeftHand.SetActive(false);

               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].DefaultColor = Color.white;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].HotBarUI.Outline.color = Color.white;
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].InvUI.Outline.color = Color.white;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].DefaultColor = Color.white;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].HotBarUI.Outline.color = Color.white;
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].InvUI.Outline.color = Color.white;

               if(GameServices.Equipment.HasAnItemInSlot(EquipSlot.PrimaryHand)){
                    RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
                    LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

                    RightHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
                    LeftHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);

                    GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.GameObject.SetActive(true);
                    
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

                    //GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].Item.ItemProperties.ItemBehavior?.Equipped();
               }


               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.OffHand)){
                    RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
                    LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

                    RightHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);
                    LeftHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);

                    GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.GameObject.SetActive(true);
                    
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

                    //GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].Item.ItemProperties.ItemBehavior?.Equipped();
               }

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

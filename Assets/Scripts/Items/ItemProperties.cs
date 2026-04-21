using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ItemProperties)), CanEditMultipleObjects]
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
     public GameObject Shadow;
[Header("UI")]
     public string Description;
     public string UseLabel = "Use";
     public Sprite ItemSprite;
     public GameObject CustomUIPrefab;
[Header("Components")]
     public ItemBehavior ItemBehavior;
     public SpriteRenderer ItemRenderer;
     public AnimationPlayer AnimationPlayer;
     public SpriteSorter SpriteSorter;
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
          if (!ItemBehavior)ItemBehavior = GetComponent<ItemBehavior>();
          if (!SpriteSorter)SpriteSorter = GetComponent<SpriteSorter>();
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

     private void OnDestroy() {
          if (GameServices.GlobalVariables.AllItems.Contains(this))
               GameServices.GlobalVariables.AllItems.Remove(this);
     }

     public void SetUpItem(){
          if (Shadow)
               if (Shadow.activeSelf) Shadow.SetActive(false);

          if(CustomUIPrefab)
               CustomUICanvasGroup = Instantiate(CustomUIPrefab, GameServices.GlobalVariables.CustomItemUICanvasGroup.transform).GetComponent<CanvasGroup>();
          
          if (equipSlot == EquipSlot.PrimaryHand){
               if (!gameObject.activeSelf)gameObject.SetActive(true);
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

                         GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = SpriteSorter;
                    }else{
                         if (RightHand.activeSelf)RightHand.SetActive(false);
                         GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = null;
                    }

                    if (UsesLeftHand){
                         if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                         LeftHand.transform.localPosition = LeftHandPosition;

                         GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = SpriteSorter;
                    }else{
                         if (RightHand.activeSelf)LeftHand.SetActive(false);
                         GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = null;
                    }
                    ///////
               }else{ 
                    gameObject.SetActive(false);
               }
          }else if (equipSlot == EquipSlot.OffHand){
               if (!gameObject.activeSelf)gameObject.SetActive(true);
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

                         GameServices.GlobalVariables.OffHandObject.RightHandSorter.AlwaysOnTopOf = SpriteSorter;
                    }else{
                         RightHand.SetActive(false);

                         GameServices.GlobalVariables.OffHandObject.RightHandSorter.AlwaysOnTopOf = null;
                    }

                    if (UsesLeftHand){
                         if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                         LeftHand.transform.localPosition = LeftHandPosition;

                         GameServices.GlobalVariables.OffHandObject.LeftHandSorter.AlwaysOnTopOf = SpriteSorter;
                    }else{
                         LeftHand.SetActive(false);

                         GameServices.GlobalVariables.OffHandObject.LeftHandSorter.AlwaysOnTopOf = null;
                    }
                    ///////
               }else{
                    gameObject.SetActive(false);
               }
          }else if (equipSlot == EquipSlot.BothHands){
               if (!gameObject.activeSelf)gameObject.SetActive(true);
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

                    GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = SpriteSorter;
               }else{
                    if (RightHand.activeSelf)RightHand.SetActive(false);
                    GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = null;
               }

               if (UsesLeftHand){
                    if (!LeftHand.activeSelf)LeftHand.SetActive(true);
                    LeftHand.transform.localPosition = LeftHandPosition;

                    GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = SpriteSorter;
               }else{
                    if (RightHand.activeSelf)LeftHand.SetActive(false);
                    GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = null;
               }
               ///////

               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.OffHand)){
                   GameServices.Equipment.GetItemInSlot(EquipSlot.OffHand)?.UnSetUpItem();
                   GameServices.Equipment.GetItemInSlot(EquipSlot.OffHand)?.gameObject.SetActive(false);
               }

               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.PrimaryHand)){
                    GameServices.Equipment.GetItemInSlot(EquipSlot.PrimaryHand)?.UnSetUpItem();
                     GameServices.Equipment.GetItemInSlot(EquipSlot.PrimaryHand)?.gameObject.SetActive(false);
               }

               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.PrimaryHand].InvUI.Outline.color = Color.black;

               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].DefaultColor = Color.black;
               
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].HotBarUI.Outline.color = Color.black;
               GameServices.Equipment.Equipment.Slots[EquipSlot.OffHand].InvUI.Outline.color = Color.black;
          }

          transform.localPosition = LocalPosition;
          
          if (ItemBehavior)ItemBehavior.Equipped();
     }

     public void UnSetUpItem(){
          if (Shadow)
               if (!Shadow.activeSelf) Shadow.SetActive(true);

          if (CustomUICanvasGroup)
            Destroy(CustomUICanvasGroup.gameObject);

          if (equipSlot == EquipSlot.PrimaryHand){
               transform.parent = null;
               if (gameObject.activeSelf)gameObject.SetActive(false);

               GameObject RightHand = GameServices.GlobalVariables.PrimaryHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.PrimaryHandObject.LeftHand;

               RightHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
               LeftHand.transform.SetParent(GameServices.GlobalVariables.PrimaryHandObject.CenterObject.transform);
          
               if (RightHand.activeSelf)RightHand.SetActive(false);
               if (LeftHand.activeSelf)LeftHand.SetActive(false);

               GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = null;
               GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = null;
          }else if (equipSlot == EquipSlot.OffHand){
               transform.parent = null;
               if (gameObject.activeSelf)gameObject.SetActive(false);

               GameObject RightHand = GameServices.GlobalVariables.OffHandObject.RightHand;
               GameObject LeftHand = GameServices.GlobalVariables.OffHandObject.LeftHand;

               RightHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);
               LeftHand.transform.SetParent(GameServices.GlobalVariables.OffHandObject.CenterObject.transform);
          
               if (RightHand.activeSelf)RightHand.SetActive(false);
               if (LeftHand.activeSelf)LeftHand.SetActive(false);

               GameServices.GlobalVariables.OffHandObject.RightHandSorter.AlwaysOnTopOf = null;
               GameServices.GlobalVariables.OffHandObject.LeftHandSorter.AlwaysOnTopOf = null;

          }else if (equipSlot == EquipSlot.BothHands){
               transform.parent = null;
               if (gameObject.activeSelf)gameObject.SetActive(false);

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

               if(GameServices.Equipment.HasAnItemInSlot(EquipSlot.PrimaryHand))
                    GameServices.Equipment.GetItemInSlot(EquipSlot.PrimaryHand)?.SetUpItem();

               if (GameServices.Equipment.HasAnItemInSlot(EquipSlot.OffHand))
                    GameServices.Equipment.GetItemInSlot(EquipSlot.OffHand)?.SetUpItem();

               GameServices.GlobalVariables.PrimaryHandObject.RightHandSorter.AlwaysOnTopOf = null;
               GameServices.GlobalVariables.PrimaryHandObject.LeftHandSorter.AlwaysOnTopOf = null;
          }
     
          if (ItemBehavior)ItemBehavior.UnEquipped();
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

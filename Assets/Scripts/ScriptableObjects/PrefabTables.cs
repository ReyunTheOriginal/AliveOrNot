using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PrefabTables")]
public class PrefabTables : ScriptableObject
{
   public List<ItemProperties> Items;
   public List<ObjectIDContainer> GameObjects; 
}

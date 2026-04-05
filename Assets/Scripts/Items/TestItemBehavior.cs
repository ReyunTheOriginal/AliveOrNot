using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItemBehavior : ItemBehavior
{
    public Sprite sprite;
    public override void Use(){
        GameObject obj = new GameObject();
        SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();
        ren.sprite = sprite;
        obj.transform.position = transform.position;
    }
    public override void Equipped(){
        Debug.Log("Equipped Test Item");
    }
    public override void UnEquipped(){
        Debug.Log("UnEquipped Test Item");
    }

    public override void Hold(){
        Debug.Log("is Equipped rn Test Item");
    }

}

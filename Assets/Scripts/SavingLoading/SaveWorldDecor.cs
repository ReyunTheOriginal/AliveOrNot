using UnityEngine;

public class SaveWorldDecor : BasicSave
{
    private void Awake() {
        InstanceID = transform.name;
    }
}

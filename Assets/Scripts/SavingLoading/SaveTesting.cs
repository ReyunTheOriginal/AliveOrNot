using UnityEngine;

public class SaveTesting : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)){
            Saving.SaveAll(Saving.SaveFileIndex);
        }
        if (Input.GetKeyDown(KeyCode.K)){
            Saving.LoadAll(Saving.SaveFileIndex);
        }
    }
}

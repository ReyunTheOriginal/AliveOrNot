using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PauseMenu)), CanEditMultipleObjects]
public class PauseMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Close All Menus")){
            UIManager.CloseAllMenus();
        }

        DrawDefaultInspector();
    }
}
#endif

public class PauseMenu : MonoBehaviour
{
    public CanvasGroup PauseMenuGroup;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (UIManager.OpenedMenus.Count > 0){
                UIManager.SetActiveCanvasGroup(true, UIManager.TopMenu.Value, UIManager.TopMenu.Key, false);
            }else{
                UIManager.ToggleCanvasGroup(true, PauseMenuGroup, "PauseMenu");
            }
        }
    }
}

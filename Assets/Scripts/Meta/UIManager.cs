using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CanvasGroup)), CanEditMultipleObjects]
public class CanvasGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CanvasGroup group = (CanvasGroup)target;
        if (GUILayout.Button("Toggle")){
            UIManager.ToggleCanvasGroup(false, group, "");
        }

        DrawDefaultInspector();
    }
}
#endif

public static class UIManager
{
    public static Canvas OverlayCanvas;
    public static Canvas WorldCanvas;
    public static Dictionary<string, CanvasGroup> OpenedMenus = new Dictionary<string, CanvasGroup>();

    public static void SetActiveCanvasGroup(bool IsAMenu, CanvasGroup Group, string MenuName = "UI", bool state = true, bool Ghost = false){
        if (Group){
            Group.alpha = state? 1: 0;
            if(!Ghost)Group.interactable = state;
            if(!Ghost)Group.blocksRaycasts = state;

            if (IsAMenu){
                if (!state){
                    if (OpenedMenus.ContainsKey(MenuName))
                        OpenedMenus.Remove(MenuName);
                }else{
                    if (!OpenedMenus.ContainsKey(MenuName))
                        OpenedMenus.Add(MenuName, Group);
                }
            }
        }else{
            Debug.Log("Null CanvasGroup");
        }
    }

    public static void CloseAllMenus(){
        foreach (var menu in OpenedMenus.Values){
            SetActiveCanvasGroup(true, menu, "", false);
        }

        OpenedMenus.Clear();
    }

    public static void ToggleCanvasGroup(bool IsAMenu, CanvasGroup group, string name){
        bool state = !IsActiveCanvasGroup(group);
        SetActiveCanvasGroup(IsAMenu, group, name, state);
    }

    public static bool IsActiveCanvasGroup(CanvasGroup Group, bool Ghost = false){
        if (Ghost) return Group.alpha == 1;

        return Group.alpha == 1 && Group.blocksRaycasts && Group.interactable;
    }

    public static bool AMenuIsOpened(){
        return OpenedMenus.Count > 0;
    }
}

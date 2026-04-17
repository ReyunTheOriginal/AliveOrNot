using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Canvas OverlayCanvas;
    public Canvas WorldCanvas;
    public Dictionary<string, CanvasGroup> OpenedMenus = new Dictionary<string, CanvasGroup>();

    private void Awake() {
        GameServices.UI = this;
    }

    public void SetActiveCanvasGroup(bool IsAMenu, CanvasGroup Group, string MenuName = "UI", bool state = true, bool Ghost = false){
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

    public void CloseAllMenus(){
        foreach (var menu in OpenedMenus.Values){
            SetActiveCanvasGroup(true, menu, "", false);
        }

        OpenedMenus.Clear();
    }

    public void ToggleCanvasGroup(bool IsAMenu, CanvasGroup group, string name){
        bool state = !IsActiveCanvasGroup(group);
        SetActiveCanvasGroup(IsAMenu, group, name, state);
    }

    public bool IsActiveCanvasGroup(CanvasGroup Group, bool Ghost = false){
        if (Ghost) return Group.alpha == 1;

        return Group.alpha == 1 && Group.blocksRaycasts && Group.interactable;
    }

    public bool AMenuIsOpened(){
        return OpenedMenus.Count > 0;
    }
}

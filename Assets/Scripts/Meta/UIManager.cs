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

    public void SetActiveCanvasGroup(bool IsAMenu, CanvasGroup Group, string MenuName = "UI", bool state = true){
        Group.alpha = state? 1: 0;
        Group.interactable = state;
        Group.blocksRaycasts = state;

        if (IsAMenu){
            if (!state){
                if (OpenedMenus.ContainsKey(MenuName))
                    OpenedMenus.Remove(MenuName);
            }else{
                if (!OpenedMenus.ContainsKey(MenuName))
                    OpenedMenus.Add(MenuName, Group);
            }
        }
    }

    public void CloseAllMenus(){
        foreach (var menu in OpenedMenus.Values){
            SetActiveCanvasGroup(true, menu, "", false);
        }

        OpenedMenus.Clear();
    }

    public void ToggleUI(bool IsAMenu, CanvasGroup group, string name){
        bool state = !IsActive(group);
        SetActiveCanvasGroup(IsAMenu, group, name, state);
    }

    public bool IsActive(CanvasGroup Group){
        return Group.alpha == 1 && Group.blocksRaycasts && Group.interactable;
    }

    public bool AMenuIsOpened(){
        return OpenedMenus.Count > 0;
    }
}

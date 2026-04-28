using UnityEngine;

public class OnlyShowIfGroupClosed : MonoBehaviour
{
    public CanvasGroup OppositeGroup; //the group it should be set active to its opposite activate state
    public bool OppositeGroupIsGhost;
    private CanvasGroup Group;
    public bool IsAMenu;

    #if UNITY_EDITOR
        private void OnValidate() {
            Group = GetComponent<CanvasGroup>();
        }
    #endif

    void Update()
    {
        UIManager.SetActiveCanvasGroup(IsAMenu, Group, "", !UIManager.IsActiveCanvasGroup(OppositeGroup, OppositeGroupIsGhost));
    }
}

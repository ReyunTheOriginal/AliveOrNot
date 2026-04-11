using UnityEngine;
using UnityEngine.EventSystems; // Required for pointer events

public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isHovering { get; private set; }

    public void OnPointerEnter(PointerEventData eventData){
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData){
        isHovering = false;
    }
}

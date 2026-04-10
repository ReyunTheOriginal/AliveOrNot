using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterTiles : MonoBehaviour
{

    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player")){
            GameServices.GlobalVariables.Player.SpriteRenderer.gameObject.transform.localPosition = new Vector3(0,-0.25f,0);
            GameServices.GlobalVariables.Player.MovementScript.CurrentStates[PlayerMovement.State.InShallowWater] = true;
            GameServices.GlobalVariables.Player.SpriteMask.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")){
            GameServices.GlobalVariables.Player.SpriteRenderer.gameObject.transform.localPosition = new Vector3(0,0,0);
            GameServices.GlobalVariables.Player.MovementScript.CurrentStates[PlayerMovement.State.InShallowWater] = false;
            GameServices.GlobalVariables.Player.SpriteMask.enabled = false;
        }
    }
}


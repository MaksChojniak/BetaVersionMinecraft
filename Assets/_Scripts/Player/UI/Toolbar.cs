using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{

    public PlayerInput playerInput;
    public RectTransform highlight;
    public ItemSlot[] itemSlots;




    public void Create()
    {

        var player = GameObject.FindWithTag("Player");
        playerInput = player.GetComponent<PlayerInput>();
        playerInput.BlockTypeNumberSelected = 0;
    }

    private void Update()
    {

        if (GetBlockTypeSelected().Equals(true))
        {
            highlight.position = itemSlots[playerInput.BlockTypeNumberSelected].icon.transform.position;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                playerInput.BlockTypeNumberSelected = playerInput.BlockTypeNumberSelected - 1;
            }
            else { playerInput.BlockTypeNumberSelected = playerInput.BlockTypeNumberSelected + 1; }

            if (playerInput.BlockTypeNumberSelected > itemSlots.Length - 1) playerInput.BlockTypeNumberSelected = 0;

            if (playerInput.BlockTypeNumberSelected < 0) playerInput.BlockTypeNumberSelected = itemSlots.Length - 1;

            highlight.position = itemSlots[playerInput.BlockTypeNumberSelected].icon.transform.position;
        }
    }
    private bool GetBlockTypeSelected()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { playerInput.BlockTypeNumberSelected = 0; return true; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { playerInput.BlockTypeNumberSelected = 1; return true; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { playerInput.BlockTypeNumberSelected = 2; return true; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { playerInput.BlockTypeNumberSelected = 3; return true; }
        return false;
    }
}

[System.Serializable]
public class ItemSlot
{

    public int itemID;
    public Image icon;

}

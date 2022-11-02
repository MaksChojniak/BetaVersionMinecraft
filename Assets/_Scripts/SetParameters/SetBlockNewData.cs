using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetBlockNewData : MonoBehaviour
{
    public GameObject blockParameterParent;
    public List<GameObject> blockParameters;
    public BlockDataSO blockData;
    public List<TextureData> blockDataList;
    void Start()
    {
        for (int i = 0; i < blockParameterParent.transform.childCount; i++)
        {
            blockDataList.Add(blockData.textureDataList[i]);
            blockParameters.Add(blockParameterParent.transform.GetChild(i).gameObject);
        }

        for (int i = 1; i < blockDataList.Count; i++)
        {
            blockDataList[i].durability = 1;
            blockDataList[i].placable = true;
        }

        SetText();
        SetClicked();
    }


    public void SetText()
    {
        for (int i = 0; i < blockParameterParent.transform.childCount; i++)
        {
            if(blockParameters[i].name == blockDataList[i].blockType.ToString())
                blockParameters[i].transform.GetChild(0).GetComponent<InputField>().text = blockDataList[i].durability.ToString();
        }
    }

    public void SetClicked()
    {
        for (int i = 1; i < blockParameterParent.transform.childCount; i++)
        {
            if (blockParameters[i].name == blockDataList[i].blockType.ToString())
            {
                if(blockDataList[i].placable == true)
                {
                    blockParameters[i].transform.GetChild(2).GetComponent<Button>().interactable = false;
                }
                else
                {
                    blockParameters[i].transform.GetChild(3).GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void SetDurabilityData()
    {
        for (int i = 0; i < blockParameterParent.transform.childCount; i++)
        {
            GameObject currentButton = EventSystem.current.currentSelectedGameObject;
            if (currentButton.transform.parent.gameObject.name == blockDataList[i].blockType.ToString())
            {
                Text newParameter = blockParameters[i].transform.GetChild(0).GetChild(2).GetComponent<Text>();
                blockDataList[i].durability = int.Parse(newParameter.text);
            }
        }
        SetText();
    }

    public void SetPlacableData()
    {
        for (int i = 0; i < blockParameterParent.transform.childCount; i++)
        {
            GameObject currentButton = EventSystem.current.currentSelectedGameObject;
            if (currentButton.transform.parent.gameObject.name == blockParameters[i].name)
            {
                if(currentButton.name == "Set On")
                {
                    blockDataList[i].placable = true;
                }
                else
                {
                    blockDataList[i].placable = false;
                }
            }
        }
        SetClicked();
    }
}

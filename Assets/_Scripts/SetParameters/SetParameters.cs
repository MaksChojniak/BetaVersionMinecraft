using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class SetParameters : MonoBehaviour
{
    public GameObject worldParameters;
    public World worldManager;
    public List<GameObject> inputWorldFields;


    void Start()
    {
        IntializeParameter();
        setText();
    }


    public void setText()
    {

        inputWorldFields[0].GetComponent<InputField>().text = worldManager.mapSizeInChunks.ToString();
        inputWorldFields[1].GetComponent<InputField>().text = worldManager.chunkSize.ToString();
        inputWorldFields[2].GetComponent<InputField>().text = worldManager.chunkHeight.ToString();
        inputWorldFields[3].GetComponent<InputField>().text = worldManager.chunkDrawingRange.ToString();
        inputWorldFields[4].GetComponent<InputField>().text = worldManager.mapSeedOffset.x.ToString();
        inputWorldFields[5].GetComponent<InputField>().text = worldManager.mapSeedOffset.y.ToString();

    }

    public void IntializeParameter()
    {
        for (int i = 0; i < worldParameters.transform.childCount; i++)
        {
            if (worldParameters.transform.GetChild(i).childCount > 2)
            {
                inputWorldFields.Add(worldParameters.transform.GetChild(i).GetChild(0).gameObject);
                inputWorldFields.Add(worldParameters.transform.GetChild(i).GetChild(1).gameObject);
            }
            else if (worldParameters.transform.GetChild(i).childCount == 2)
            {
                inputWorldFields.Add(worldParameters.transform.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    public void SetParameter()
    {
        if (EventSystem.current.currentSelectedGameObject.gameObject == inputWorldFields[0].transform.parent.GetChild(1).gameObject)
        {
            Text newParameter = inputWorldFields[0].transform.GetChild(2).GetComponent<Text>();
            if (newParameter != null && newParameter.text.Length > 0)
                worldManager.mapSizeInChunks = int.Parse(newParameter.text);
            return;
        }
        else if (EventSystem.current.currentSelectedGameObject.gameObject == inputWorldFields[1].transform.parent.GetChild(1).gameObject)
        {
            Text newParameter = inputWorldFields[1].transform.GetChild(2).GetComponent<Text>();
            if (newParameter != null && newParameter.text.Length > 0)
                worldManager.chunkSize = int.Parse(newParameter.text);
            return;
        }
        else if (EventSystem.current.currentSelectedGameObject.gameObject == inputWorldFields[2].transform.parent.GetChild(1).gameObject)
        {
            Text newParameter = inputWorldFields[2].transform.GetChild(2).GetComponent<Text>();
            if (newParameter != null && newParameter.text.Length > 0)
                worldManager.chunkHeight = int.Parse(newParameter.text);
            return;
        }
        else if (EventSystem.current.currentSelectedGameObject.gameObject == inputWorldFields[3].transform.parent.GetChild(1).gameObject)
        {
            Text newParameter = inputWorldFields[3].transform.GetChild(2).GetComponent<Text>();
            if (newParameter != null && newParameter.text.Length > 0)
                worldManager.chunkDrawingRange = int.Parse(newParameter.text);
            return;
        }
        else if (EventSystem.current.currentSelectedGameObject.gameObject == inputWorldFields[4].transform.parent.GetChild(2).gameObject)
        {
            Text xParameter = inputWorldFields[4].transform.GetChild(2).GetComponent<Text>();
            Text yParameter = inputWorldFields[5].transform.GetChild(2).GetComponent<Text>();
            if (xParameter != null && yParameter != null && xParameter.text.Length > 0 && yParameter.text.Length > 0)
                worldManager.mapSeedOffset = new Vector2Int(int.Parse(xParameter.text), int.Parse(yParameter.text));
            return;
        }
        setText();
    }

}

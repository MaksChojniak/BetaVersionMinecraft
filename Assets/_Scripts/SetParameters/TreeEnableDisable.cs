using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeEnableDisable : MonoBehaviour
{
    public GameObject treeSetParameter;
    public NoiseSettings treeEnable;
    public NoiseSettings treeDisable;

    public GameObject terrainGenerator;
    public List<GameObject> biomesGenerators;


    void Start()
    {
        for (int i = 0; i < terrainGenerator.transform.childCount; i++)
        {
            biomesGenerators.Add(terrainGenerator.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < biomesGenerators.Count; i++)
        {
            biomesGenerators[i].GetComponent<TreeGenerator>().treeNoiseSettings = treeDisable;
        }
        SetClicked();


    }

    public void SetClicked()
    {
        for (int i = 0; i < biomesGenerators.Count; i++)
        {
            if (biomesGenerators[i].GetComponent<TreeGenerator>().treeNoiseSettings == treeDisable)
            {
                treeSetParameter.transform.GetChild(1).GetComponent<Button>().interactable = false;
            }
            else if (biomesGenerators[i].GetComponent<TreeGenerator>().treeNoiseSettings == treeEnable)
            {
                treeSetParameter.transform.GetChild(0).GetComponent<Button>().interactable = false;
            }
        }
    }

    public void TreeEnable()
    {
        for(int i = 0; i < biomesGenerators.Count; i++)
        {
            biomesGenerators[i].GetComponent<TreeGenerator>().treeNoiseSettings = treeEnable;
        }
        SetClicked();
    }

    public void TreeDisable()
    {
        for (int i = 0; i < biomesGenerators.Count; i++)
        {
            biomesGenerators[i].GetComponent<TreeGenerator>().treeNoiseSettings = treeDisable;
        }
        SetClicked();
    }
}

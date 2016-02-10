using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TglTower : MonoBehaviour
{

    public GameObject towerToBuild;
    public Sprite selectOK;
    public Sprite noselectOK;
    public Sprite noselectKO;
    public bool isActive;
	public Text txtCost;

    private Toggle toggle;
    private Image[] images;
    private Tower tower;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        images = toggle.GetComponentsInChildren<Image>();

        foreach (Transform child in towerToBuild.transform)
        {
            if (child.name == "Sphere")
            {
                tower = child.GetComponent<Tower>() as Tower;
                break;
            }
        }

        toggle.onValueChanged.AddListener(StatusChanged);
    }

    private void StatusChanged(bool isThereMoneyPb)
    {
        RefreshBuildable(toggle.isOn, towerToBuild, isThereMoneyPb);
    }

    /** *********************** REFRESH BUILDABLE  *************************  */
    public void RefreshBuildable(bool isOn, GameObject towerToBuild, bool isThereMoneyPb)
    {
        StartCoroutine(RefreshBuildableCoroutine(isOn, towerToBuild, isThereMoneyPb));
    }

    private IEnumerator RefreshBuildableCoroutine(bool isOn, GameObject towerToBuild, bool isThereMoneyPb)
    {
        foreach (GameObject floor in GameObject.FindGameObjectsWithTag("floor"))
        {
            Build build = floor.GetComponent("Build") as Build;
            if (!build.hasTower)
            {
                build.towerToBuild = isOn ? towerToBuild : null;
                build.NotifyTowerChanged(isOn, isThereMoneyPb);
                yield return new WaitForSeconds(0.0001F);
            }
        }
    }
    /** *********************** REFRESH BUILDABLE  *************************  */

    public void enable()
    {
        if (!isActive)
        {
            toggle.enabled = true;
            isActive = true;
            toggle.isOn = false;
        }
        foreach (Image image in images)
        {
            string name = image.gameObject.transform.name;
            if (name.Equals("Background"))
                image.sprite = toggle.isOn ? selectOK : noselectOK;
            else if (name.Equals("Checkmark"))
                image.sprite = toggle.isOn ? selectOK : noselectOK;
        }
    }

    public void disable()
    {
        if (isActive)
        {
            toggle.enabled = false;
            isActive = false;
            toggle.isOn = false;
        }
        foreach (Image image in images)
        {
            string name = image.gameObject.transform.name;
            if (name.Equals("Background"))
                image.sprite = noselectKO;
            else if (name.Equals("Checkmark"))
                image.sprite = noselectKO;
        }
    }
	
	void Update()
	{
        txtCost.text = tower.cost + "";
	}
}

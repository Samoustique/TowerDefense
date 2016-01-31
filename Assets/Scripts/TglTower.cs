using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TglTower : MonoBehaviour
{

    public GameObject towerToBuild;
    public Sprite selectOK;
    public Sprite selectKO;
    public Sprite noselectOK;
    public Sprite noselectKO;
    public bool isActive;
	public Text txtCost;

    private Toggle toggle;
    private Image[] images;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        images = toggle.GetComponentsInChildren<Image>();
    }

    public void TowerTglClick()
    {
        if (toggle.isOn) {
            foreach (GameObject floor in GameObject.FindGameObjectsWithTag("floor"))
            {
                Build build = floor.GetComponent("Build") as Build;
                build.towerToBuild = isActive ? towerToBuild : null;
            }
        }
    }

    public void enable()
    {
        isActive = true;
        TowerTglClick();
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
        isActive = false;
        TowerTglClick();
        foreach (Image image in images)
        {
            string name = image.gameObject.transform.name;
            if (name.Equals("Background"))
                image.sprite = toggle.isOn ? selectKO : noselectKO;
            else if (name.Equals("Checkmark"))
                image.sprite = toggle.isOn ? selectKO : noselectKO;
        }
    }
	
	void Update()
	{
		foreach (Transform child in towerToBuild.transform)
		{
			if (child.name == "Sphere") {
				Tower tower = child.GetComponent<Tower>() as Tower;
				txtCost.text = tower.cost + "";
				break;
			}
		}
	}
}

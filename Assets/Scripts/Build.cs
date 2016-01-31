using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Build : MonoBehaviour {
    
    public bool hasTower;
    public GameObject towerToBuild, towerBuilt;
    public Text txtInfo;
	public Material selectedMaterial;
		
	private bool isMouseOverChecked, wouldBlockPath;
	private GameObject fakeObstacle, obstacle;
	private GameObject fakeMob;
    private Color basicQuadColor;

    void Start()
    {
        hasTower = false;
		fakeObstacle = GameObject.Find("fakeObstacle");
		fakeMob = GameObject.Find("fakeMob");
        basicQuadColor = GetComponent<Renderer>().material.color;
    }

	void OnMouseOver ()
	{
		if(!isMouseOverChecked)
		{
			isMouseOverChecked = true;
			StartCoroutine(CheckMouseOver(0.0001));
		}
	}

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = basicQuadColor;
		Destroy(obstacle);
		wouldBlockPath = false;
		isMouseOverChecked = false;
    }

    void OnMouseUp()
    {
        if (isMouseOverChecked)
        {
            // Display tower data
            GameManager.SelectUnselectTower(towerBuilt, selectedMaterial);
            if (wouldBlockPath)
            {
                StartCoroutine(ShowMessage("Don't block the way !", 2));
            }
            else if (towerToBuild && !towerBuilt)
            {
                print("build");
                towerBuilt = Instantiate(towerToBuild, transform.position, Quaternion.identity) as GameObject;
                Tower tower = towerBuilt.GetComponentInChildren<Tower>() as Tower;
                GameManager.gold -= tower.cost;
                GetComponent<Renderer>().material.color = Color.blue;
            }
            else if (!towerBuilt)
            {
                StartCoroutine(ShowMessage("Not enough cash !", 2));
            }
        }
    }

    private IEnumerator ShowMessage(string message, float delay)
    {
        txtInfo.text = message;
        txtInfo.enabled = true;
        yield return new WaitForSeconds(delay);
        txtInfo.enabled = false;
    }
		
	private IEnumerator CheckMouseOver(double delay)
    {
		Color colorToSet = basicQuadColor;
		NavMeshPath path = null;

		if (towerBuilt == null && towerToBuild != null)
		{
			obstacle = Instantiate(fakeObstacle, transform.position, Quaternion.identity) as GameObject;
			path = new NavMeshPath();
		}
		
		yield return new WaitForSeconds((float) delay);
		
		// Check if there has been no mouseExit while waiting
		if(isMouseOverChecked)
		{
			// case OK
			colorToSet = Color.green;
			if (path != null)
				print("towerBuilt : " + towerBuilt + " towerToBuild : " + towerToBuild + " path : " + path.status);
			else
				print("towerBuilt : " + towerBuilt + " towerToBuild : " + towerToBuild + " path : " + path);
			// case already built
			if (towerBuilt != null)
			{
				colorToSet = Color.blue;
			}
			// case not enough cash
			else if(towerToBuild == null)
			{
				colorToSet = Color.red;
			}
			// case check the way
			else 
			{
				if (path !=null)
				{
					NavMeshAgent agent = fakeMob.GetComponent<NavMeshAgent>();
					agent.CalculatePath(GameObject.Find("home").transform.position, path);
					print(path.status);
					// the way can be blocked
					if (path.status != NavMeshPathStatus.PathComplete)
					{
						print("ROUGE");
						colorToSet = Color.red;
						wouldBlockPath = true;
					}
				}
				Destroy(obstacle);
			}
			
			GetComponent<Renderer>().material.color = colorToSet;
		}
    }
}

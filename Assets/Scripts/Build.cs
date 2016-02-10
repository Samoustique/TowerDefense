using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Build : MonoBehaviour {
    
    public bool hasTower;
    public GameObject towerToBuild, towerBuilt;
    public Text txtInfo;
	public Material selectedMaterial;
		
	private bool isBuildable;
	private GameObject fakeObstacle, fakeMob;
    private Color basicQuadColor;
    
    void Start()
    {
        hasTower = false;
		fakeObstacle = GameObject.Find("fakeObstacle");
		fakeMob = GameObject.Find("fakeMob");
        basicQuadColor = GetComponent<Renderer>().material.color;
    }

    void OnMouseUp()
    {
        // Display tower data
        GameManager.SelectUnselectTower(towerBuilt, selectedMaterial);
        if (isBuildable && towerToBuild && !towerBuilt)
        {
            towerBuilt = Instantiate(towerToBuild, transform.position, Quaternion.identity) as GameObject;
            Tower tower = towerBuilt.GetComponentInChildren<Tower>() as Tower;
            GameManager.gold -= tower.cost;
            GetComponent<Renderer>().material.color = basicQuadColor;
            RefreshBuildable(true, towerToBuild, true);
        }
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

    /** *********************** CHECK BUILDABLE  *************************  */
    public void NotifyTowerChanged(bool isOn, bool isThereMoneyPb)
    {
        StartCoroutine(CheckBuildableCoroutine(isOn, isThereMoneyPb, 0.0001F));
    }

    private IEnumerator CheckBuildableCoroutine(bool isOn, bool isThereMoneyPb, double delay)
    {
        Color colorToSet = basicQuadColor;
        NavMeshPath path = null;

        if (towerBuilt == null)
        {
            GameObject obstacle = null;
            if (isOn && towerToBuild != null)
            {
                obstacle = Instantiate(fakeObstacle, transform.position, Quaternion.identity) as GameObject;
                path = new NavMeshPath();
            }

            yield return new WaitForSeconds((float)delay);
            if (path != null)
            {
                NavMeshAgent agent = fakeMob.GetComponent<NavMeshAgent>();
                agent.CalculatePath(GameObject.Find("home").transform.position, path);
                // the way can be blocked
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    isBuildable = false;
                }
                else
                {
                    isBuildable = true;
                    colorToSet = Color.green;
                }
            }
            Destroy(obstacle);
        }
        else
        {
            isBuildable = false;
        }
        GetComponent<Renderer>().material.color = colorToSet;
    }
    /** *********************** CHECK BUILDABLE  *************************  */
}

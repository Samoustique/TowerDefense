using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class GameManager : MonoBehaviour {

    public static int gold = 2000;
    public static int life = 5;
    public static List<GameObject> mobsAlive = new List<GameObject>();

    public Text txtGold, txtLife;
    public GameObject particles, home;
    public List<Material> homeColors;

    private static Material notSelectedMaterial;
	private static GameObject selectedTower;
	
    private Toggle[] toggles;
    private GameObject titleRound, smallTitleRound;
    private UnityStandardAssets.ImageEffects.TiltShift tiltShift;
    private float roundTimer, spawnTimer, spawnLaps = 2;
    private int nbSpawnInARound, nbRound = 1;
    private bool showTitleRoundText;
    private Spawn spawner;
	
    void Start()
    {
        showTitleRoundText = true;
        toggles = GameObject.Find("TowerChoice").GetComponentsInChildren<Toggle>();
        titleRound = GameObject.Find("titleRound");
        smallTitleRound = GameObject.Find("smallTitleRound");
        tiltShift = GameObject.Find("Main Camera").GetComponent<UnityStandardAssets.ImageEffects.TiltShift>();
        roundTimer = Time.time;
        spawner = GameObject.Find("Spawn").GetComponentInChildren<Spawn>();
		HideTowerDetails();
    }

    void Update()
    {
        UpdateRound();

        UpdateGold();

		UpdateTowerSelection();
		
		UpdateTowerDetails();

        // home life
        txtLife.text = "LIFE : " + life;
        if (life == 0)
        {
            Instantiate(particles, GameObject.Find("home").transform.position, Quaternion.identity);
            Destroy(GameObject.Find("home"));
        }
    }
	
	private void UpdateRound()
	{
		showTitleRoundText = Time.time < roundTimer + 2;
        titleRound.SetActive(showTitleRoundText);
        tiltShift.enabled = showTitleRoundText;
        smallTitleRound.SetActive(!showTitleRoundText);

        if (showTitleRoundText)
        {
            (titleRound.GetComponent<Text>()).text = "Round " + nbRound;
            spawnTimer = Time.time;
            nbSpawnInARound = 0;
        }
        else
        {
            (smallTitleRound.GetComponent<Text>()).text = "Round " + nbRound;
            // if we finished to spawn everybody
            if (Spawn.mobsPerRound[nbRound - 1].Count == 0)
            {
                if (GameManager.mobsAlive.Count == 0)
                {
                    // next round
                    roundTimer = Time.time;
                    nbRound++;
                }
            }
            else
            {
                if (Time.time > spawnTimer + spawnLaps)
                {
                    nbSpawnInARound++;
                    spawner.SpawnMob(nbRound);
                    spawnTimer = Time.time;
                }
            }
        }
	}

    static public void ShowUpReward(Vector3 showPosition, GameObject textToInstantiate)
    {
        Vector3 worldToScreenPoint = Camera.main.WorldToScreenPoint(showPosition);
        GameObject empty = Instantiate(new GameObject(), worldToScreenPoint, Quaternion.identity) as GameObject;
        empty.transform.name = "RewardWrapper";
        empty.transform.SetParent(GameObject.Find("Canvas").transform);
        GameObject temp = Instantiate(textToInstantiate, worldToScreenPoint, Quaternion.identity) as GameObject;
        temp.transform.SetParent(empty.transform);
        temp.GetComponent<Animator>().SetTrigger("Reward");
        Destroy(temp.gameObject, 1.0F);
        Destroy(empty.gameObject, 1.0F);
    }

    private void UpdateGold()
	{
        txtGold.text = "GOLD : " + gold;
	}
	
	private void UpdateTowerSelection()
	{
		foreach (Toggle toggle in toggles)
        {
            TglTower tglTower = toggle.GetComponent<TglTower>() as TglTower;
            Tower tower = null;		
			Transform sphere = FindSphereInChildren(tglTower.towerToBuild.transform);
			if (sphere != null)
			{
				tower = sphere.GetComponent<Tower>() as Tower;
			}

            if (tower != null)
            {
                if (gold >= tower.cost)
                    tglTower.enable();
                else
                    tglTower.disable();
            }
        }
	}
	
	private void UpdateTowerDetails()
	{
		/*if(selectedTower != null)
		{
			Tower tower = selectedTower.GetComponentInChildren<Tower>() as Tower;
			Button btnUp = GameObject.Find("btnUp").GetComponent<Button>() as Button;
			ColorBlock cb = btnUp.colors;
			cb.normalColor = Color.red;
			btnUp.colors = cb;
			btnUp.interactable = gold >= tower.evolvPrice;
			print("ici " + btnUp.interactable);
		}*/
	}

    /*static public bool LoadMobsPerRound()
    {
        Spawn.mobsPerRound = new List<Dictionary<int, int>>();
        Dictionary<int, int> round = new Dictionary<int, int>();
        try
        {
            string line;
            StreamReader theReader = new StreamReader("C:\\Users\\Public\\Documents\\Unity Projects\\New Unity Project\\Assets\\Documents\\Spawn.txt", Encoding.Default);
            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();

                    if (line != null)
                    {
                        if (line.Length == 0)
                        {
                            Spawn.mobsPerRound.Add(round);
                            round = new Dictionary<int, int>();
                        }
                        else
                        {
                            string[] entries = line.Split(' ');
                            round.Add(System.Convert.ToInt32(entries[0]), System.Convert.ToInt32(entries[1]));
                        }
                    }
                }
                while (line != null); 
                theReader.Close();
                return true;
            }
        }
        catch (System.Exception e)
        {
            print("Exception during load");
            return false;
        }
    }*/
	
	static public void SelectUnselectTower(GameObject towerToSelect, Material selectedMaterial)
	{
		if (!towerToSelect)
		{
			UnselectTower();
		} else if (selectedTower != null && towerToSelect != selectedTower) // TODO autoriser cliquage dans zone d'infos de la tour
		{
			UnselectTower();
			SelectTower(towerToSelect, selectedMaterial);
		}
		else if (!selectedTower || towerToSelect != selectedTower) // TODO autoriser cliquage dans zone d'infos de la tour
		{
			SelectTower(towerToSelect, selectedMaterial);
		}
	}
	
	static private void SelectTower(GameObject towerToSelect, Material selectedMaterial)
	{
		selectedTower = towerToSelect;			
		Transform sphere = FindSphereInChildren(towerToSelect.transform);
		if (sphere != null)
		{
			notSelectedMaterial = sphere.GetComponent<Renderer>().material;
			sphere.GetComponent<Renderer>().material = selectedMaterial;
		}
		RevealTowerDetails(towerToSelect);
	}
	
	static private void RevealTowerDetails(GameObject towerToSelect)
	{
		Tower tower = towerToSelect.GetComponentInChildren<Tower>() as Tower;
		Bullet bullet = tower.bullet.GetComponent<Bullet>() as Bullet;
		GameObject towerSelectedDetails = GameObject.Find("TowerSelected");
		towerSelectedDetails.GetComponent<Image>().enabled = true;
		SphereCollider sphereColl = towerToSelect.GetComponentInChildren<SphereCollider>() as SphereCollider;

		DisplayTowerCharacteristic("Name", tower.title);
		DisplayTowerCharacteristic("Lvl", "lvl" + tower.level);
		DisplayTowerCharacteristic("Dmg", "Dmg\n" + bullet.damage);
		DisplayTowerCharacteristic("Rng", "Rng\n" + sphereColl.radius);
		DisplayTowerCharacteristic("Rhy", "Rhy\n" + tower.rhythm);
		DisplayTowerCharacteristic("Sell", "Sell\n" + tower.sellingPrice);
		GameObject.Find("btnSell").GetComponent<Image>().enabled = true;
		
		Tower towerUp = null;
		Transform sphere = FindSphereInChildren(tower.towerUp.transform);
		if (sphere != null)
		{
			towerUp = sphere.GetComponent<Tower>() as Tower;
		}
		
		if (towerUp)
		{
			DisplayTowerCharacteristic("Up", "Up\n" + towerUp.cost);
			
			GameObject gameObjectUp = GameObject.Find("btnUp");
			gameObjectUp.GetComponent<Image>().enabled = true;
			Button btnUp = gameObjectUp.GetComponent<Button>() as Button;
			btnUp.interactable = gold >= towerUp.cost;
		}
	}
	
	static private Transform FindSphereInChildren(Transform objectToInspect)
	{
		Transform toReturn = null;
		foreach (Transform child in objectToInspect)
		{
			if (child.name == "Sphere") {
				return child;
			}
			else{
				toReturn = FindSphereInChildren(child);
				if(toReturn != null)
				{
					return toReturn;
				}
			}
		}
		return toReturn;
	}
	
	static private void DisplayTowerCharacteristic(string objectName, string textToSet)
	{
		Text text = GameObject.Find(objectName).GetComponent<Text>();
		text.text = textToSet;
		text.enabled = true;
	}	
	
	static public void UnselectTower()
	{
		if (selectedTower != null)
		{
			Transform sphere = FindSphereInChildren(selectedTower.transform);
			if (sphere != null)
			{
				sphere.GetComponent<Renderer>().material = notSelectedMaterial;
			}
		}
		selectedTower = null;
		
		HideTowerDetails();
	}
	
	static private void HideTowerDetails()
	{
		// background image
		GameObject towerSelectedDetails = GameObject.Find("TowerSelected");
		towerSelectedDetails.GetComponent<Image>().enabled = false;
		
		// texts
		Text[] texts = towerSelectedDetails.GetComponentsInChildren<Text>();
		foreach (Text text in texts)
        {
			text.enabled = false;
        }
		
		// buttons
		Image[] images = towerSelectedDetails.GetComponentsInChildren<Image>();
		foreach (Image img in images)
        {
			img.enabled = false;
        }
		GameObject gameObjectUp = GameObject.Find("btnSell");
		gameObjectUp.GetComponent<Image>().enabled = false;
		gameObjectUp.GetComponentInChildren<Text>().enabled = false;
		GameObject gameObjectSell = GameObject.Find("btnUp");
		gameObjectSell.GetComponent<Image>().enabled = false;
		gameObjectSell.GetComponentInChildren<Text>().enabled = false;
	}
	
	static public void sellTower()
	{
		Tower tower = selectedTower.GetComponentInChildren<Tower>() as Tower;
		gold += tower.sellingPrice;
		Destroy(selectedTower);
		selectedTower = null;
		HideTowerDetails();
	}
	
	static public void upTower()
	{
		print("up up up besoin de sauvegarder le Build de la tour sélectionnée.");
	}

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class GameManager : MonoBehaviour {

    public static int gold = 3500;
    public static int life = 5;
    public static List<GameObject> mobsAlive = new List<GameObject>();
    public static Step step;

    public Text txtGold, txtLife, txtTimerConstruction;
    public Button btnNextWave;
    public GameObject particles, home;
    public int constructionLast;
    public enum Step
    {
        CONSTRUCTION_TITLE,
        CONSTRUCTION,
        ROUND_TITLE,
        ROUND
    };

    private static Material notSelectedMaterial;
	private static GameObject selectedTower;
    private static GameObject selectionAura;

    private Toggle[] toggles;
    private Dictionary<TglTower, Tower> choiceTowers;
    private GameObject title, smallTitle, timerConstruction, nextWave;
    private UnityStandardAssets.ImageEffects.TiltShift tiltShift;
    private float titleTimer, spawnTimer, constructionTimer, spawnLaps, shakeTimer, shakeAmount;
    //private int nbSpawnInARound;
    private int nbRound = 1;
    private bool showTitleText;
    private Spawn spawner;
    private Transform originalCamera;  

    void Start()
    {
        toggles = GameObject.Find("TowerChoice").GetComponentsInChildren<Toggle>();
        choiceTowers = new Dictionary<TglTower, Tower>();
        retrieveChoiceTowers();
        DisableChoices();

        title = GameObject.Find("txtTitle");
        smallTitle = GameObject.Find("txtSmallTitle");
        tiltShift = GameObject.Find("Main Camera").GetComponent<UnityStandardAssets.ImageEffects.TiltShift>();
        titleTimer = constructionTimer = Time.time;
        spawnLaps = 2;
        spawner = GameObject.Find("Spawn").GetComponentInChildren<Spawn>();
        step = Step.CONSTRUCTION_TITLE;
        shakeAmount = 0.1F;
        shakeTimer = 1F;

        HideTowerDetails();
        
        btnNextWave.onClick.AddListener(EndConstructionTime);
        nextWave = GameObject.Find("btnNextWave");
        nextWave.SetActive(false);

        originalCamera = transform;
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
            if (shakeTimer >= 0)
            {
                Vector2 shakePos = UnityEngine.Random.insideUnitCircle * shakeAmount;
                transform.position = new Vector3(transform.position.x + shakePos.x, transform.position.y + shakePos.y, transform.position.z);
                shakeTimer -= Time.deltaTime;
            }
            Instantiate(particles, GameObject.Find("home").transform.position, Quaternion.identity);
            Destroy(GameObject.Find("home"));
        }
    }

    void DisableChoices()
    {
        // disable towerChoice
        foreach (TglTower tglTower in choiceTowers.Keys)
        {
            tglTower.disable();
        }
    }

    void EnableChoices()
    {
        foreach (TglTower tglTower in choiceTowers.Keys)
        {
            tglTower.enable();
        }
    }

    private void ConstructionTitleTime()
    {
        transform.position = originalCamera.position;
        transform.rotation = originalCamera.rotation;
        (title.GetComponent<Text>()).text = "Construction";
        title.SetActive(true);
        tiltShift.enabled = true;
        smallTitle.SetActive(false);
        foreach(Toggle toggle in toggles)
        {
            toggle.enabled = false;
        }

        if (Time.time >= titleTimer + 2)
        {
            step = Step.CONSTRUCTION;
            constructionTimer = Time.time;
            txtTimerConstruction.enabled = true;
            nextWave.SetActive(true);
            EnableChoices();
        }
    }

    private void ConstructionTime()
    {
        (smallTitle.GetComponent<Text>()).text = "Construction";
        title.SetActive(false);
        tiltShift.enabled = false;
        smallTitle.SetActive(true);
        
        // timer display
        float timeLeft = constructionTimer + constructionLast - Time.time;
        int seconds = (int)(timeLeft % 60F);
        txtTimerConstruction.text = seconds.ToString("00");
        
        if (seconds == 0)
        {
            EndConstructionTime();
        }
    }

    public void EndConstructionTime()
    {
        txtTimerConstruction.enabled = false;
        nextWave.SetActive(false);
        step = Step.ROUND_TITLE;
        titleTimer = Time.time;
        //nbSpawnInARound = 0;
        DisableChoices();
    }

    private void RoundTitleTime()
    {
        (title.GetComponent<Text>()).text = "Wave " + nbRound;
        title.SetActive(true);
        tiltShift.enabled = true;
        smallTitle.SetActive(false);
        foreach (Toggle toggle in toggles)
        {
            toggle.enabled = false;
        }

        if (Time.time >= titleTimer + 2)
        {
            step = Step.ROUND;
            titleTimer = Time.time;
        }
    }

    private void RoundTime()
    {
        (smallTitle.GetComponent<Text>()).text = "Wave " + nbRound;
        title.SetActive(false);
        tiltShift.enabled = false;
        smallTitle.SetActive(true);
        GameObject center = GameObject.Find("Quad (26)");
        transform.LookAt(center.transform);
        transform.RotateAround(center.transform.position, new Vector3((float)Math.Cos(Time.time), 1, 0), Time.deltaTime * 10);

        // if we finished to spawn everybody
        if (Spawn.mobsPerRound[nbRound - 1].Count == 0)
        {
            if (GameManager.mobsAlive.Count == 0)
            {
                titleTimer = Time.time;
                nbRound++;
                step = Step.CONSTRUCTION_TITLE;
            }
        }
        else
        {
            if (Time.time > spawnTimer + spawnLaps)
            {
                //nbSpawnInARound++;
                spawner.SpawnMob(nbRound);
                spawnTimer = Time.time;
            }
        }
    }

    private void UpdateRound()
	{
        switch (step)
        {
            case Step.CONSTRUCTION_TITLE :
                ConstructionTitleTime();
                break;
            case Step.CONSTRUCTION :
                ConstructionTime();
                break;
            case Step.ROUND_TITLE :
                RoundTitleTime();
                break;
            case Step.ROUND :
                RoundTime();
                break;
        }
	}

    static public void ShowUpReward(Vector3 showPosition, GameObject textToInstantiate)
    {
        Vector3 worldToScreenPoint = Camera.main.WorldToScreenPoint(showPosition);
        GameObject toDelete = new GameObject();
        GameObject empty = Instantiate(toDelete, worldToScreenPoint, Quaternion.identity) as GameObject;
        empty.transform.name = "RewardWrapper";
        empty.transform.SetParent(GameObject.Find("Canvas").transform);
        GameObject temp = Instantiate(textToInstantiate, worldToScreenPoint, Quaternion.identity) as GameObject;
        temp.transform.SetParent(empty.transform);
        temp.GetComponent<Animator>().SetTrigger("Reward");
        Destroy(temp.gameObject, 1.0F);
        Destroy(empty.gameObject, 1.0F);
        Destroy(toDelete, 1.0F);
    }

    private void UpdateGold()
	{
        txtGold.text = "GOLD : " + gold;
	}
	
	private void UpdateTowerSelection()
	{
        if (step == Step.CONSTRUCTION)
        {
            foreach (TglTower tglTower in choiceTowers.Keys)
            {
                if (gold >= choiceTowers[tglTower].cost)
                    tglTower.enable();
                else
                    tglTower.disable();
            }
        }
	}

    private void retrieveChoiceTowers()
    {
        foreach (Toggle toggle in toggles)
        {
            TglTower tglTower = toggle.GetComponent<TglTower>() as TglTower;
            Tower tower = null;
            Transform sphere = FindSphereInChildren(tglTower.towerToBuild.transform);
            if (sphere != null)
            {
                tower = sphere.GetComponent<Tower>() as Tower;
                choiceTowers.Add(tglTower, tower);
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
		}
        else if (selectedTower != null && towerToSelect != selectedTower) // TODO autoriser cliquage dans zone d'infos de la tour
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
        selectionAura = (GameObject) Instantiate(GameObject.Find/*Resources.Load*/("SelectionAura"), towerToSelect.transform.position, Quaternion.identity);
        selectionAura.transform.SetParent(selectedTower.transform);
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
		DisplayTowerCharacteristic("Sell", "Sell\n+" + tower.sellingPrice);
		GameObject.Find("btnSell").GetComponent<Image>().enabled = true;
		
		Tower towerUp = null;
        if (tower.towerUp != null)
        {
            Transform sphere = FindSphereInChildren(tower.towerUp.transform);
            if (sphere != null)
            {
                towerUp = sphere.GetComponent<Tower>() as Tower;
            }

            if (towerUp)
            {
                DisplayTowerCharacteristic("Up", "Up\n-" + towerUp.cost);

                GameObject gameObjectUp = GameObject.Find("btnUp");
                gameObjectUp.GetComponent<Image>().enabled = true;
                Button btnUp = gameObjectUp.GetComponent<Button>() as Button;
                btnUp.interactable = gold >= towerUp.cost;
            }
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
			else
            {
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
        Destroy(selectionAura);
        selectionAura = null;
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

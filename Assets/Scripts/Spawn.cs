using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class Spawn : MonoBehaviour
{

    public List<GameObject> referenceMobs;

    public static List<Dictionary<GameObject, int>> mobsPerRound;

    // Use this for initialization
    void Start()
    {
        /*GameManager.LoadMobsPerRound();
        foreach (Dictionary<int, int> dic in mobsPerRound)
        {
            foreach (KeyValuePair<int, int> k in dic)
            {
                print(k.Key + " - " + k.Value);
            }
        }*/
        mobsPerRound = new List<Dictionary<GameObject, int>>()
        {
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 4 },
                { referenceMobs[1], 4 },
                { referenceMobs[2], 3 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 3 },
                { referenceMobs[1], 1 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 4 },
                { referenceMobs[1], 1 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 5 },
                { referenceMobs[1], 2 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 6 },
                { referenceMobs[1], 4 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 8 },
                { referenceMobs[1], 4 }
            },
            new Dictionary<GameObject, int>()
            {
                { referenceMobs[0], 8 },
                { referenceMobs[1], 6 }
            }
        };
    }

    public void SpawnMob(int nbRound)
    {
        Dictionary<GameObject, int> mobsToSpawn = mobsPerRound[nbRound - 1];

        // Select a random type of mob
        int randID = UnityEngine.Random.Range(0, mobsToSpawn.Count);

        List<GameObject> keyList = new List<GameObject>(mobsToSpawn.Keys);
        GameObject objRand = keyList[randID];
        Quaternion quater = Quaternion.identity;
        quater.y += 180;
        GameObject _mob = (GameObject)Instantiate(objRand, transform.position, quater);

        GameManager.mobsAlive.Add(_mob);
        mobsToSpawn[objRand]--;
        if (mobsToSpawn[objRand] == 0)
        {
            // when no more of this type of mob to instantiate, remove it
            mobsToSpawn.Remove(objRand);
        }
    }

    public static void notifyMobIsDestroyed(GameObject mobToDelete)
    {
        GameManager.mobsAlive.Remove(mobToDelete);
    }
}

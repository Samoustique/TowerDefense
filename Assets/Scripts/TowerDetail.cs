﻿using UnityEngine;
using System.Collections;

public class TowerDetail : MonoBehaviour {
	
	public AudioClip sellingSound;
	
	public void ClickBtnSell()
	{
		GameManager.sellTower();
		GameObject.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(sellingSound);
	}
	
	public void ClickBtnUp()
	{
		GameManager.upTower();
	}
}

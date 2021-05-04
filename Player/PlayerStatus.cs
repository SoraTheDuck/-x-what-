using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
	[SerializeField] private float damagedelay = 0.25f;

	public Image HPBar;
	public Image DmgEffect;
	public Image MPBar;

	public static float PlayerHP;
	public static float PlayerMP;
	public static float MaxHP;
	public static float MaxMP;


	void Start()
	{
		MaxHP = 100f;
		MaxMP = 5f;
		PlayerHP = MaxHP;
		PlayerMP = MaxMP;
	}

	void Update()
	{
		HPBar.fillAmount = PlayerHP / MaxHP;
		MPBar.fillAmount = PlayerMP / MaxMP;

		if (DmgEffect.fillAmount > HPBar.fillAmount)
		{
			DmgEffect.fillAmount -= damagedelay * Time.deltaTime;
		}
		else
			DmgEffect.fillAmount = HPBar.fillAmount;

		if (PlayerHP < 0.5)
		{
			Debug.Log("U ded");
			PlayerHP = MaxHP; //remove after beta
		}
	}
}

      
        
	
		  
		  


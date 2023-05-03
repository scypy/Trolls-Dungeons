//**************************************************
// NewBehaviourScript.cs
//
// Union Game Studios 2022
//**************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGS
{
	public class NewBehaviourScript : MonoBehaviour
	{
		//Singleton
		public static NewBehaviourScript Instance;

		void Awake()
		{
			if (Instance != null)
				Destroy(gameObject);
			else
				Instance = this;
		}
		
		void Start()
		{
			
		}
	
		void Update()
		{
			
		}
	}
}

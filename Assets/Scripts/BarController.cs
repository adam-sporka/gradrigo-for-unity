using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : MonoBehaviour
{
	int m_SelfDestruct = -1;
	GameController game;

	// Start is called before the first frame update
	void Start()
	{
		game = FindObjectOfType<GameController>();
		gameObject.tag = "bar";
	}
}

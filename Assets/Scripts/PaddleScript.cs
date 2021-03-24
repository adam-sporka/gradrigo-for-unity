using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleScript : MonoBehaviour
{
	GameController game;

	[SerializeField]
	private Rigidbody2D rb;

	////////////////////////////////////////////////////////////////
	void Start()
	{
		game = FindObjectOfType<GameController>();
		rb = GetComponent<Rigidbody2D>();
		gameObject.tag = "paddle";
	}

	////////////////////////////////////////////////////////////////
	public void ResetBeforeGame()
	{
		rb.position.Set(0.0f, rb.position.y);
	}

	////////////////////////////////////////////////////////////////
	void FixedUpdate()
	{
		rb.freezeRotation = true;
		if (game.m_GameScreen == GameController.Screens.GAME)
		{
			Vector2 vel = Vector2.zero;
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				if (transform.position.x > -8.9f) vel += Vector2.left * 20.0f;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				if (transform.position.x < 8.9f) vel += Vector2.right * 20.0f;
			}
			rb.velocity = vel;
		}
		else
		{
			rb.velocity = Vector2.zero;
		}
	}
}

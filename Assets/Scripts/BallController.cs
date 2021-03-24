using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
	GameController game;
	Rigidbody2D rb;
	ArrayList destroyed = new ArrayList();

	////////////////////////////////////////////////////////////////
	void Start()
	{
		game = FindObjectOfType<GameController>();
		rb = GetComponent<Rigidbody2D>();
	}

	////////////////////////////////////////////////////////////////
	public void SetupForGame()
	{
		rb.position = new Vector2(0.0f, 0.0f);
		rb.velocity = Vector2.down * 10.0f;
		destroyed = new ArrayList();
	}

	////////////////////////////////////////////////////////////////
	public void ResetBeforeGame()
	{
		rb.position = new Vector2(0.0f, 0.0f);
		rb.velocity = Vector2.zero;
		destroyed = new ArrayList();
	}

	////////////////////////////////////////////////////////////////
	void FixedUpdate()
	{
		if (game.m_GameScreen == GameController.Screens.GAME)
		{
			if (rb.velocity.magnitude < 10.0f)
			{
				rb.velocity = rb.velocity * (10.0f / rb.velocity.magnitude);
			}

			if (rb.velocity.magnitude > 30.0f)
			{
				rb.velocity = rb.velocity * (30.0f / rb.velocity.magnitude);
			}

			if (Mathf.Abs(rb.velocity.y) < 5.0f)
			{
				rb.velocity += new Vector2(0, rb.velocity.y > 0 ? 5f : -5f);
			}

			if (transform.position.y < -12)
			{
				game.OnBallOut();
			}
		}

		if (game.m_GameScreen == GameController.Screens.VICTORY)
		{
			rb.velocity = Vector2.zero;
		}
	}

	////////////////////////////////////////////////////////////////
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.tag == "bar") {
			if (destroyed.IndexOf(collision.collider.name) < 0)
			{
				collision.gameObject.GetInstanceID();
				game.OnBarDestroyed(collision.gameObject);
				destroyed.Add(collision.collider.name);
				Destroy(collision.collider.gameObject);
			}
		}

		if (collision.collider.tag == "paddle")
		{
			game.m_Gradrigo.StartVoice("paddle_bounce:" + (1.0f * rb.velocity.magnitude).ToString());
		}

		if (collision.collider.tag == "level")
		{
			game.m_Gradrigo.StartVoice("wall_bounce");
		}
	}
}

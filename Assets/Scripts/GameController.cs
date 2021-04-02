using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	public Gradrigo m_Gradrigo;
	public TextAsset m_GradrigoScript;

	// Game objects
	public BallController m_Ball;
	public PaddleScript m_Paddle;
	public GameObject m_BarObject;

	// On-screen GUI
	public Text m_Message;
	public Text m_Score;
	public GameObject m_Logo;

	// Game State
	ArrayList m_Bars = new ArrayList();
	int m_nLives = 3;
	int m_nBarsPlaced = 0;
	int m_nBarsDestroyed = 0;
	float m_fStopwatch = 0.0f;

	string m_LastCountdownText = "";
	float m_MessageCountdown = 0.0f;
	float m_GetReadyCountdown = 0.0f;

	////////////////////////////////////////////////////////////////
	public enum Screens
	{
		NOTHING, INTRO, GET_READY, GAME, MISSED, VICTORY, GAME_OVER
	}

	public Screens m_GameScreen = Screens.NOTHING;

	////////////////////////////////////////////////////////////////
	void Start()
	{
		m_Message.text = "";
		m_Score.text = "";
		m_Gradrigo = GetComponent<Gradrigo>();
		m_Gradrigo.ParseString(m_GradrigoScript.text);
	}

	////////////////////////////////////////////////////////////////
	void ResetGame()
	{
		m_nBarsPlaced = 0;
		m_nBarsDestroyed = 0;
		m_nLives = 3;
		m_Paddle.ResetBeforeGame();
		m_fStopwatch = 0;

		foreach (var a in m_Bars)
		{
			GameObject o = (GameObject)a;
			Destroy(o.gameObject);
		}
		m_Bars.Clear();

		for (int x = -8; x <= 8; x += 2)
		{
			for (float y = 4; y <= 7.6f; y += 1.5f)
			{
				m_nBarsPlaced++;

				var newBar = Instantiate(m_BarObject, new Vector3(x, y, 0.0f), Quaternion.identity);
				newBar.name = "Bar " + m_nBarsPlaced.ToString();
				m_Bars.Add(newBar);
			}
		}
	}

	////////////////////////////////////////////////////////////////
	void SwitchScreen(Screens screen)
	{
		m_GameScreen = screen;

		if (screen == Screens.INTRO)
		{
			m_Ball.ResetBeforeGame();
			ResetGame();
			m_Message.text = "Welcome! Press SPACE to Start";
			m_Score.text = "";
			m_Logo.SetActive(true);

			m_Gradrigo.StartVoice("music_welcome");
		}

		if (screen == Screens.GET_READY)
		{
			m_Ball.ResetBeforeGame();
			m_Message.text = "";
			m_LastCountdownText = "";
			m_GetReadyCountdown = 3.0f;
		}

		if (screen == Screens.MISSED)
		{
			m_Message.text = "Missed! Press SPACE to Continue";
			m_Gradrigo.StartVoice("missed");
		}

		if (screen == Screens.GAME)
		{
			m_Logo.SetActive(false);
			m_Message.text = "GO!";
			m_MessageCountdown = 2f;
			m_Ball.SetupForGame();
		}

		if (screen == Screens.GAME_OVER)
		{
			m_Message.text = "Game Over – Press SPACE to Play Again";
			m_Gradrigo.StartVoice("music_game_over");
		}

		if (screen == Screens.VICTORY)
		{
			m_Message.text = "Congrats! – Press SPACE to Play Again";
			m_Gradrigo.StartVoice("music_victory");
		}
	}

	////////////////////////////////////////////////////////////////
	public void OnBarDestroyed(GameObject obj)
	{
		m_nBarsPlaced--;
		m_nBarsDestroyed++;
		m_Bars.Remove(obj);

		m_Gradrigo.StartVoice("bar_bounce:" + m_nBarsDestroyed.ToString());
	}

	////////////////////////////////////////////////////////////////
	public void OnBallOut()
	{
		m_nLives -= 1;
		if (m_nLives == 0)
		{
			SwitchScreen(Screens.GAME_OVER);
		}
		else
		{
			SwitchScreen(Screens.MISSED);
		}
	}

	////////////////////////////////////////////////////////////////
	void Update()
	{
		if (m_GameScreen == Screens.NOTHING)
		{
			SwitchScreen(Screens.INTRO);
		}

		////////////////////////////////
		else if (m_GameScreen == Screens.INTRO || m_GameScreen == Screens.MISSED)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SwitchScreen(Screens.GET_READY);
			}
		}

		////////////////////////////////
		else if (m_GameScreen == Screens.GAME)
		{
			m_fStopwatch += Time.deltaTime;
			m_Score.text = "Lives left: " + m_nLives.ToString() + " – Time: " + m_fStopwatch.ToString("000.0");

			if (m_MessageCountdown >= 0)
			{
				m_MessageCountdown -= Time.deltaTime;
			}
			else
			{
				m_Message.text = "";
			}

			if (m_nBarsPlaced == 0)
			{
				SwitchScreen(Screens.VICTORY);
			}
		}

		////////////////////////////////
		else if (m_GameScreen == Screens.GET_READY)
		{
			m_GetReadyCountdown -= 3.0f * Time.deltaTime;
			m_Message.text = Mathf.CeilToInt(m_GetReadyCountdown).ToString();

			if (m_LastCountdownText != m_Message.text)
			{
				int note = 60;
				if (m_GetReadyCountdown <= 0.0f) note = 72;

				m_Gradrigo.StartVoice("tick:" + note.ToString());
				m_LastCountdownText = m_Message.text;
			}

			if (m_GetReadyCountdown <= 0.0f)
			{
				SwitchScreen(Screens.GAME);
			}
		}

		////////////////////////////////
		else if (m_GameScreen == Screens.VICTORY || m_GameScreen == Screens.GAME_OVER)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SwitchScreen(Screens.INTRO);
			}			
		}
	}
}

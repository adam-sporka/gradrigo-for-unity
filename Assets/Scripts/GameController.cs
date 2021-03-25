using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	public int m_nLives = 3;
	public int m_nBars = 0;
	public int m_nScore = 0;

	public bool m_ResetBall = false;

	public GameObject m_BarObject;
	public Gradrigo m_Gradrigo;

	public Text m_Message;
	public Text m_Score;
	public GameObject m_Logo;

	public BallController m_Ball;
	public PaddleScript m_Paddle;

	public bool m_bFirstUpdate = true;

	float m_GetReadyCountdown;
	string m_LastCountdownText = "";
	float m_GoMessage = 0.0f;
	int m_nLastMusicVoice = -1;

	ArrayList m_Bars = new ArrayList();

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
	}

	////////////////////////////////////////////////////////////////
	void ResetGame()
	{
		m_nBars = 0;
		m_nScore = 0;
		m_nLives = 3;
		m_Paddle.ResetBeforeGame();

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
				m_nBars++;

				var newBar = Instantiate(m_BarObject, new Vector3(x, y, 0.0f), Quaternion.identity);
				newBar.name = "Bar " + m_nBars.ToString();
				m_Bars.Add(newBar);
			}
		}
	}

	////////////////////////////////////////////////////////////////
	void SwitchScreen(Screens screen)
	{
		m_GameScreen = screen;
		// Do sound here

		if (screen == Screens.INTRO)
		{
			m_Ball.ResetBeforeGame();
			ResetGame();
			m_Message.text = "Welcome! Press SPACE to Start";
			m_Score.text = "";
			m_Logo.SetActive(true);
			m_Gradrigo.StopVoice(m_nLastMusicVoice);
			m_nLastMusicVoice = m_Gradrigo.StartVoice("music_welcome");
			Debug.Log(m_nLastMusicVoice);
		}

		if (screen == Screens.GET_READY)
		{
			m_Gradrigo.StopVoice(m_nLastMusicVoice);
			m_Ball.ResetBeforeGame();
			m_Message.text = "";
			m_LastCountdownText = "";
			m_GetReadyCountdown = 3.0f;
		}

		if (screen == Screens.MISSED)
		{
			m_Message.text = "Missed! Press SPACE to Continue";
			m_Gradrigo.StopVoice(m_nLastMusicVoice);
			m_nLastMusicVoice = m_Gradrigo.StartVoice("missed");
		}

		if (screen == Screens.GAME)
		{
			m_Logo.SetActive(false);
			m_GoMessage = 2f;
			m_Message.text = "GO!";
			m_Ball.SetupForGame();
		}

		if (screen == Screens.GAME_OVER)
		{
			m_Message.text = "Game Over – Press SPACE to Play Again";
			m_Gradrigo.StopVoice(m_nLastMusicVoice);
			m_nLastMusicVoice = m_Gradrigo.StartVoice("music_game_over");
		}

		if (screen == Screens.VICTORY)
		{
			m_Message.text = "Congrats! – Press SPACE to Play Again";
			m_Gradrigo.StopVoice(m_nLastMusicVoice);
			m_nLastMusicVoice = m_Gradrigo.StartVoice("music_victory");
		}
	}

	////////////////////////////////////////////////////////////////
	public void OnBarDestroyed(GameObject obj)
	{
		m_nBars--;
		m_nScore++;
		m_Gradrigo.StartVoice("bar_bounce:" + m_nScore.ToString());
		m_Bars.Remove(obj);
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
	void FirstUpdate()
	{
		m_Gradrigo.ParseString(@"

wall_bounce = dur(~0.15) {
	fadeout(~0.15)
	%d = 40
	@loop
	noise17e(%d)
}

paddle_bounce:%d = dur(~0.2) {
	fadeout(~0.2)
	%a = #60 - %d
	@loop
	sqr(%a)
	%a = %a - ? * 0.004
}

bar_bounce:%d = dur(~0.2) {
	fadeout(~0.2)
	%r = #(72 + %d)
	@loop
	dur (~0.04) {
		fadeout(~0.04)
		@loop sqr(%r)
	}
	dur (~0.04) {
		@loop `5`
	}
}

missed_note:%m = dur(!0.33) {
	vol(0.0)
	fadein(!0.33)
	@loop
	noise5(#%m)
}

tick:%d = {
	dur(~0.05) { @loop sqr(#%d) }
}

missed = {
	vol(0.5)
	missed_note: C6 B5 A#5
}

note:%d:%w = {
	%d = !(%d * .25)
	dur(%d) {
		fadeout(%d)
		@loop sqr(#%w)
	}
}

music_welcome = {
	note:
		6: A4 
	vol(.75) 
		6: D4 
		2: 0
		1: A4 C5
	vol(.9) 
		2: B4 A4 G4
	vol(.7)
		4: D4
}

music_victory = {
	note:
		6: A4 
	vol(.75) 
		6: D5
		2: 0
		1: D4 F#4
	vol(.9) 
		2: G4 A4 B4
	vol(.7)
		4: A4
}

music_game_over = {
	note:
		6: A4 
	vol(.75) 
		6: A3
		2: 0
		1: A3 C4
	vol(.9) 
		2: D4 C4 C4
	vol(.7)
		4: D4
}"

		);
	}

	////////////////////////////////////////////////////////////////
	void Update()
	{
		if (m_bFirstUpdate)
		{
			FirstUpdate();
			m_bFirstUpdate = false;
		}

		if (m_GameScreen == Screens.NOTHING)
		{
			SwitchScreen(Screens.INTRO);
		}
		else if (m_GameScreen == Screens.INTRO || m_GameScreen == Screens.MISSED)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SwitchScreen(Screens.GET_READY);
			}
		}
		else if (m_GameScreen == Screens.GAME)
		{
			m_Score.text = "Lives left: " + m_nLives.ToString();

			if (m_GoMessage >= 0)
			{
				m_GoMessage -= Time.deltaTime;
			}
			else
			{
				m_Message.text = "";
			}

			if (m_nBars == 0)
			{
				SwitchScreen(Screens.VICTORY);
			}
		}
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
		else if (m_GameScreen == Screens.VICTORY || m_GameScreen == Screens.GAME_OVER)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SwitchScreen(Screens.INTRO);
			}			
		}
	}
}

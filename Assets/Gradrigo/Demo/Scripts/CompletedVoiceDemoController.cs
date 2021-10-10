using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletedVoiceDemoController : MonoBehaviour
{
	private Gradrigo m_Gradrigo;
	private int m_LastVoiceId;

	// Start is called before the first frame update
	void Start()
	{
		m_Gradrigo = GetComponent<Gradrigo>();
		m_Gradrigo.ParseString("one_second = dur(~1) { fadeout(~1) @loop sqr(#C4) }");
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_LastVoiceId = m_Gradrigo.StartVoice("one_second");
		}

		if (m_Gradrigo.IsVoiceCompleted(m_LastVoiceId))
		{
			Debug.Log($"Voice {m_LastVoiceId} completed");
		}

		m_Gradrigo.ResetVoicePolling();
	}
}

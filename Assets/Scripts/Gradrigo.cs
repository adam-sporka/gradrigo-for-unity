// +-----------------+
// |   gradrigo.cs   |
// +-----------------+
//
// Unity Interface for Gradrigo
// by Adam Sporka
//
// (Currently for Windows only)
//
// Check the website
// https://adam.sporka.eu/gradrigo.html
//
// Please support Gradrigo on Patreon
// https://patreon.com/adam_sporka

using UnityEngine;
using System.Runtime.InteropServices;

public class Gradrigo : MonoBehaviour
{
	[DllImport("gradrigo.dll", EntryPoint = "NewInstance")]
	private static extern int NewInstance(int sample_rate);

	[DllImport("gradrigo.dll", EntryPoint = "DestroyInstance")]
	private static extern void DestroyInstance(int id);

	[DllImport("gradrigo.dll", EntryPoint = "GetBuffer")]
	private static extern int GetBuffer(int num_samples, [In, Out] float[] output, int id);

	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "ParseString")]
	public static extern int _ParseString([MarshalAs(UnmanagedType.LPStr)] string gradrigo_source_code, int id);

	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "StartVoice")]
	public static extern int _StartVoice([MarshalAs(UnmanagedType.LPStr)] string box_string, int id);

	[DllImport("gradrigo.dll", EntryPoint = "ReleaseVoice")]
	public static extern void _ReleaseVoice(int voice_id, int id);

	[DllImport("gradrigo.dll", EntryPoint = "StopVoice")]
	public static extern void _StopVoice(int voice_id, int id);

	[DllImport("gradrigo.dll", EntryPoint = "StopAllVoices")]
	public static extern void _StopAllVoices(int id);

	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "GetResponseString")]
	[return: MarshalAs(UnmanagedType.LPStr)]
	public static extern string _GetResponseString(int request_id, int id);

	[DllImport("gradrigo.dll", EntryPoint = "MidiNoteOn")]
	public static extern void _MidiNoteOn(int note, int velocity, int id);

	[DllImport("gradrigo.dll", EntryPoint = "MidiNoteOff")]
	public static extern void _MidiNoteOff(int note, int id);

	[DllImport("gradrigo.dll", EntryPoint = "SetVariable")]
	public static extern void _SetVariable([MarshalAs(UnmanagedType.LPStr)] string variable_name, float value, int id);

	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "ReportBoxesAsJson")]
	[return: MarshalAs(UnmanagedType.LPStr)]
	public static extern string _ReportBoxesAsJson(int id);

	////////////////////////////////////////////////////////////////
	public int StartVoice(string voice)
	{
		return _StartVoice(voice, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	public int ParseString(string gradrigo_source_code)
	{
		if (!m_bRunning)
		{
			m_sTextToParse = gradrigo_source_code;
			return 0;
		}

		int result = _ParseString(gradrigo_source_code, m_InstanceID);
		return result;
	}

	////////////////////////////////////////////////////////////////
	public void ReleaseVoice(int id)
	{
		_ReleaseVoice(id, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	public void StopVoice(int id)
	{
		_StopVoice(id, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	public void OnAudioFilterRead(float[] data, int channels)
	{
		if (!m_bRunning)
		{
			return;
		}

		int num_frames = data.Length / channels;
		float[] temp = new float[num_frames];
		int num = GetBuffer(num_frames, temp, m_InstanceID);

		int i = 0;
		for (int n = 0; n < num_frames; n++)
		{
			for (int c = 0; c < channels; c++)
			{
				data[i] *= temp[n];
				i++;
			}
		}
	}

	////////////////////////////////////////////////////////////////
	public void Start()
	{
		var audio_configuration = AudioSettings.GetConfiguration();
		audio_configuration.dspBufferSize = 512;
		AudioSettings.Reset(audio_configuration);

		m_InstanceID = NewInstance(audio_configuration.sampleRate);

		var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);
		dummy.SetData(new float[] { 1 }, 0);

		AudioSource TargetSrc = GetComponent<AudioSource>();
		TargetSrc.clip = dummy; //just to let unity play the audiosource
		TargetSrc.loop = true;
		TargetSrc.spatialBlend = 1;
		TargetSrc.Play();

		m_bRunning = true;

		if (m_sTextToParse.Length > 0)
		{
			ParseString(m_sTextToParse);
			m_sTextToParse = "";
		}
	}

	////////////////////////////////////////////////////////////////
	public void SetVariable(string name, float value)
	{
		if (!m_bRunning)
		{
			return;
		}

		_SetVariable(name, value, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	public void OnDestroy()
	{
		DestroyInstance(m_InstanceID);
	}

	public int m_InstanceID;

	bool m_bRunning;
	string m_sTextToParse;
}

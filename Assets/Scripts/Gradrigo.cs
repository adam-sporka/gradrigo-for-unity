// Overall tutorial
// https://www.alanzucconi.com/2015/10/11/how-to-write-native-plugins-for-unity/

// More detailed reference for data types
// https://bravenewmethod.com/2017/10/30/unity-c-native-plugin-examples/

// Making the playback 3D
// https://stackoverflow.com/questions/38843408/realtime-3d-audio-streaming-and-playback

using UnityEngine;
using System.Runtime.InteropServices;
 
public class Gradrigo : MonoBehaviour
{
#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "NewInstance")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "NewInstance")]
#endif
	private static extern int NewInstance(int sample_rate);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "DestroyInstance")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "DestroyInstance")]
#endif
	private static extern void DestroyInstance(int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "GetBuffer")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "GetBuffer")]
#endif
	private static extern int GetBuffer(int num_samples, [In, Out] float[] output, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "ParseString")]
#else 
	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "ParseString")]
#endif
	public static extern int _ParseString([MarshalAs(UnmanagedType.LPStr)] string gradrigo_source_code, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "StartVoice")]
#else 
	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "StartVoice")]
#endif
	public static extern int _StartVoice([MarshalAs(UnmanagedType.LPStr)] string box_string, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "ReleaseVoice")]
#else
	[DllImport("gradrigo.dll", EntryPoint = "ReleaseVoice")]
#endif
	public static extern void _ReleaseVoice(int voice_id, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "StopVoice")]
#else
	[DllImport("gradrigo.dll", EntryPoint = "StopVoice")]
#endif
	public static extern void _StopVoice(int voice_id, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "StopAllVoices")]
#else
	[DllImport("gradrigo.dll", EntryPoint = "StopAllVoices")]
#endif
	public static extern void _StopAllVoices(int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "GetResponseString")]
#else 
	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "GetResponseString")]
#endif
	[return: MarshalAs(UnmanagedType.LPStr)]
	public static extern string _GetResponseString(int request_id, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "MidiNoteOn")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "MidiNoteOn")]
#endif
	public static extern void _MidiNoteOn(int note, int velocity, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "MidiNoteOff")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "MidiNoteOff")]
#endif
	public static extern void _MidiNoteOff(int note, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", EntryPoint = "SetVariable")]
#else 
	[DllImport("gradrigo.dll", EntryPoint = "SetVariable")]
#endif
	public static extern void _SetVariable([MarshalAs(UnmanagedType.LPStr)] string variable_name, float value, int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR 
	[DllImport("__Internal", CharSet = CharSet.Ansi, EntryPoint = "ReportBoxesAsJson")]
#else 
	[DllImport("gradrigo.dll", CharSet = CharSet.Ansi, EntryPoint = "ReportBoxesAsJson")]
#endif
	[return: MarshalAs(UnmanagedType.LPStr)]
	public static extern string _ReportBoxesAsJson(int id);

#if (UNITY_IPHONE || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "StopAllVoices")]
#else
	[DllImport("gradrigo.dll", EntryPoint = "AmIAlive")]
#endif
	public static extern int _AmIAlive();

	////////////////////////////////////////////////////////////////
	public int StartVoice(string voice)
	{
		return _StartVoice(voice, m_nGradrigoInstanceId);
	}

	////////////////////////////////////////////////////////////////
	public int ParseString(string gradrigo_source_code)
	{
		if (!running)
		{
			return 0;
		}

		int result = _ParseString(gradrigo_source_code, m_nGradrigoInstanceId);
		return result;
	}

	////////////////////////////////////////////////////////////////
	public void ReleaseVoice(int id)
	{
		_ReleaseVoice(id, m_nGradrigoInstanceId);
	}

	////////////////////////////////////////////////////////////////
	public void StopVoice(int id)
	{
		_StopVoice(id, m_nGradrigoInstanceId);
	}

	////////////////////////////////////////////////////////////////
	public void OnAudioFilterRead(float[] data, int channels)
	{
		if (!running)
		{
			return;
		}

		int num_frames = data.Length / channels;
		float[] temp = new float[num_frames];
		int num = GetBuffer(num_frames, temp, m_nGradrigoInstanceId);

		int i = 0;
		for (int n = 0; n < num_frames; n++)
		{
			for (int c = 0; c<channels; c++)
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

		m_nGradrigoInstanceId = NewInstance(audio_configuration.sampleRate);
		Debug.Log("Gradrigo instance ID will be " + m_nGradrigoInstanceId);

		var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);
		dummy.SetData(new float[] { 1 }, 0);

		AudioSource TargetSrc = GetComponent<AudioSource>();
		TargetSrc.clip = dummy; //just to let unity play the audiosource
		TargetSrc.loop = true;
		TargetSrc.spatialBlend = 1;
		TargetSrc.Play();

		running = true;
	}

	////////////////////////////////////////////////////////////////
	public void SetVariable(string name, float value)
	{
		if (!running)
		{
			return;
		}

		_SetVariable(name, value, m_nGradrigoInstanceId);
	}

	////////////////////////////////////////////////////////////////
	public void OnDestroy()
	{
		Debug.Log("Destroying instance ID " + m_nGradrigoInstanceId);
		DestroyInstance(m_nGradrigoInstanceId);
	}

	public int m_nGradrigoInstanceId;
	public bool running;
}

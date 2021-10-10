// +-----------------+
// |   gradrigo.cs   |
// +-----------------+
//
// Unity for Gradrigo 0.0.5
// by Adam Sporka
//
// Work in progress, currently for Windows only
//
// Check the website for the latest version and for licensing options
// https://adam.sporka.eu/gradrigo.html
//
// Please support Gradrigo on Patreon
// https://patreon.com/adam_sporka
//
// Gradrigo is provided by the copyright holders and contributors "as is"
// and any express or implied warranties, including, but not limited to,
// the implied warranties of merchantability and fitness for
// a particular purpose are disclaimed. in no event shall the copyright
// holder or contributors be liable for any direct, indirect, incidental,
// special, exemplary, or consequential damages (including,
// but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out
// of the use of this software, even if advised of the possibility of such
// damage. 

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

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

	[DllImport("gradrigo.dll", EntryPoint = "EnableCompletedVoicePolling")]
	public static extern void _EnableCompletedVoicePolling(bool enable, int id);

	[DllImport("gradrigo.dll", EntryPoint = "PollCompletedVoice")]
	public static extern int _PollCompletedVoice(int id);

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Have the Gradrigo engine parse the given script. You may make
	/// multiple calls to this method. You may even redefine the 
	/// same boxes at run-time, even though heavy parsing is discouraged
	/// as it's performed directly in the audio thread. The safest
	/// way is to call this during the initialization of your game.
	/// </summary>
	/// <param name="gradrigo_source_code">
	/// Gradrigo code. To learn the syntax, please check the tutorial
	/// in the Gradrigo Standalone App or read
	/// https://adam.sporka.eu/gradrigo-language-manual.html
	/// </param>
	public void ParseString(string gradrigo_source_code)
	{
		if (!m_bRunning)
		{
			m_sTextToParse = m_sTextToParse + "\n" + gradrigo_source_code;
			return;
		}
		_ParseString(gradrigo_source_code, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Start an existing box (audio event) in a new voice
	/// </summary>
	/// <param name="text">The box is specified as a string followed by parameters.
	/// e.g. "blip:60" would start box "blib" and pass 60 as a parameter to it.
	/// The need for parameters depends on the Gradrigo script (see ParseString).
	/// See Gradrigo built-in tutorial for more.</param>
	/// <returns>Voice ID to use with ReleaseVoice and StopVoice if needed</returns>
	public int StartVoice(string text)
	{
		return _StartVoice(text, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Release the given voice, i.e. enter the release phase of the playback.
	/// The voice would eventually stop playing. See ** @release LABEL ** in
	/// gradrigo-language-manual.html
	/// </summary>
	/// <param name="id">ID obtained by StartVoice</param>
	public void ReleaseVoice(int id)
	{
		_ReleaseVoice(id, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Stop the given voice immediately.
	/// </summary>
	/// <param name="id">ID obtained by StartVoice</param>
	public void StopVoice(int id)
	{
		_StopVoice(id, m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Stop all voices immediately. (The computer musicians call this
	/// the "panic button".)
	/// </summary>
	public void StopAllVoices()
	{
		_StopAllVoices(m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	/// <summary>
	/// Set a global variable to a given value.
	/// To be used after the engine started producing sound.
	/// </summary>
	/// <param name="name">Name of the global variable</param>
	/// <param name="value">Target value</param>
	public void SetVariable(string name, float value)
	{
		if (!m_bRunning) return;
		_SetVariable(name, value, m_InstanceID);
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
		for (int f = 0; f < num_frames; f++)
		{
			for (int c = 0; c < channels; c++)
			{
				data[i] *= temp[f];
				i++;
			}
		}

		int completed = _PollCompletedVoice(m_InstanceID);
		while (completed != 0)
		{
			m_Completed.Add(completed);
			completed = _PollCompletedVoice(m_InstanceID);
		}
	}

	////////////////////////////////////////////////////////////////
	public bool IsVoiceCompleted(int VoiceId)
	{
		foreach (int id in m_Completed)
		{
			if (id == VoiceId)
			{
				m_Completed.Remove(id);
				return true;
			}
		}

		return false;
	}

	////////////////////////////////////////////////////////////////
	public void ResetVoicePolling()
	{
		m_Completed.Clear();
	}

	////////////////////////////////////////////////////////////////
	public void Start()
	{
		var audio_configuration = AudioSettings.GetConfiguration();
		audio_configuration.dspBufferSize = 512;
		AudioSettings.Reset(audio_configuration);

		m_InstanceID = NewInstance(audio_configuration.sampleRate);
		_EnableCompletedVoicePolling(true, m_InstanceID);

		var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);
		dummy.SetData(new float[] { 1 }, 0);

		AudioSource TargetSrc = GetComponent<AudioSource>();
		TargetSrc.clip = dummy;
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
	public void OnDestroy()
	{
		DestroyInstance(m_InstanceID);
	}

	////////////////////////////////////////////////////////////////
	private ArrayList m_Completed = new ArrayList();
	public int m_InstanceID = 0;
	string m_sTextToParse = "";
	bool m_bRunning = false;
}

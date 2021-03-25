using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Gradrigo))]
public class GradrigoEditor : Editor
{
	string text;
	string voice;

	public override void OnInspectorGUI()
	{
		// base.OnInspectorGUI();
		Gradrigo myGradrigo = (Gradrigo)target;
		if (myGradrigo) 
		{
			EditorGUILayout.IntField("Instance ID", myGradrigo.m_nGradrigoInstanceId);
			text = EditorGUILayout.TextArea(text, GUILayout.Height(100));
			if (GUILayout.Button("Parse"))
			{
				myGradrigo.ParseString(text);
			}
			voice = EditorGUILayout.TextField("Test voice parameters", voice);
			if (GUILayout.Button("Start Voice"))
			{
				myGradrigo.StartVoice(voice);
			}
		}
	}
}

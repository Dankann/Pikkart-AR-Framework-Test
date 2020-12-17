using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PikkartAR
{
	[CustomEditor(typeof(CloudMarkerObject), true)]
	[CanEditMultipleObjects]
	public class CloudMarkerObjectEditor : Editor {

	    PikkartMain mainScript;
		CloudMarkerObject cloudMarkerObject;

		SerializedProperty cloudMarkerIdSP;
        SerializedProperty cloudMarkerARLogoPatternSP;

        SerializedProperty OnCloudMarkerFoundEventsSP;
        SerializedProperty OnCloudMarkerLostEventsSP;
        SerializedProperty OnCloudMarkerPatternCodeFoundEventsSP;

        void OnEnable(){
			mainScript = FindObjectOfType<PikkartMain> ();
			cloudMarkerObject = (CloudMarkerObject)target;
			cloudMarkerIdSP = serializedObject.FindProperty ("cloudMarkerId");
            cloudMarkerARLogoPatternSP = serializedObject.FindProperty("cloudMarkerARLogoPattern");
            OnCloudMarkerFoundEventsSP = serializedObject.FindProperty("OnCloudMarkerFoundEvents");
            OnCloudMarkerLostEventsSP = serializedObject.FindProperty("OnCloudMarkerLostEvents");
            OnCloudMarkerPatternCodeFoundEventsSP = serializedObject.FindProperty("OnCloudMarkerPatternCodeFoundEvents");
        }

		public override void OnInspectorGUI(){
            myOnInspectorGUI();
		}

        public void myOnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(cloudMarkerIdSP);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(cloudMarkerARLogoPatternSP);
            //Events Listeners
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(OnCloudMarkerFoundEventsSP);
            EditorGUILayout.PropertyField(OnCloudMarkerLostEventsSP);
            EditorGUILayout.PropertyField(OnCloudMarkerPatternCodeFoundEventsSP);

            serializedObject.ApplyModifiedProperties();
        }
	}
}
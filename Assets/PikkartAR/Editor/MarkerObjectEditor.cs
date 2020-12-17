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
	[CustomEditor(typeof(MarkerObject), true)]
	[CanEditMultipleObjects]
	public class MarkerObjectEditor : Editor {

	    PikkartMain mainScript;
		MarkerObject markerObject;

		SerializedProperty markerIdSP;
		SerializedProperty markerIndexSP;
        SerializedProperty markerARLogoPatternSP;

        SerializedProperty OnMarkerFoundEventsSP;
        SerializedProperty OnMarkerLostEventsSP;
        SerializedProperty OnMarkerPatternCodeFoundEventsSP;

        void OnEnable(){
			mainScript = FindObjectOfType<PikkartMain> ();
			markerObject = (MarkerObject)target;
			markerIdSP = serializedObject.FindProperty ("markerId");
            markerARLogoPatternSP = serializedObject.FindProperty("markerARLogoPattern");
            markerIndexSP = serializedObject.FindProperty ("markerIndex");
            OnMarkerFoundEventsSP = serializedObject.FindProperty("OnMarkerFoundEvents");
            OnMarkerLostEventsSP = serializedObject.FindProperty("OnMarkerLostEvents");
            OnMarkerPatternCodeFoundEventsSP = serializedObject.FindProperty("OnMarkerPatternCodeFoundEvents");
        }

		public override void OnInspectorGUI(){
            myOnInspectorGUI();
		}

        public void myOnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Import Markers"))
            {
                mainScript.Check();
                EditorUtility.SetDirty(mainScript);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            // se non ci sono marker importati esco (mostro solo il pulsante)
            //if (mainScript.GetMarkerList().Length < 2) {
            //	return;
            //}

            // popolo la lista di marker
            string[] choices = new string[mainScript.GetMarkerList().Length];
            int index = 0;
            foreach (string marker in mainScript.GetMarkerList())
            {
                if (marker == "")
                    choices[index] = "---";
                else
                    choices[index] = marker;
                index++;
            }

            // mostro il selettore
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Marker id:");

            // verifico match tra marker e index
            if (markerIndexSP.intValue > 0 || markerIdSP.stringValue != "")
            {
                int i = IndexOf(markerIdSP.stringValue, mainScript.GetMarkerList());
                if (i != -1 && markerIndexSP.intValue != i)
                {
                    // marker e' in lista ma con index sbagliato
                    markerIndexSP.intValue = i;
                }
                else if (i == -1)
                {
                    // marker non in lista
                    ResetIndex();
                }
                //marker e' in lista con id giusto non faccio nulla
            }

            int currentChoice = -1;
            if (mainScript.GetMarkerList().Length < 2 || (markerIndexSP.intValue == 0 && markerIdSP.stringValue == ""))
            {
                currentChoice = EditorGUILayout.Popup(0, choices);
                SetNewMarker(currentChoice);
            }
            else
            {
                currentChoice = EditorGUILayout.Popup(markerIndexSP.intValue, choices);
                if (currentChoice != markerIndexSP.intValue)
                {
                    SetNewMarker(currentChoice);
                }
            }

            

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(markerARLogoPatternSP);

            //Events Listeners
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(OnMarkerFoundEventsSP);
            EditorGUILayout.PropertyField(OnMarkerLostEventsSP);
            EditorGUILayout.PropertyField(OnMarkerPatternCodeFoundEventsSP);

            serializedObject.ApplyModifiedProperties();
        }

		public void SetNewMarker(int newIndex){
			markerIndexSP.intValue = newIndex;
			markerIdSP.stringValue = mainScript.GetMarkerList()[markerIndexSP.intValue];

			string filepath = Application.dataPath + "/Editor/markerImages/" + markerIdSP.stringValue + ".jpg";
			if (markerIndexSP.intValue == 0)
            {
                //filepath = Application.dataPath + "/Editor/markerImages/placeholder.jpg";
                filepath = Application.dataPath + "/PikkartAR/Assets/Images/placeholder.jpg";
                markerIdSP.stringValue = "";
			}

            // applicazione material
            var material = new Material(Shader.Find("Diffuse"));
            material.mainTexture = IMG2Sprite.instance.LoadTexture(filepath);
            if(markerObject.GetComponentInChildren<ReferenceImage>() != null)
                markerObject.GetComponentInChildren<ReferenceImage>().gameObject.
                    GetComponent<MeshRenderer>().material = material;

            string id = markerIdSP.stringValue;
            MarkerData container = new MarkerData();
            if (id == "")
            {
                container.name = "0";
                container.width = 1;
                container.height = 1;
            }
            else
            {
                //caricamento xml e ridimensionamento
                string xmlPath = Application.streamingAssetsPath + "/xmls/" + id + ".xml";

                var serializer = new XmlSerializer(typeof(MarkerData));
                var stream = new FileStream(xmlPath, FileMode.Open);
                container = serializer.Deserialize(stream) as MarkerData;
                stream.Close();
            }

            // riposizionamento e scala del quad
            if (markerObject.GetComponentInChildren<ReferenceImage>() != null)
            {
                Transform referenceImageTransform = markerObject.GetComponentInChildren<ReferenceImage>().transform;

                Vector3 newScale = new Vector3(container.width, container.height, 1);
                referenceImageTransform.localScale = newScale;

                referenceImageTransform.position = Vector3.zero;

                Vector3 newCenter = new Vector3(0 - container.width / 2, 0, container.height / 2);
                referenceImageTransform.localPosition = newCenter;
            }
        }

		public void ResetIndex(){
			markerIndexSP.intValue = -1;
			markerIdSP.stringValue = "";
		}

		public int IndexOf(string markerId, string[] markerList){
			int i = 0;
			foreach (string id in markerList) {
				if (markerId == id) {
					return i;
				}
				i++;
			}
			return -1;
		}
	}
}
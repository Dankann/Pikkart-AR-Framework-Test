using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PikkartAR
{
	[CustomEditor(typeof(PikkartMain), true)]
	[CanEditMultipleObjects]
	public class PikkartMainEditor : Editor {

	    PikkartMain mainScript;
        SerializedProperty recognitionStorageOptionSP;
        SerializedProperty recognitionModeOptionSP;
        SerializedProperty databasesSP;
#pragma warning disable 0414
        SerializedProperty markerListSP;
        SerializedProperty discoverListSP;
#pragma warning restore 0414
        SerializedProperty useWebcamOnEditorSP;

        SerializedProperty isSeeThroughDeviceSP;
        SerializedProperty seeThroughFramePredictionSP;

        void OnEnable(){
	        mainScript = (PikkartMain)target;
            recognitionStorageOptionSP = serializedObject.FindProperty("recognitionStorage");
            recognitionModeOptionSP = serializedObject.FindProperty("recognitionMode");
            databasesSP = serializedObject.FindProperty("databases");
            markerListSP = serializedObject.FindProperty("markerList");
            discoverListSP = serializedObject.FindProperty("discoverList");
            useWebcamOnEditorSP = serializedObject.FindProperty("useWebcamOnEditor");
            isSeeThroughDeviceSP = serializedObject.FindProperty("isSeeThroughDevice");
            seeThroughFramePredictionSP = serializedObject.FindProperty("seeThroughFramePrediction");
        }

		public override void OnInspectorGUI(){
            serializedObject.Update();

            EditorGUILayout.PropertyField(useWebcamOnEditorSP);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Recognition Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(recognitionStorageOptionSP);
            EditorGUILayout.PropertyField(recognitionModeOptionSP);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Databases", EditorStyles.boldLabel);
            for (int i = 0; i < databasesSP.arraySize; i++)
            {
                GUILayout.BeginHorizontal();

                string selection = EditorGUILayout.TextField(databasesSP.GetArrayElementAtIndex(i).stringValue);
                databasesSP.GetArrayElementAtIndex(i).stringValue = selection;

                if (GUILayout.Button("X", GUILayout.Width(20f)))
                {
                    databasesSP.DeleteArrayElementAtIndex(i);
                }

                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add a database"))
            {
                databasesSP.InsertArrayElementAtIndex(databasesSP.arraySize);
                databasesSP.GetArrayElementAtIndex(databasesSP.arraySize - 1).stringValue = "";
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Markers", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Imported markers:");
            foreach (string marker in mainScript.GetMarkerList())
            {
                EditorGUILayout.LabelField(marker);
            }
            EditorGUILayout.LabelField("Discover Models", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Imported discover models:");
            foreach (string discover in mainScript.GetDiscoverList())
            {
                EditorGUILayout.LabelField(discover);
            }
            if (GUILayout.Button("Import Local Markers"))
            {
                mainScript.Check();
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            markerListSP = serializedObject.FindProperty("markerList");
            discoverListSP = serializedObject.FindProperty("discoverList");

            EditorGUILayout.PropertyField(isSeeThroughDeviceSP);
            EditorGUILayout.PropertyField(seeThroughFramePredictionSP);

            serializedObject.ApplyModifiedProperties();
        }
	}
}
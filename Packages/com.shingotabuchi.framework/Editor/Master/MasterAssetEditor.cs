using UnityEditor;
using UnityEngine;
using Fwk.Master;
using System;

namespace Fwk.Editor
{
    [CustomEditor(typeof(MasterAsset<>), true)]
    public class MasterAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty dataList;
        private bool dataExpanded = true;

        private void OnEnable()
        {
            dataList = serializedObject.FindProperty("_data");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default inspector for other properties
            DrawPropertiesExcluding(serializedObject, "_data");

            // Draw data list with custom formatting
            if (dataList != null)
            {
                EditorGUILayout.BeginVertical();

                // Create a foldout header for "Data"
                dataExpanded = EditorGUILayout.Foldout(dataExpanded, $"Data ({dataList.arraySize})", true);

                if (dataExpanded)
                {
                    EditorGUI.indentLevel++;

                    // Draw array elements with indices
                    for (int i = 0; i < dataList.arraySize; i++)
                    {
                        SerializedProperty element = dataList.GetArrayElementAtIndex(i);
                        string name = element.displayName;

                        // Create a label with the index
                        string label = $"[{i + 1}] {name}";

                        EditorGUILayout.PropertyField(element, new GUIContent(label), true);
                    }

                    EditorGUI.indentLevel--;

                    // Add buttons row
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        dataList.InsertArrayElementAtIndex(dataList.arraySize);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        if (dataList.arraySize > 0)
                            dataList.DeleteArrayElementAtIndex(dataList.arraySize - 1);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                DrawDefaultInspector();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

using QvPen.UdonScript;
using System.Collections.Generic;
using VRC.SDKBase;

namespace Swishf.AutoPen
{
    [CustomEditor(typeof(AutoPenManager))]
    public class AutoPenManagerEditor : Editor
    {
        SerializedProperty _penManagers, _pens, _penPickups;

        private void OnEnable()
        {
            _penManagers = serializedObject.FindProperty("_penManagers");
            _pens = serializedObject.FindProperty("_pens");
            _penPickups = serializedObject.FindProperty("_penPickups");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("QvPen 自動検索"))
            {
                var foundManagers = new List<QvPen_PenManager>();
                var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in rootObjs)
                {
                    var managers = root.GetComponentsInChildren<QvPen_PenManager>();
                    foundManagers.AddRange(managers);
                }

                _penManagers.ClearArray();
                _penManagers.arraySize = foundManagers.Count;
                _pens.ClearArray();
                _pens.arraySize = foundManagers.Count;
                _penPickups.ClearArray();
                _penPickups.arraySize = foundManagers.Count;
                for (int i = 0; i < _penManagers.arraySize; i++)
                {
                    var serializedManager = _penManagers.GetArrayElementAtIndex(i);
                    serializedManager.boxedValue = foundManagers[i];

                    FieldInfo fieldInfo = typeof(QvPen_PenManager).GetField("pen", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo == null)
                    {
                        continue;
                    }

                    var pen = fieldInfo.GetValue(foundManagers[i]) as QvPen_Pen;
                    if (pen == null)
                    {
                        continue;
                    }

                    var serializedPen = _pens.GetArrayElementAtIndex(i);
                    serializedPen.objectReferenceValue = pen;

                    var serializedPenPickups = _penPickups.GetArrayElementAtIndex(i);
                    serializedPenPickups.objectReferenceValue = pen.GetComponent<VRC_Pickup>();
                }
            }

            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();
        }
    }
}
#endif

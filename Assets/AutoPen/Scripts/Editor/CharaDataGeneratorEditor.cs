#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Swishf.AutoPen
{
    [CustomEditor(typeof(CharaDataGenerator))]
    public class CharaDataGeneratorEditor : Editor
    {
        CharaDataGenerator _target;

        private void OnEnable()
        {
            _target = target as CharaDataGenerator;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("データ生成"))
            {
                MethodInfo method = typeof(CharaDataGenerator).GetMethod("DrawDataToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var paramters = new object[] { };
                method.Invoke(_target, paramters);
            }

            if (GUILayout.Button("トランスフォームをリセット"))
            {
                ResetPenCharDataTransforms(_target.transform);
                EditorUtility.SetDirty(_target.gameObject);
            }

            // if (GUILayout.Button("メタデータ編集"))
            // {
            //     foreach (Transform drawDataObj in _target.transform)
            //     {
            //         foreach (Transform lineGroupObj in drawDataObj)
            //         {
            //             foreach (Transform lineObj in lineGroupObj)
            //             {
            //                 lineObj.name = $"0,1,{lineObj.name}";
            //             }
            //         }
            //     }
            //     EditorUtility.SetDirty(_target.gameObject);
            // }
            DrawDefaultInspector();
        }

        public static void ResetPenCharDataTransforms(Transform charaDataObj, bool changeName = true)
        {
            foreach (Transform drawDataObj in charaDataObj)
            {
                DrawDataGeneratorEditor.ResetDrawDataTransforms(drawDataObj, isCharData: true, changeName: changeName);
            }
        }
    }
}
#endif

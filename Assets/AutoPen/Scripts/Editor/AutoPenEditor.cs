#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Swishf.AutoPen
{
    [CustomEditor(typeof(AutoPen))]
    public class AutoPenEditor : Editor
    {
        AutoPen _target;

        private void OnEnable()
        {
            _target = target as AutoPen;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button(EditorApplication.isPlaying ? "描画実行" : "描画実行 (プレイモードのみ)"))
            {
                if (EditorApplication.isPlaying)
                {
                    MethodInfo method = typeof(AutoPen).GetMethod("StartDraw()", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var paramters = new object[] { };
                    method.Invoke(_target, paramters);
                }
            }
            GUI.enabled = true;

            DrawDefaultInspector();
        }
    }
}
#endif

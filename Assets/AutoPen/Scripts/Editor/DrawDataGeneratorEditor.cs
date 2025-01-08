#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Swishf.AutoPen
{
    [CustomEditor(typeof(DrawDataGenerator))]
    public class DrawDataGeneratorEditor : Editor
    {
        DrawDataGenerator _target;

        private void OnEnable()
        {
            _target = target as DrawDataGenerator;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("データ生成"))
            {
                MethodInfo method = typeof(DrawDataGenerator).GetMethod("DrawDataToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var paramters = new object[] { };
                method.Invoke(_target, paramters);
            }

            if (GUILayout.Button("トランスフォームをリセット (単体データ用)"))
            {
                ResetDrawDataTransforms(_target.transform, isCharData: false);
                EditorUtility.SetDirty(_target.gameObject);
            }

            if (GUILayout.Button("トランスフォームを最小領域でクロップ"))
            {
                CropDrawDataTransforms(_target.transform);
                EditorUtility.SetDirty(_target.gameObject);
            }

            // if (GUILayout.Button("メタデータ変換"))
            // {
            //     foreach (Transform lineGroupObj in _target.transform)
            //     {
            //         var s = lineGroupObj.name.Split(",");
            //         lineGroupObj.name = $"{s[1]},{s[0]}";
            //     }
            //     EditorUtility.SetDirty(_target.gameObject);
            // }

            // if (GUILayout.Button("メタデータ移行"))
            // {
            //     foreach (Transform lineGroupObj in _target.transform)
            //     {
            //         foreach (Transform lineObj in lineGroupObj)
            //         {
            //             AutoPenUtils.StringToLineMetaData(lineObj.name, out string name, out int colorId, out LineType lineType);
            //             lineObj.name = $"{colorId},{(int)lineType},{name}";
            //         }
            //     }
            //     EditorUtility.SetDirty(_target.gameObject);
            // }

            DrawDefaultInspector();
        }

        public static void ResetDrawDataTransforms(Transform dataObj, bool isCharData, bool changeName = true)
        {
            var changeLinesName = /*changeName*/ false;
            var changeLineName = changeName;
            var changePointName = changeName;
            if (changeName)
            {
                if (!isCharData)
                {
                    changeLinesName = false;
                }
            }

            Dictionary<Transform, Vector3> pointPositions = new();
            foreach (Transform lineGroup in dataObj)
            {
                if (changeLinesName)
                {
                    lineGroup.name = $"LineGroup_{lineGroup.GetSiblingIndex()}";
                }
                foreach (Transform lineObj in lineGroup)
                {
                    if (changeLineName)
                    {
                        AutoPenUtils.StringToLineMetaData(lineObj.name, out string name, out int colorId, out LineType lineType);
                        lineObj.name = AutoPenUtils.LineMetaDataToString($"line_{lineObj.GetSiblingIndex()}", colorId, lineType);
                    }
                    foreach (Transform pointObj in lineObj)
                    {
                        if (changePointName)
                        {
                            pointObj.name = $"point_{pointObj.GetSiblingIndex()}";
                        }
                        pointPositions[pointObj] = pointObj.position;
                    }
                }
            }

            var linesPositionOffset = Vector3.zero;
            if (isCharData)
            {
                linesPositionOffset = new Vector3(0.5f, 0, 0);
            }

            foreach (Transform lineGroupObj in dataObj)
            {
                ResetTransform(lineGroupObj);
                lineGroupObj.localPosition += linesPositionOffset;
                foreach (Transform lineObj in lineGroupObj)
                {
                    ResetTransform(lineObj);
                    foreach (Transform pointObj in lineObj)
                    {
                        ResetTransform(pointObj);
                    }
                }
            }

            foreach (Transform pointObj in pointPositions.Keys)
            {
                pointObj.position = pointPositions[pointObj];
            }
        }

        public static void CropDrawDataTransforms(Transform dataObj)
        {
            foreach (Transform lineGroupObj in dataObj)
            {
                foreach (Transform lineObj in lineGroupObj)
                {
                    CropTransformLeftBottom(lineObj);
                }
            }

            foreach (Transform lineGroupObj in dataObj)
            {
                CropTransformLeftBottom(lineGroupObj);
            }
        }

        static void ResetTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        static void CropTransformLeftBottom(Transform root)
        {
            var minPos = Vector3.one * float.MaxValue;

            Dictionary<Transform, Vector3> childPositions = new();
            foreach (Transform child in root)
            {
                childPositions[child] = child.position;

                minPos.x = Mathf.Min(minPos.x, child.position.x);
                minPos.y = Mathf.Min(minPos.y, child.position.y);
                minPos.z = Mathf.Min(minPos.z, child.position.z);
            }

            root.transform.position = minPos;

            foreach (Transform child in childPositions.Keys)
            {
                child.position = childPositions[child];
            }
        }
    }
}
#endif

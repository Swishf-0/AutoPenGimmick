#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;

using QvPen.UdonScript;

namespace Swishf.AutoPen
{
    public class DrawDataVisualizer : MonoBehaviour
    {
        [SerializeField] bool _showLine = true;
        [SerializeField] StraightLineData _straightLineData;
        [SerializeField] SplineData _splineData;
        [SerializeField] Transform _drawDataObj;
        [SerializeField] AutoPenManager _autoPenManager;

        void Initialize()
        {
            if (_splineData == null)
            {
                _splineData = GetComponent<SplineData>();
            }

            if (_straightLineData == null)
            {
                _straightLineData = GetComponent<StraightLineData>();
            }

            if (_drawDataObj == null)
            {
                var drawDataGenerator = GetComponent<DrawDataGenerator>();

                MethodInfo method = typeof(DrawDataGenerator).GetMethod("Initialize", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var paramters = new object[] { };
                method.Invoke(drawDataGenerator, paramters);

                FieldInfo fieldInfo = typeof(DrawDataGenerator).GetField("_drawDataObj", BindingFlags.NonPublic | BindingFlags.Instance);
                _drawDataObj = fieldInfo.GetValue(drawDataGenerator) as Transform;
            }

            if (_autoPenManager == null)
            {
                var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in rootObjs)
                {
                    var setting = root.GetComponentInChildren<AutoPenManager>();
                    if (setting != null)
                    {
                        _autoPenManager = setting;
                        break;
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!_showLine)
            {
                return;
            }

            Initialize();
            DrawLineData(_drawDataObj, _straightLineData, _splineData, _autoPenManager.PenManagers);
        }

        static void DrawLineData(Transform drawDataRoot, StraightLineData straightLineData, SplineData splineData, QvPen_PenManager[] penManagers)
        {
            foreach (Transform lineGroupObj in drawDataRoot)
            {
                foreach (Transform lineObj in lineGroupObj)
                {
                    AutoPenUtils.StringToLineMetaData(lineObj.name, out string name, out int colorId, out LineType lineType);

                    var color = Color.black;
                    if (penManagers != null && penManagers.Length > 0 && penManagers[colorId % penManagers.Length] != null)
                    {
                        color = penManagers[colorId % penManagers.Length].colorGradient.Evaluate(0.5f);
                    }
                    Gizmos.color = color;

                    var points = MiscUtils.GetChildrenPositions(lineObj, Vector3.zero);
                    switch (lineType)
                    {
                        case LineType.Straight:
                            {
                                straightLineData.Init(ref points);
                                for (float t = 0; t < straightLineData.GetPointCount(); t += 0.01f)
                                {
                                    Gizmos.DrawLine(straightLineData.CalcPosition(t), straightLineData.CalcPosition(t + 0.01f));
                                }
                                break;
                            }
                        case LineType.Spline:
                            {
                                splineData.Init(ref points);
                                for (float t = 0; t < splineData.GetPointCount(); t += 0.01f)
                                {
                                    Gizmos.DrawLine(splineData.CalcPosition(t), splineData.CalcPosition(t + 0.01f));
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
#endif

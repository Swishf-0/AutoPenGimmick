using UdonSharp;
using UnityEngine;

namespace Swishf.AutoPen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StraightLineData : UdonSharpBehaviour
    {
        const int TX_STEP = 20;

        Vector3[] _points;
        float[] _txTable;

        public int GetPointCount()
        {
            return _points.Length;
        }

        public void Init(ref Vector3[] points)
        {
            _points = points;
            _txTable = null;
        }

        public Vector3 CalcPosition(float t)
        {
            return StraightLineUtils.CalcPosition(ref _points, t);
        }
    }

    public static class StraightLineUtils
    {
        public static void InitTXTable(ref Vector3[] points, ref float[] txTable, int txStep)
        {
        }

        public static Vector3 CalcPosition(ref Vector3[] points, float t)
        {
            if (points.Length == 0)
            {
                return Vector3.zero;
            }
            if (points.Length == 1)
            {
                return points[0];
            }

            int i = Mathf.Clamp((int)t, 0, points.Length - 1);
            if (i == points.Length - 1)
            {
                return points[points.Length - 1];
            }

            float dt = t - i;
            return points[i] + (points[i + 1] - points[i]) * dt;
        }
    }
}

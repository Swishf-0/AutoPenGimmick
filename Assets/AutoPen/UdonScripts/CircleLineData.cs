using UdonSharp;
using UnityEngine;

namespace Swishf.AutoPen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CircleLineData : UdonSharpBehaviour
    {
        const int TX_STEP = 20;

        Vector3[] _points;
        float[] _txTable;

        public int GetPointCount()
        {
            return 1;
        }

        public void Init(ref Vector3[] points)
        {
            _points = points;
            _txTable = null;
        }

        public Vector3 CalcPosition(float t)
        {
            return CircleLineUtils.CalcPosition(ref _points, t);
        }
    }

    public static class CircleLineUtils
    {
        public static void InitTXTable(ref Vector3[] points, ref float[] txTable, int txStep)
        {
        }

        public static Vector3 CalcPosition(ref Vector3[] points, float t)
        {
            return Vector3.zero;
        }
    }
}

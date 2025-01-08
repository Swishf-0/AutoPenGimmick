using UnityEngine;

namespace Swishf.AutoPen
{
    public class DrawDataGenerator : MonoBehaviour
    {
        const int DIGIT = 1000;

        [SerializeField, TextArea(1, 5)] string _drawDataString;
        [SerializeField] Transform _drawDataObj;

        void Initialize()
        {
            if (_drawDataObj == null)
            {
                _drawDataObj = transform;
            }
        }

        void DrawDataToString()
        {
            Initialize();
            AutoPenUtils.DrawDataFromObject(_drawDataObj, out var lines, out var lineColors, out var lineTypes);
            _drawDataString = AutoPenUtils.DrawDataToString(ref lines, ref lineColors, ref lineTypes, DIGIT);
        }
    }
}

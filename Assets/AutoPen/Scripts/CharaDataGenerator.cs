#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;

namespace Swishf.AutoPen
{
    public class CharaDataGenerator : MonoBehaviour
    {
        [SerializeField, TextArea(1, 5)] string _charaDataString;
        [SerializeField] Transform _charaDataObj;

        void DrawDataToString()
        {
            ConvertAllDrawDataToString(out var drawDataStringList, out var idList, out var actionList);
            if (drawDataStringList == null)
            {
                return;
            }

            _charaDataString = $"{DataStringListToString(drawDataStringList)}&{System.String.Join(",", idList)}&{System.String.Join(",", actionList)}";
        }

        string DataStringListToString(string[] dataStringList)
        {
            var str = "";
            foreach (var dataString in dataStringList)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += "#";
                }
                str += dataString;
            }
            return str;
        }

        void ConvertAllDrawDataToString(out string[] drawDataStringList, out int[] idList, out int[] actionList)
        {
            drawDataStringList = new string[_charaDataObj.childCount];
            idList = new int[_charaDataObj.childCount];
            actionList = new int[_charaDataObj.childCount];

            for (int i = 0; i < _charaDataObj.childCount; i++)
            {
                idList[i] = -1;

                var drawDataObj = _charaDataObj.GetChild(i);
                var dataGeneartor = drawDataObj.GetComponent<DrawDataGenerator>();
                MethodInfo method = typeof(DrawDataGenerator).GetMethod("DrawDataToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var paramters = new object[] { };
                method.Invoke(dataGeneartor, paramters);

                FieldInfo fieldInfo = typeof(DrawDataGenerator).GetField("_drawDataString", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    OnError(drawDataObj);
                    return;
                }

                var drawDataString = fieldInfo.GetValue(dataGeneartor) as string;
                if (string.IsNullOrEmpty(drawDataString))
                {
                    OnError(drawDataObj);
                    return;
                }

                drawDataStringList[i] = drawDataString;
                if (!StringToCharaMetaData(dataGeneartor.transform.name, out var dataId, out var actionId))
                {
                    OnWarning(drawDataObj);
                }
                idList[i] = dataId;
                actionList[i] = actionId;
            }
        }

        bool StringToCharaMetaData(string str, out int dataId, out int actionId)
        {
            dataId = -1;
            actionId = 0;

            var splitString = str.Split(",");
            var validData = splitString.Length == 3;
            if (validData)
            {
                if (!int.TryParse(splitString[0], out dataId))
                {
                    validData = false;
                }
                int.TryParse(splitString[1], out actionId);
            }

            return validData;
        }

        void OnWarning(Transform t)
        {
            Debug.LogWarning($"正しくないデータが見つかりました. 該当データは無視されます.: {t.name}");
        }

        void OnError(Transform t)
        {
            Debug.LogError($"正しくないデータが見つかりました. 処理が中断しました.: {t.name}");
        }
    }
}
#endif

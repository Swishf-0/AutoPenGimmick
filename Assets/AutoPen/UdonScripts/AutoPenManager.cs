using UdonSharp;
using UnityEngine;

using QvPen.UdonScript;
using System;
using VRC.SDKBase;

namespace Swishf.AutoPen
{
    public enum LineType
    {
        Straight = 0,
        Spline = 1,
        Circle = 2,
    }

    public class AutoPenManager : UdonSharpBehaviour
    {
        const int CHAR_CODE_SPACE = 10000000;
        const int CHAR_CODE_ENTER = 10000001;

        [SerializeField] QvPen_PenManager[] _penManagers;
        [SerializeField] QvPen_Pen[] _pens;
        [SerializeField] VRC_Pickup[] _penPickups;
        [SerializeField, TextArea(1, 5)] string _charaTableString;
        public QvPen_PenManager[] PenManagers => _penManagers;
        public QvPen_Pen[] Pens => _pens;
        public VRC_Pickup[] PenPickups => _penPickups;

        AutoPen[] _autoPens;
        [SerializeField] string[] _charKeys;
        [SerializeField] int[] _charCodes;

        void Start()
        {
            _autoPens = transform.GetComponentsInChildren<AutoPen>();

            foreach (var autoPen in _autoPens)
            {
                autoPen.Initialize(this);
            }

            AutoPenUtils.StringToCharaTable(_charaTableString, out _charKeys, out _charCodes);
        }

        void Update()
        {
            foreach (var autoPen in _autoPens)
            {
                autoPen.Update_();
            }
        }

        public int GetCharCode(string key)
        {
            if (key == "\n")
            {
                return CHAR_CODE_ENTER;
            }
            if (key == " " || key == "　")
            {
                return CHAR_CODE_SPACE;
            }
            var idx = Array.IndexOf(_charKeys, key);
            if (idx < 0)
            {
                return -1;
            }
            return _charCodes[idx];
        }

        public int[] GetCharCodes(string text)
        {
            var chars = text.ToCharArray();
            int[] charCodes = new int[chars.Length];
            for (int i = 0; i < chars.Length; i++)
            {
                charCodes[i] = GetCharCode(chars[i].ToString());
            }
            return charCodes;
        }
    }

    public static class AutoPenUtils
    {
        public static void DrawDataFromObject(Transform drawDataObj, out Vector3[][] lines, out int[] lineColors, out LineType[] lineTypes)
        {
            Vector3 rootPositionOffset = drawDataObj.position;

            int lineCount = 0;
            foreach (Transform lineGroupObj in drawDataObj)
            {
                lineCount += lineGroupObj.childCount;
            }

            lines = new Vector3[lineCount][];
            lineColors = new int[lineCount];
            lineTypes = new LineType[lineCount];

            int c = 0;
            for (int i = 0; i < drawDataObj.childCount; i++)
            {
                var lineGroupObj = drawDataObj.GetChild(i);
                for (int j = 0; j < lineGroupObj.childCount; j++)
                {
                    var lineObj = lineGroupObj.GetChild(j);
                    lines[c] = MiscUtils.GetChildrenPositions(lineObj, rootPositionOffset);

                    StringToLineMetaData(lineObj.name, out string name, out int colorId, out LineType lineType);
                    lineColors[c] = colorId;
                    lineTypes[c] = lineType;

                    c++;
                }
            }
        }

        public static void StringToCharaData(string charaDataString, int digit, out Vector3[][][] charaDataLines, out int[] charaDataIds, out int[] charaActions, out LineType[][] charaDataLineTypes)
        {
            var dsList = charaDataString.Split("&");
            var drawDataString = dsList[0].Split("#");
            charaDataLines = new Vector3[drawDataString.Length][][];
            charaDataLineTypes = new LineType[drawDataString.Length][];
            for (int i = 0; i < drawDataString.Length; i++)
            {
                StringToDrawData(drawDataString[i], digit, out var lines, out var lineColors, out var lineTypes);
                charaDataLines[i] = lines;
                charaDataLineTypes[i] = lineTypes;
            }

            MiscUtils.StringToIntArray(dsList[1], out charaDataIds);
            MiscUtils.StringToIntArray(dsList[2], out charaActions);
        }

        public static void StringToDrawData(string drawDataString, int digit, out Vector3[][] lines, out int[] lineColors, out LineType[] lineTypes)
        {
            var dataStringList = drawDataString.Split("@");
            if (dataStringList.Length <= 0)
            {
                lines = new Vector3[0][];
                lineColors = new int[0];
                lineTypes = new LineType[0];
                return;
            }
            StringToLines(dataStringList[0], digit, out lines);

            if (dataStringList.Length <= 1)
            {
                lineColors = new int[lines.Length];
                lineTypes = new LineType[lines.Length];
                return;
            }
            MiscUtils.StringToIntArray(dataStringList[1], out lineColors);

            if (dataStringList.Length <= 2)
            {
                lineTypes = new LineType[lines.Length];
                return;
            }
            MiscUtils.StringToIntArray(dataStringList[2], out var lineTypesInt);
            lineTypes = IntArrayToLineTypes(ref lineTypesInt);
        }

        public static void StringToLines(string linesString, int digit, out Vector3[][] lines)
        {
            var lineStringList = linesString.Split("\n");
            lines = new Vector3[lineStringList.Length][];
            for (int i = 0; i < lineStringList.Length; i++)
            {
                MiscUtils.StringToVector3Array(lineStringList[i], out var points, digit);
                lines[i] = points;
            }
        }

        public static void StringToLineMetaData(string str, out string name, out int colorId, out LineType lineType)
        {
            name = str;
            colorId = 0;
            lineType = LineType.Straight;

            if (String.IsNullOrEmpty(str))
            {
                return;
            }

            var metas = str.Split(',');

            if (metas.Length <= 1)
            {
                return;
            }

            if (metas.Length > 1)
            {
                int.TryParse(metas[0], out colorId);
            }

            if (metas.Length > 2)
            {
                if (int.TryParse(metas[1], out var lineTypeInt))
                {
                    lineType = (LineType)lineTypeInt;
                }
            }

            name = metas[metas.Length - 1];
        }

        public static string LineMetaDataToString(string name, int colorId, LineType lineType)
        {
            return $"{colorId},{(int)lineType},{name}";
        }

        public static string DrawDataToString(ref Vector3[][] lines, ref int[] lineColors, ref LineType[] lineTypes, int digit)
        {
            var lineString = LinesToString(ref lines, digit);
            var lineColorsString = System.String.Join(",", lineColors);
            var lineTypesString = System.String.Join(",", LineTypesToIntArray(ref lineTypes));
            return $"{lineString}@{lineColorsString}@{lineTypesString}";
        }

        static string LinesToString(ref Vector3[][] lines, int digit)
        {
            var text = "";
            foreach (Vector3[] line in lines)
            {
                string ps = "";
                foreach (Vector3 point in line)
                {
                    if (!string.IsNullOrEmpty(ps))
                    {
                        ps += ",";
                    }
                    ps += $"{(int)(point.x * digit)},{(int)(point.y * digit)},{(int)(point.z * digit)}";
                }

                if (!string.IsNullOrEmpty(text))
                {
                    text += "\n";
                }
                text += ps;
            }
            return text;
        }

        public static LineType[] IntArrayToLineTypes(ref int[] lineTypesInt)
        {
            var lineTypes = new LineType[lineTypesInt.Length];
            for (int i = 0; i < lineTypesInt.Length; i++)
            {
                lineTypes[i] = (LineType)lineTypesInt[i];
            }
            return lineTypes;
        }

        public static int[] LineTypesToIntArray(ref LineType[] lineTypes)
        {
            var lineTypesInt = new int[lineTypes.Length];
            for (int i = 0; i < lineTypes.Length; i++)
            {
                lineTypesInt[i] = (int)lineTypes[i];
            }
            return lineTypesInt;
        }

        public static void StringToCharaTable(string charaTableString, out string[] charKeys, out int[] charCodes)
        {
            if (string.IsNullOrEmpty(charaTableString))
            {
                charKeys = new string[0];
                charCodes = new int[0];
                return;
            }

            var keyIdStringList = charaTableString.Split("\n");
            charKeys = new string[keyIdStringList.Length];
            charCodes = new int[keyIdStringList.Length];
            for (int i = 0; i < keyIdStringList.Length; i++)
            {
                StringToCharKeyCode(keyIdStringList[i], out string charKey, out int charCode);
                charKeys[i] = charKey;
                charCodes[i] = charCode;
            }
        }

        static bool StringToCharKeyCode(string str, out string charKey, out int charCode)
        {
            charKey = "";
            charCode = -1;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var keIdStrPair = str.Split(",");
            if (keIdStrPair.Length < 2)
            {
                return false;
            }

            if (!int.TryParse(keIdStrPair[keIdStrPair.Length - 1], out var charCodeParsed))
            {
                return false;
            }

            var keyParsed = string.Empty;
            for (int i = 0; i < keIdStrPair.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(keyParsed))
                {
                    keyParsed += ",";
                }
                keyParsed += keIdStrPair[i];
            }

            if (string.IsNullOrEmpty(keyParsed))
            {
                return false;
            }

            charKey = keyParsed;
            charCode = charCodeParsed;
            return true;
        }
    }

    public static class MiscUtils
    {
        public static Vector3[] GetChildrenPositions(Transform root, Vector3 offset)
        {
            var positions = new Vector3[root.childCount];
            for (int i = 0; i < root.childCount; i++)
            {
                positions[i] = root.GetChild(i).position - offset;
            }
            return positions;
        }

        public static void StringToIntArray(string valueString, out int[] value, string separator = ",")
        {
            var stringArray = valueString.Split(separator);
            value = new int[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (int.TryParse(stringArray[i], out var v))
                {
                    value[i] = v;
                }
            }
        }

        public static bool StringToVector3Array(string valueString, out Vector3[] values, int digit, string separator = ",")
        {
            float digitInv = 1f / digit;

            var stringArray = valueString.Split(separator);
            if (stringArray.Length % 3 != 0)
            {
                values = new Vector3[0];
                return false;
            }

            values = new Vector3[stringArray.Length / 3];
            int j = 0;
            for (int i = 0; i < stringArray.Length; i += 3)
            {
                if (int.TryParse(stringArray[i], out var __x) && int.TryParse(stringArray[i + 1], out var __y) && int.TryParse(stringArray[i + 2], out var __z))
                {
                    values[j] = new Vector3(__x * digitInv, __y * digitInv, __z * digitInv);
                }
                j++;
            }

            return true;
        }

        public static Color32 GetColorByHsv(int h)
        {
            float s = 1, v = 1;
            Color32 c = new Color32();
            int i = (int)(h / 60f);
            float f = h / 60f - i;
            byte p1 = (byte)(v * (1 - s) * 255);
            byte p2 = (byte)(v * (1 - s * f) * 255);
            byte p3 = (byte)(v * (1 - s * (1 - f)) * 255);
            byte vi = (byte)(v * 255);
            byte r = 0, g = 0, b = 0;
            switch (i)
            {
                case 0: r = vi; g = p3; b = p1; break;
                case 1: r = p2; g = vi; b = p1; break;
                case 2: r = p1; g = vi; b = p3; break;
                case 3: r = p1; g = p2; b = vi; break;
                case 4: r = p3; g = p1; b = vi; break;
                case 5: r = vi; g = p1; b = p2; break;
                default: break;
            }
            c.a = 255;
            c.r = r;
            c.g = g;
            c.b = b;
            return c;
        }
    }
}

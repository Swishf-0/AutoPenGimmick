using System;
using UdonSharp;
using UnityEngine;

namespace Swishf.AutoPen
{
    enum WriterState
    {
        Wait,
        Writing,
    }

    enum LetterAction
    {
        None = 0,
        BackOneLetter = 1,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoPenWriter : UdonSharpBehaviour
    {
        const int DIGIT = 1000;
        const int QUEUE_COUNT = 50;
        const int CHAR_CODE_SPACE = 10000000;
        const int CHAR_CODE_ENTER = 10000001;
        const int CHAR_CODE_INVALID = -1;

        public bool Initialized { get; private set; }
        public Transform BaseAnchor => _baseAnchor;

        public float LetterSpacing { get => _letterSpacing; set => _letterSpacing = value; }
        public float LineSpacing { get => _lineSpacing; set => _lineSpacing = value; }
        public float LetterScale { get => _letterScale; set => _letterScale = value; }
        public int LetterColor { get => _letterColor; set => _letterColor = value; }
        public bool NeedResetBaseAnchor { get; set; }

        [SerializeField, TextArea(1, 5)] string _charaDataString;
        Vector3[][][] _charaDataLines;
        int[] _charaDataIds;
        int[] _charaActions;
        LineType[][] _charaDataLineTypes;

        [SerializeField] AutoPen _autoPen;
        WriterState _writerState;
        int _currentWriteCharIdx, _currentBookCharIdx;
        int[] _inputQueueChar;
        int[] _inputQueueLineColors;

        [SerializeField] float _letterSpacing = 0.8f;
        [SerializeField] float _lineSpacing = 1.2f;
        [SerializeField] float _letterScale = 1f;
        [SerializeField] int _letterColor;
        [SerializeField] Transform _baseAnchor;

        int _currentLineIdx = 0;
        Vector3 _preBasePosition;

        void Update() { Update_(); }

        void Initialize()
        {
            AutoPenUtils.StringToCharaData(_charaDataString, DIGIT, out _charaDataLines, out _charaDataIds, out _charaActions, out _charaDataLineTypes);

            InitInput();

            Initialized = true;
        }

        void Update_()
        {
            if (!Initialized)
            {
                if (_autoPen.Initialized)
                {
                    Initialize();
                }
                return;
            }

            UpdateWriting();
        }

        void InitInput()
        {
            _currentWriteCharIdx = 0;
            _currentBookCharIdx = 0;
            _writerState = WriterState.Wait;
            _inputQueueChar = new int[QUEUE_COUNT];
            _inputQueueLineColors = new int[QUEUE_COUNT];
            for (int i = 0; i < _inputQueueChar.Length; i++)
            {
                _inputQueueChar[i] = CHAR_CODE_INVALID;
                _inputQueueLineColors[i] = 0;
            }

            _baseAnchor.parent = transform.parent;
            _baseAnchor.SetPositionAndRotation(transform.position, transform.rotation);
            _preBasePosition = _baseAnchor.position;

            _autoPen.ReturnPenOnEnd = false;
        }

        void UpdateWriting()
        {
            switch (_writerState)
            {
                case WriterState.Wait:
                    {
                        bool needResetBaseAnchor = NeedResetBaseAnchor;
                        NeedResetBaseAnchor = false;
                        needResetBaseAnchor |= _preBasePosition.x != _baseAnchor.position.x || _preBasePosition.y != _baseAnchor.position.y || _preBasePosition.z != _baseAnchor.position.z;
                        if (needResetBaseAnchor)
                        {
                            _preBasePosition = _baseAnchor.position;
                            _autoPen.BaseAnchor.SetPositionAndRotation(_baseAnchor.position, _baseAnchor.rotation);
                            _currentLineIdx = 0;
                        }

                        var charaCode = _inputQueueChar[_currentWriteCharIdx];
                        ExecAction(charaCode, out var isDrawData);
                        if (isDrawData && GetDrawData(charaCode, out var lines, out var lineColors, out var lineTypes))
                        {
                            _autoPen.StartDraw(lines, lineColors, lineTypes, _letterScale);
                            _writerState = WriterState.Writing;
                            return;
                        }

                        _inputQueueChar[_currentWriteCharIdx] = CHAR_CODE_INVALID;
                        _currentWriteCharIdx = (_currentWriteCharIdx + 1) % _inputQueueChar.Length;
                        return;
                    }
                case WriterState.Writing:
                    {
                        if (_autoPen.IsIdle)
                        {
                            _inputQueueChar[_currentWriteCharIdx] = CHAR_CODE_INVALID;
                            _currentWriteCharIdx = (_currentWriteCharIdx + 1) % _inputQueueChar.Length;
                            _writerState = WriterState.Wait;
                            MoveCursorNext();
                        }

                        return;
                    }
            }
        }

        void MoveCursorNext()
        {
            _autoPen.BaseAnchor.position += _letterSpacing * _baseAnchor.right;
        }

        void MoveCursorBack()
        {
            _autoPen.BaseAnchor.position -= _letterSpacing * _baseAnchor.right;
        }

        void MoveCursorReturn()
        {
            _currentLineIdx++;
            _autoPen.BaseAnchor.SetPositionAndRotation(
                _baseAnchor.position - _currentLineIdx * _lineSpacing * _baseAnchor.up,
                _baseAnchor.rotation);
        }

        void ExecAction(int charaCode, out bool isDrawData)
        {
            if (charaCode == CHAR_CODE_INVALID)
            {
                isDrawData = false;
                return;
            }

            if (charaCode == CHAR_CODE_SPACE)
            {
                isDrawData = false;
                MoveCursorNext();
                return;
            }

            if (charaCode == CHAR_CODE_ENTER)
            {
                isDrawData = false;
                MoveCursorReturn();
                return;
            }

            var index = Array.IndexOf(_charaDataIds, charaCode);
            if (index < 0)
            {
                isDrawData = false;
                return;
            }

            var action = (LetterAction)_charaActions[index];
            switch (action)
            {
                case LetterAction.BackOneLetter:
                    {
                        MoveCursorBack();
                        break;
                    }
            }

            isDrawData = true;
            return;
        }

        bool GetDrawData(int charaCode, out Vector3[][] lines, out int[] lineColors, out LineType[] lineTypes)
        {
            var index = Array.IndexOf(_charaDataIds, charaCode);
            if (index < 0)
            {
                lines = new Vector3[0][];
                lineColors = new int[0];
                lineTypes = new LineType[0];
                return false;
            }

            lines = _charaDataLines[index];
            lineTypes = _charaDataLineTypes[index];

            int meta = _inputQueueLineColors[_currentWriteCharIdx];
            lineColors = new int[lines.Length];
            for (int i = 0; i < lineColors.Length; i++)
            {
                lineColors[i] = meta;
            }

            return true;
        }

        public void OnClickKey(int keyCode)
        {
            if (_inputQueueChar[_currentBookCharIdx] != CHAR_CODE_INVALID)
            {
                return;
            }

            _inputQueueChar[_currentBookCharIdx] = keyCode;
            _inputQueueLineColors[_currentBookCharIdx] = _letterColor;
            _currentBookCharIdx = (_currentBookCharIdx + 1) % _inputQueueChar.Length;
        }
    }
}

using UdonSharp;
using UnityEngine;

using VRC.SDKBase;

using QvPen.UdonScript;

namespace Swishf.AutoPen
{
    enum DrawControlState
    {
        Wait,
        SetData,
        WaitStartDrawLine,
        WaitNextLineDraw,
        DrawLine,
        End,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoPen : UdonSharpBehaviour
    {
        const int DIGIT = 1000;

        [SerializeField, TextArea(1, 5)] string _drawDataString;
        [SerializeField] bool _usedrawDataObj;
        [SerializeField] Transform _drawDataObj;

        [SerializeField] float _waitTime = 0.12f;
        [SerializeField] float _timeStep = 0.12f;

        public bool Initialized { get; private set; }
        public bool IsIdle => _drawState == DrawControlState.Wait;
        public bool ReturnPenOnEnd { get; set; } = true;
        public Transform BaseAnchor => _baseAnchor;

        Vector3[][] _lines;
        int[] _lineColors;
        LineType[] _lineTypes;

        AutoPenManager _autoPenManager;
        QvPen_Pen _currentPen;
        Vector3 _inkPositionOffset;
        Vector3 _initialPenPosition;
        Quaternion _initialPenRotation;
        Transform _baseAnchor;
        int _currentLineIdx;
        DrawControlState _drawState = DrawControlState.Wait;
        float _timer;
        AutoDraw _autoDraw;
        float _scale = 1;

        bool _waitingPenReturn;
        float _penReturnTime;

        public void Initialize(AutoPenManager autoPenManager)
        {
            _autoPenManager = autoPenManager;

            _autoDraw = GetComponent<AutoDraw>();
            _autoDraw.Initialize(_timeStep, _waitTime);

            _baseAnchor = transform.Find("BaseAnchor");
            _baseAnchor.parent = transform.parent;
            _baseAnchor.SetPositionAndRotation(transform.position, transform.rotation);

            if (_usedrawDataObj)
            {
                InitDrawDataFromObject();
            }
            else
            {
                InitDrawDataFromString();
            }

            Initialized = true;
        }

        public void Update_()
        {
            if (!Initialized)
            {
                return;
            }

            UpdateAutoDraw();
            WaitReturnPen();
        }

        void InitDrawDataFromObject()
        {
            AutoPenUtils.DrawDataFromObject(_drawDataObj, out _lines, out _lineColors, out _lineTypes);
        }

        void InitDrawDataFromString()
        {
            AutoPenUtils.StringToDrawData(_drawDataString, DIGIT, out _lines, out _lineColors, out _lineTypes);
        }

        QvPen_Pen GetAvailablePen(int i, QvPen_Pen currentPen, out bool isSamePen)
        {
            for (int __offset_idx = 0; __offset_idx < _autoPenManager.Pens.Length; __offset_idx++)
            {
                var idx = (__offset_idx + i) % _autoPenManager.Pens.Length;
                var pen = _autoPenManager.Pens[idx];
                if (pen == null)
                {
                    continue;
                }
                if (_autoPenManager.PenPickups[idx] == null || _autoPenManager.PenPickups[idx].IsHeld)
                {
                    continue;
                }

                isSamePen = pen == currentPen;
                if (isSamePen)
                {
                    if (isSamePen && Networking.IsOwner(pen.gameObject))
                    {
                        return pen;
                    }
                }
                else if (!pen.isHeld)
                {
                    return pen;
                }
            }

            isSamePen = false;
            return null;
        }

        public void StartDraw()
        {
            InitDraw();
        }

        public void StartDraw(Vector3[][] lines, int[] lineColors, LineType[] lineTypes, float scale)
        {
            _lines = lines;
            _lineColors = lineColors;
            _lineTypes = lineTypes;
            _scale = scale;
            StartDraw();
        }

        void UsePen(QvPen_Pen pen, out Vector3 inkPositionOffset)
        {
            if (pen == null)
            {
                inkPositionOffset = Vector3.zero;
                return;
            }

            if (!Networking.IsOwner(pen.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, pen.gameObject);
            }

            _initialPenPosition = GetPenPosition();
            _initialPenRotation = GetPenRotation();

            SetPenRotation(_baseAnchor.rotation);

            var inkPosition = pen.transform.Find("InkPosition");
            if (inkPosition == null)
            {
                inkPositionOffset = Vector3.zero;
            }
            else
            {
                var p = pen.transform.position - inkPosition.position;
                inkPositionOffset = new Vector3(Vector3.Dot(p, pen.transform.right), Vector3.Dot(p, pen.transform.up), Vector3.Dot(p, pen.transform.forward));
            }

            pen.OnPickup();
        }

        void ReturnPen(QvPen_Pen pen)
        {
            if (pen == null || !pen.IsUser)
            {
                return;
            }

            pen.OnDrop();
            SetPenPosition(_initialPenPosition);
            SetPenRotation(_initialPenRotation);
        }

        void InitDraw()
        {
            if (!IsIdle)
            {
                _drawState = DrawControlState.Wait;
                ReturnPen(_currentPen);
                _currentPen = null;
            }

            if (_lines.Length <= 0)
            {
                _drawState = DrawControlState.End;
                return;
            }

            _currentLineIdx = 0;

            _drawState = DrawControlState.SetData;
        }

        void UpdateAutoDraw()
        {
            switch (_drawState)
            {
                case DrawControlState.SetData:
                    {
                        var pen = GetAvailablePen(_lineColors[_currentLineIdx], _currentPen, out var isSamePen);
                        if (pen == null)
                        {
                            _drawState = DrawControlState.End;
                            return;
                        }

                        if (!isSamePen)
                        {
                            ReturnPen(_currentPen);
                            _currentPen = pen;
                            UsePen(_currentPen, out _inkPositionOffset);
                        }
                        else
                        {
                            if (!_currentPen.IsUser/* || !_currentPen.isHeld*/)
                            {
                                _drawState = DrawControlState.End;
                                return;
                            }
                            SetPenRotation(_baseAnchor.rotation);
                        }

                        if (_lines[_currentLineIdx] == null || _lines[_currentLineIdx].Length <= 0)
                        {
                            _drawState = DrawControlState.End;
                            return;
                        }


                        _autoDraw.StartDraw(_lines[_currentLineIdx], _currentPen.transform, _baseAnchor, _inkPositionOffset, _scale, _lineTypes[_currentLineIdx]);
                        _timer = Time.time + _waitTime;
                        _drawState = DrawControlState.WaitStartDrawLine;
                        return;
                    }
                case DrawControlState.WaitStartDrawLine:
                    {
                        if (Time.time < _timer)
                        {
                            return;
                        }

                        _currentPen.OnPickupUseDown();
                        _drawState = DrawControlState.DrawLine;
                        return;
                    }
                case DrawControlState.DrawLine:
                    {
                        _autoDraw.Update_();
                        if (_autoDraw.IsIdle)
                        {
                            _timer = Time.time + _waitTime;
                            _drawState = DrawControlState.WaitNextLineDraw;
                        }

                        return;
                    }
                case DrawControlState.WaitNextLineDraw:
                    {
                        if (Time.time < _timer)
                        {
                            return;
                        }

                        _currentPen.OnPickupUseUp();

                        _currentLineIdx++;
                        if (_currentLineIdx < _lines.Length)
                        {
                            _drawState = DrawControlState.SetData;
                            return;
                        }

                        _drawState = DrawControlState.End;

                        return;
                    }
                case DrawControlState.End:
                    {
                        _drawState = DrawControlState.Wait;
                        if (ReturnPenOnEnd)
                        {
                            ReturnPen(_currentPen);
                            _currentPen = null;
                        }
                        else
                        {
                            StartWaitReturnPen();
                        }
                        return;
                    }
            }
        }

        void StartWaitReturnPen()
        {
            _waitingPenReturn = true;
            _penReturnTime = Time.time + 1;
        }

        void WaitReturnPen()
        {
            if (!_waitingPenReturn)
            {
                return;
            }

            if (_drawState != DrawControlState.Wait)
            {
                _waitingPenReturn = false;
                return;
            }

            if (Time.time > _penReturnTime)
            {
                _waitingPenReturn = false;
                ReturnPen(_currentPen);
                _currentPen = null;
            }
        }

        Vector3 GetPenPosition()
        {
            return _currentPen.transform.position;
        }

        Quaternion GetPenRotation()
        {
            return _currentPen.transform.rotation;
        }

        void SetPenPosition(Vector3 position)
        {
            _currentPen.transform.position = position;
        }

        void SetPenRotation(Quaternion rotation)
        {
            _currentPen.transform.rotation = rotation;
        }
    }
}

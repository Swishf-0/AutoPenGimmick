using UdonSharp;
using UnityEngine;

namespace Swishf.AutoPen
{
    enum DrawState
    {
        Wait,
        Move,
        Interval,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoDraw : UdonSharpBehaviour
    {
        public bool IsIdle => _drawState == DrawState.Wait;

        float _timeStep;
        float _intervalTime;
        DrawState _drawState;
        StraightLineData _straightLineData;
        SplineData _splineData;
        int _pointCount;
        Transform _target;
        Transform _baseAnchor;
        Vector3 _positionOffset;
        float _scalse;
        LineType _lineType;
        float _currentTimeStep;
        bool _isLastTimeStep;
        int _currentLineStep;
        float _timer;

        public void Initialize(float timeStep, float lineStepIntervalTime)
        {
            _timeStep = timeStep;
            _intervalTime = lineStepIntervalTime;

            _drawState = DrawState.Wait;
            _splineData = GetComponent<SplineData>();
            _straightLineData = GetComponent<StraightLineData>();
        }

        public void Update_()
        {
            switch (_drawState)
            {
                case DrawState.Move:
                    {
                        UpdateMove();
                        return;
                    }
                case DrawState.Interval:
                    {
                        if (Time.time >= _timer)
                        {
                            _drawState = DrawState.Move;
                        }
                        return;
                    }
            }
        }

        void InitLine(ref Vector3[] points)
        {
            switch (_lineType)
            {
                case LineType.Straight:
                    {
                        _straightLineData.Init(ref points);
                        _pointCount = _straightLineData.GetPointCount();
                        break;
                    }
                case LineType.Spline:
                    {
                        _splineData.Init(ref points);
                        _pointCount = _splineData.GetPointCount();
                        break;
                    }
            }
        }

        public void StartDraw(Vector3[] points, Transform target, Transform baseAnchor, Vector3 positionOffset, float scalse, LineType lineType)
        {
            _target = target;
            _baseAnchor = baseAnchor;
            _positionOffset = positionOffset;
            _scalse = scalse;
            _lineType = lineType;

            InitLine(ref points);

            _currentTimeStep = 0;
            _currentLineStep = 0;
            _isLastTimeStep = false;

            _drawState = DrawState.Move;

            UpdateTargetPosition(_currentTimeStep);
        }

        bool UpdateStep()
        {
            switch (_lineType)
            {
                case LineType.Straight:
                    {
                        return UpdateStepStraight();
                    }
                case LineType.Spline:
                    {
                        return UpdateStepSpline();
                    }
            }
            return false;
        }

        bool UpdateStepStraight()
        {
            if (_isLastTimeStep)
            {
                return false;
            }

            _currentTimeStep += _timeStep;
            if (_currentLineStep + 1 <= _currentTimeStep)
            {
                _currentLineStep++;
                _currentTimeStep = _currentLineStep;

                if (_currentLineStep >= _pointCount - 1)
                {
                    _isLastTimeStep = true;
                }
                else
                {
                    _timer = Time.time + _intervalTime;
                    _drawState = DrawState.Interval;
                }

                return true;
            }

            return true;
        }

        bool UpdateStepSpline()
        {
            if (_isLastTimeStep)
            {
                return false;
            }

            _currentTimeStep += _timeStep;
            if (_currentTimeStep < _pointCount - 1)
            {
                return true;
            }

            _isLastTimeStep = true;
            _currentTimeStep = _pointCount - 1;

            return true;
        }

        void UpdateMove()
        {
            if (!UpdateStep())
            {
                FinishDraw();
                return;
            }

            UpdateTargetPosition(_currentTimeStep);
        }

        void UpdateTargetPosition(float t)
        {
            var p = CalcPoint(t) * _scalse + _positionOffset;
            _target.position = _baseAnchor.right * p.x + _baseAnchor.up * p.y + _baseAnchor.forward * p.z + _baseAnchor.position;
        }

        Vector3 CalcPoint(float t)
        {
            switch (_lineType)
            {
                case LineType.Straight:
                    {
                        return _straightLineData.CalcPosition(t);
                    }
                case LineType.Spline:
                    {
                        return _splineData.CalcPosition(t);
                    }
            }
            return Vector3.zero;
        }

        void FinishDraw()
        {
            _drawState = DrawState.Wait;
        }
    }
}

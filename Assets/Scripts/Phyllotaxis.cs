using System;
using UnityEngine;

public class Phyllotaxis : MonoBehaviour
{
    #region Serialize Fields

    [Header("Audio Peer")]
    [SerializeField] private AudioPeer _audioPeer;
    [SerializeField] private Color _trailColor;

    [Header("Config")]
    [SerializeField] private float _degree, _scale;
    [SerializeField] private int _numberStart;
    [SerializeField] private int _stepSize;
    [SerializeField] private int _maxIteration;

    [Header("Lerping")]
    [SerializeField] private bool _useLerping;
    [SerializeField] private Vector2 _lerpPosSpeedMinMax;
    [SerializeField] private AnimationCurve _lerpPosAnimCurve;
    [SerializeField] private int _lerpBand;

    [Header("Repeat")]
    [SerializeField] private bool _repeat;

    [Header("Invert")]
    [SerializeField] private bool _invert;

    [Header("Scaling")]
    [SerializeField] private bool _useScaleAnimation, _useScaleCurve;
    [SerializeField] private Vector2 _scaleAnimMinMax;
    [SerializeField] private AnimationCurve _scaleAnimCurve;
    [SerializeField] private float _scaleAnimSpeed;
    [SerializeField] private int _scaleBand;
    #endregion

    #region Private Attributes
    // Main
    private TrailRenderer _trailRenderer;
    private Vector2 _phyllotaxisPosition;
    private Material _trailMaterial;

    //Lerping
    private bool _isLerping;
    private Vector3 _startPosition, _endPosition;
    private float _lerpPosTimer, _lerpPosSpeed;
    private float _timeStartedLerping;

    //Config
    private int _number;
    private int _currentIteration;
    private bool _forward = true;

    //Scaling
    private float _scaleTimer, _currentScale;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailMaterial = new Material(_trailRenderer.material);
        _trailMaterial.SetColor("_TintColor", _trailColor);
        _trailRenderer.material = _trailMaterial;

        _currentScale = _scale;
        _number = _numberStart;
        transform.localPosition = CalculatePhyllotaxis(_degree, _currentScale, _number);
        if (_useLerping)
        {
            _isLerping = true;
            SetLerpPositions();
        }
    }

    private void Update()
    {
        if (_useScaleAnimation)
        {
            if (_useScaleCurve)
            {
                _scaleTimer += (_scaleAnimSpeed * _audioPeer.GetAudioBand(_scaleBand)) * Time.deltaTime;
                if (_scaleTimer >= 1)
                {
                    _scaleTimer -= 1;
                }

                _currentScale = Mathf.Lerp(_scaleAnimMinMax.x, _scaleAnimMinMax.y,
                    _scaleAnimCurve.Evaluate(_scaleTimer));
            }
            else
            {
                _currentScale = Mathf.Lerp(_scaleAnimMinMax.x, _scaleAnimMinMax.y, _audioPeer.GetAudioBand(_scaleBand));
            }
        }

        if (_useLerping)
        {
            if (_isLerping)
            {
                _lerpPosSpeed = Mathf.Lerp(_lerpPosSpeedMinMax.x, _lerpPosSpeedMinMax.y,
                    _lerpPosAnimCurve.Evaluate(_audioPeer.GetAudioBand(_lerpBand)));
                _lerpPosTimer += Time.deltaTime * _lerpPosSpeed;
                transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, Mathf.Clamp01(_lerpPosTimer));
                if (_lerpPosTimer >= 1)
                {
                    _lerpPosTimer -= 1;

                    if (_forward)
                    {
                        _number += _stepSize;
                        _currentIteration++;
                    }
                    else
                    {
                        _number -= _stepSize;
                        _currentIteration--;
                    }

                    if ((_currentIteration > 0) && (_currentIteration < _maxIteration))
                    {
                        SetLerpPositions();
                    }
                    else
                    {
                        if (_repeat)
                        {
                            if (_invert)
                            {
                                _forward = !_forward;
                                SetLerpPositions();
                            }
                            else
                            {
                                _number = _numberStart;
                                _currentIteration = 0;
                                SetLerpPositions();
                            }
                        }
                        else
                        {
                            _isLerping = false;
                        }
                    }
                }
            }
        }

        if (!_useLerping)
        {
            _phyllotaxisPosition = CalculatePhyllotaxis(_degree, _currentScale, _number);
            transform.localPosition = new Vector3(_phyllotaxisPosition.x, _phyllotaxisPosition.y, 0);
            _number += _stepSize;
            _currentIteration++;
        }
    }

    #endregion

    #region Private Methods

    private Vector2 CalculatePhyllotaxis(float degree, float scale, int count)
    {
        double angle = count * (degree * Mathf.Deg2Rad);
        float r = scale * Mathf.Sqrt(count);
        float x = r * (float)Math.Cos(angle);
        float y = r * (float)Math.Sin(angle);

        Vector2 vector2 = new Vector2(x, y);
        return vector2;
    }

    private void SetLerpPositions()
    {
        _phyllotaxisPosition = CalculatePhyllotaxis(_degree, _currentScale, _number);
        _startPosition = transform.localPosition;
        _endPosition = new Vector3(_phyllotaxisPosition.x, _phyllotaxisPosition.y, 0);
    }

    #endregion

}

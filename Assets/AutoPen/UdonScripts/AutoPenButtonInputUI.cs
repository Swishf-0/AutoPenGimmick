using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Swishf.AutoPen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoPenButtonInputUI : UdonSharpBehaviour
    {
        const int DIGIT = 1000;

        [SerializeField, TextArea(1, 5)] string[] _drawDataStringList;

        [SerializeField] AutoPen _autoPen;
        [SerializeField] Transform _buttonsRoot;
        [SerializeField] Transform _anchor;

        Button[] _buttons;
        float _scale = 0.5f;

        public bool Initialized { get; private set; }

        void Update() { Update_(); }

        void Initialize()
        {
            _buttons = _buttonsRoot.GetComponentsInChildren<Button>();
            _originalDataInputUI.SetActive(false);

            _slider_letterScale.SetValueWithoutNotify(_scale * 100);
            UpdateText_slider_letterScale();

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
        }

        public void OnButtonClicked()
        {
            foreach (var button in _buttons)
            {
                if (!button.interactable)
                {
                    button.interactable = true;
                    if (int.TryParse(button.name, out int id))
                    {
                        OnButtonClicked(id);
                    }
                }
            }
        }

        public void OnButtonClicked(int id)
        {
            if (_drawDataStringList.Length <= id)
            {
                return;
            }

            StartDraw(_drawDataStringList[id]);
        }

        void StartDraw(string drawDataString)
        {
            _autoPen.BaseAnchor.SetPositionAndRotation(_anchor.position, _anchor.rotation);

            AutoPenUtils.StringToDrawData(drawDataString, DIGIT, out var lines, out var lineColors, out var lineTypes);
            _autoPen.StartDraw(lines, lineColors, lineTypes, _scale);
        }

        public void OnButtonClicked_AddData()
        {
            _originalDataInputUI.SetActive(!_originalDataInputUI.activeSelf);
        }

        [SerializeField] InputField _originalDataInputField;
        [SerializeField] GameObject _originalDataInputUI;

        public void OnButtonClicked_DrawWithOriginalData()
        {
            StartDraw(_originalDataInputField.text);
        }

        [SerializeField] Slider _slider_letterScale;
        [SerializeField] TMPro.TextMeshProUGUI _text_letterScale;

        public void OnValueChanged_slider_letterScale()
        {
            _scale = _slider_letterScale.value * 0.01f;
            UpdateText_slider_letterScale();
        }

        void UpdateText_slider_letterScale()
        {
            _text_letterScale.text = (_slider_letterScale.value * 0.01f).ToString("F2");
        }
    }
}

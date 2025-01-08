using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

using QvPen.UdonScript;
using TMPro;
using VRC.SDK3.Components;

namespace Swishf.AutoPen
{
    enum SettingType
    {
        None = 0,
        Hiragana = 1,
        Katakana = 2,
        AlphabetLarge = 3,
        AlphabetSmall = 4,
        Symbol = 5,
        ResetCursor = 6,
        DetailSettings = 7,
        ShowCursor = 8,
    }

    enum KeyBoardType
    {
        Hiragana = SettingType.Hiragana,
        Katakana = SettingType.Katakana,
        AlphabetLarge = SettingType.AlphabetLarge,
        AlphabetSmall = SettingType.AlphabetSmall,
        Symbol = SettingType.Symbol,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoPenWriterUI : UdonSharpBehaviour
    {
        [SerializeField] AutoPenManager _autoPenManager;
        [SerializeField] AutoPenWriter _autoPenWriter;
        [SerializeField] Transform _keyButtonsRoot;
        [SerializeField] Transform _colorTogglesRoot;
        [SerializeField] Transform _settingButtonsRoot;
        [SerializeField] GameObject _keyGroupRoot_Hiragana;
        [SerializeField] GameObject _keyGroupRoot_Katakana;
        [SerializeField] GameObject _keyGroupRoot_SymbolJp;
        [SerializeField] GameObject _keyGroupRoot_AlphabetLarge;
        [SerializeField] GameObject _keyGroupRoot_AlphabetSmall;
        [SerializeField] GameObject _keyGroupRoot_SymbolEn;
        [SerializeField] GameObject _keyGroupRoot_Controls;
        [SerializeField] GameObject _keyGroupRoot_SymbolMisc;
        [SerializeField] GameObject _detailSettingsRoot;
        [SerializeField] int _initialPenIdx = 1;
        [SerializeField] bool _randomInitialPenIdx = false;
        [SerializeField] KeyBoardType _initialKeyboard = KeyBoardType.Hiragana;
        [SerializeField] VRCPickup _cursor;
        [SerializeField] Transform _cursorHomePositionAnchor;
        Button[] _keyButtons, _settingButtons;
        Toggle[] _colorToggles;

        bool _initialized = false;

        bool _cursorIsHeld;

        void Update()
        {
            if (!_initialized)
            {
                if (_autoPenWriter.Initialized)
                {
                    Initialize();
                }
                return;
            }

            if (_cursorIsHeld != _cursor.IsHeld)
            {
                _cursorIsHeld = _cursor.IsHeld;
                if (_cursor.IsHeld)
                {
                    OnPickupCursor();
                }
                else
                {
                    OnDropCursor();
                }
            }

            CheckHideCursor();
        }

        void Initialize()
        {
            _keyButtons = _keyButtonsRoot.GetComponentsInChildren<Button>();
            _settingButtons = _settingButtonsRoot.GetComponentsInChildren<Button>();

            InitColorToggles();

            SwitchKeyBoard(_initialKeyboard);

            SetCursorScale(_autoPenWriter.LetterScale);

            _cursor.transform.parent = transform.parent;
            _autoPenWriter.BaseAnchor.parent = _cursor.transform;
            _autoPenWriter.BaseAnchor.localPosition = Vector3.zero;
            _autoPenWriter.BaseAnchor.localRotation = Quaternion.identity;

            InitFontSettings();
            InitDetailAutoSetting();

            _cursorIsHeld = _cursor.IsHeld;

            _hideCursor = false;
            _hideCursorTime = 0;

            _initialized = true;
        }

        void InitColorToggles()
        {
            int penCount = _autoPenManager.Pens.Length;
            Gradient[] _colors = new Gradient[penCount];
            bool[] _validColor = new bool[penCount];
            for (int i = 0; i < penCount; i++)
            {
                if (_autoPenManager.Pens[i] && _autoPenManager.Pens[i].transform.parent)
                {
                    var penManager = _autoPenManager.Pens[i].transform.parent.GetComponent<QvPen_PenManager>();
                    if (penManager)
                    {
                        _validColor[i] = true;
                        _colors[i] = penManager.colorGradient;
                    }
                }
            }

            _colorToggles = new Toggle[_colors.Length];
            if (_randomInitialPenIdx)
            {
                _initialPenIdx = UnityEngine.Random.Range(0, _colors.Length);
            }
            _autoPenWriter.LetterColor = _initialPenIdx;

            var baseToggle_0 = _colorTogglesRoot.GetChild(0);
            var baseToggle_1 = _colorTogglesRoot.GetChild(1);
            var offset = baseToggle_1.transform.localPosition - baseToggle_0.transform.localPosition;
            for (int i = _colorTogglesRoot.childCount; i < _colors.Length; i++)
            {
                Instantiate(baseToggle_0.gameObject, baseToggle_0.parent);
            }
            for (int i = 0; i < _colors.Length; i++)
            {
                var t = _colorTogglesRoot.GetChild(i);
                t.localPosition = baseToggle_0.transform.localPosition + i * offset;
                var isValid = _validColor[i] && i < _colors.Length;
                t.gameObject.SetActive(isValid);
                t.name = i.ToString();

                _colorToggles[i] = t.GetComponent<Toggle>();
                _colorToggles[i].SetIsOnWithoutNotify(i == _initialPenIdx);

                if (!isValid)
                {
                    continue;
                }

                var colorRoot = t.Find("#colors");
                for (int j = 0; j < colorRoot.childCount; j++)
                {
                    var image = colorRoot.GetChild(j).GetComponent<Image>();
                    image.color = _colors[i].Evaluate((float)j / (colorRoot.childCount - 1));
                }
            }
        }

        public void OnClickKey()
        {
            foreach (var button in _keyButtons)
            {
                if (!button.interactable)
                {
                    button.interactable = true;
                    if (int.TryParse(button.transform.name, out var keyCode))
                    {
                        OnClickKey(keyCode);
                    }
                }
            }
        }

        void OnClickKey(int keyCode)
        {
            _autoPenWriter.OnClickKey(keyCode);
            _hideCursor = true;
            _hideCursorTime = Time.time + 3;
        }

        public void OnColorToggleValueChanged()
        {
            int idx = -1;
            for (int i = 0; i < _colorToggles.Length; i++)
            {
                _colorToggles[i].SetIsOnWithoutNotify(false);
                if (!_colorToggles[i].interactable)
                {
                    _colorToggles[i].interactable = true;
                    idx = i;
                }
            }

            if (idx < 0)
            {
                return;
            }

            _colorToggles[idx].SetIsOnWithoutNotify(true);
            if (int.TryParse(_colorToggles[idx].transform.name, out var colorId))
            {
                OnColorToggleValueChanged(colorId);
            }
        }

        void OnColorToggleValueChanged(int colorId)
        {
            _autoPenWriter.LetterColor = colorId;
        }

        public void OnClickSetting()
        {
            foreach (var button in _settingButtons)
            {
                if (!button.interactable)
                {
                    button.interactable = true;
                    if (int.TryParse(button.transform.name, out var settingTypeInt))
                    {
                        OnClickSetting((SettingType)settingTypeInt);
                    }
                }
            }
        }

        void OnClickSetting(SettingType settingType)
        {
            switch (settingType)
            {
                case SettingType.Hiragana:
                case SettingType.Katakana:
                case SettingType.AlphabetLarge:
                case SettingType.AlphabetSmall:
                case SettingType.Symbol:
                    {
                        SwitchKeyBoard((KeyBoardType)settingType);
                        return;
                    }
                case SettingType.ResetCursor:
                    {
                        _cursor.transform.SetPositionAndRotation(_cursorHomePositionAnchor.position, _cursorHomePositionAnchor.rotation);
                        _autoPenWriter.NeedResetBaseAnchor = true;

                        _cursor.transform.GetChild(0).gameObject.SetActive(true);
                        _hideCursor = true;
                        _hideCursorTime = Time.time + 3;
                        return;
                    }
                case SettingType.DetailSettings:
                    {
                        _detailSettingsRoot.SetActive(true);

                        _keyGroupRoot_Hiragana.SetActive(false);
                        _keyGroupRoot_Katakana.SetActive(false);
                        _keyGroupRoot_SymbolJp.SetActive(false);
                        _keyGroupRoot_AlphabetLarge.SetActive(false);
                        _keyGroupRoot_AlphabetSmall.SetActive(false);
                        _keyGroupRoot_SymbolEn.SetActive(false);
                        _keyGroupRoot_Controls.SetActive(false);
                        _keyGroupRoot_SymbolMisc.SetActive(false);
                        return;
                    }
                case SettingType.ShowCursor:
                    {
                        var cursorObj = _cursor.transform.GetChild(0);
                        cursorObj.gameObject.SetActive(true);
                        return;
                    }
            }
        }

        void SwitchKeyBoard(KeyBoardType keyBoardType)
        {
            _detailSettingsRoot.SetActive(false);

            switch (keyBoardType)
            {
                case KeyBoardType.Hiragana:
                case KeyBoardType.Katakana:
                    {
                        _keyGroupRoot_Hiragana.SetActive(keyBoardType == KeyBoardType.Hiragana);
                        _keyGroupRoot_Katakana.SetActive(keyBoardType == KeyBoardType.Katakana);
                        _keyGroupRoot_SymbolJp.SetActive(true);
                        _keyGroupRoot_AlphabetLarge.SetActive(false);
                        _keyGroupRoot_AlphabetSmall.SetActive(false);
                        _keyGroupRoot_SymbolEn.SetActive(false);
                        _keyGroupRoot_Controls.SetActive(true);
                        _keyGroupRoot_SymbolMisc.SetActive(false);
                        return;
                    }
                case KeyBoardType.AlphabetLarge:
                case KeyBoardType.AlphabetSmall:
                    {
                        _keyGroupRoot_Hiragana.SetActive(false);
                        _keyGroupRoot_Katakana.SetActive(false);
                        _keyGroupRoot_SymbolJp.SetActive(false);
                        _keyGroupRoot_AlphabetLarge.SetActive(keyBoardType == KeyBoardType.AlphabetLarge);
                        _keyGroupRoot_AlphabetSmall.SetActive(keyBoardType == KeyBoardType.AlphabetSmall);
                        _keyGroupRoot_SymbolEn.SetActive(true);
                        _keyGroupRoot_Controls.SetActive(true);
                        _keyGroupRoot_SymbolMisc.SetActive(false);
                        return;
                    }
                case KeyBoardType.Symbol:
                    {
                        _keyGroupRoot_Hiragana.SetActive(false);
                        _keyGroupRoot_Katakana.SetActive(false);
                        _keyGroupRoot_SymbolJp.SetActive(false);
                        _keyGroupRoot_AlphabetLarge.SetActive(false);
                        _keyGroupRoot_AlphabetSmall.SetActive(false);
                        _keyGroupRoot_SymbolEn.SetActive(false);
                        _keyGroupRoot_Controls.SetActive(false);
                        _keyGroupRoot_SymbolMisc.SetActive(true);
                        return;
                    }
            }
        }

        void SetCursorScale(float scale)
        {
            var t = _cursor.transform;
            t.localScale = new Vector3(t.localScale.x / t.lossyScale.x, t.localScale.y / t.lossyScale.y, t.localScale.z / t.lossyScale.z) * scale;
        }

        [SerializeField] Slider _slider_letterSpacing;
        [SerializeField] Slider _slider_lineSpacing;
        [SerializeField] Slider _slider_letterScale;
        [SerializeField] TextMeshProUGUI _text_letterSpacing;
        [SerializeField] TextMeshProUGUI _text_lineSpacing;
        [SerializeField] TextMeshProUGUI _text_letterScale;

        void InitFontSettings()
        {
            _slider_letterSpacing.SetValueWithoutNotify(_autoPenWriter.LetterSpacing * 100);
            _slider_lineSpacing.SetValueWithoutNotify(_autoPenWriter.LineSpacing * 100);
            _slider_letterScale.SetValueWithoutNotify(_autoPenWriter.LetterScale * 100);
            UpdateText_slider_letterSpacing();
            UpdateText_slider_lineSpacing();
            UpdateText_slider_letterScale();
        }

        public void OnValueChanged_slider_letterSpacing()
        {
            _autoPenWriter.LetterSpacing = _slider_letterSpacing.value * 0.01f;
            UpdateText_slider_letterSpacing();
        }

        void UpdateText_slider_letterSpacing()
        {
            _text_letterSpacing.text = (_slider_letterSpacing.value * 0.01f).ToString("F2");
        }

        public void OnValueChanged_slider_lineSpacing()
        {
            _autoPenWriter.LineSpacing = _slider_lineSpacing.value * 0.01f;
            UpdateText_slider_lineSpacing();
        }

        void UpdateText_slider_lineSpacing()
        {
            _text_lineSpacing.text = (_slider_lineSpacing.value * 0.01f).ToString("F2");
        }

        public void OnValueChanged_slider_letterScale()
        {
            _autoPenWriter.LetterScale = _slider_letterScale.value * 0.01f;
            SetCursorScale(_autoPenWriter.LetterScale);
            UpdateText_slider_letterScale();

            bool isAuto = _toggle_detailAutoSetting.isOn;
            if (isAuto)
            {
                _slider_letterSpacing.value = _autoPenWriter.LetterScale * 100;
                _slider_lineSpacing.value = _autoPenWriter.LetterScale * 100;
            }
        }

        void UpdateText_slider_letterScale()
        {
            _text_letterScale.text = (_slider_letterScale.value * 0.01f).ToString("F2");
        }

        [SerializeField] Toggle _toggle_detailAutoSetting;
        [SerializeField] GameObject _detailAutoSettingDisabledObject;

        void InitDetailAutoSetting()
        {
            UpdateView_detailAutoSetting();
        }

        public void OnValueChanged_toggle_detailAutoSetting()
        {
            UpdateView_detailAutoSetting();
        }

        void UpdateView_detailAutoSetting()
        {
            bool isAuto = _toggle_detailAutoSetting.isOn;
            _slider_letterSpacing.interactable = !isAuto;
            _slider_lineSpacing.interactable = !isAuto;
            _detailAutoSettingDisabledObject.SetActive(isAuto);
        }

        bool _hideCursor = false;
        float _hideCursorTime;
        void OnPickupCursor()
        {
            _hideCursor = false;
            _cursor.transform.GetChild(0).gameObject.SetActive(true);
        }
        void OnDropCursor()
        {
            _hideCursor = true;
            _hideCursorTime = Time.time + 3;
        }
        void CheckHideCursor()
        {
            if (_hideCursor && Time.time > _hideCursorTime)
            {
                _hideCursor = false;
                _cursor.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        [SerializeField] InputField _customTextInputField;
        public void OnButtonClicked_DrawWithCustomText()
        {
            var codes = _autoPenManager.GetCharCodes(_customTextInputField.text);
            foreach (var code in codes)
            {
                OnClickKey(code);
            }
        }
    }
}

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace CGModifiers
{
    public class FloatSelector : MonoBehaviour
    {
        public string identifier;

        private TextMeshProUGUI valueText;
        private ShopButton[] increaseButtons, decreaseButtons;
        private ShopButton resetButton;

        public string suffix;

        public float minValue, minValueWithCheats;
        public float maxValue, maxValueWithCheats;

        public float defaultValue;

        public Action<float> onValueChanged;

        private bool CheatsEnabled => AssistController.Instance.cheatsEnabled;

        private float value;
        public float Value
        {
            get => value;
            set
            {
                this.value = Mathf.Clamp(value, CheatsEnabled ? minValueWithCheats : minValue, CheatsEnabled ? maxValueWithCheats : maxValue);
                PrefsManager.Instance.SetFloatLocal(identifier, Value);
                onValueChanged?.Invoke(Value);

                Invoke("UpdateButtons", 0); // Prevents it from changing before the button plays the noise
                SetText(Value);
            }
        }

        public void Init()
        {
            valueText = transform.Find("Value/Text").GetComponent<TextMeshProUGUI>();
            increaseButtons = [transform.Find(">").GetComponent<ShopButton>(), transform.Find(">>").GetComponent<ShopButton>()];
            decreaseButtons = [transform.Find("<").GetComponent<ShopButton>(), transform.Find("<<").GetComponent<ShopButton>()];
            resetButton = transform.Find("Value").GetComponent<ShopButton>();

            Value = PrefsManager.Instance.GetFloatLocal(identifier, defaultValue);
        }

        public void AddAmount(float amount)
        {
            Value += InputManager.Instance.InputSource.Dodge.IsPressed ? amount * 10 : amount;
        }

        public void ResetValue()
        {
            Value = defaultValue;
        }

        private void SetText(float value)
        {
            valueText.text = value.ToString() + suffix;
        }

        private void UpdateButtons()
        {
            increaseButtons.Do(btn => btn.failure = Value >= (CheatsEnabled ? maxValueWithCheats : maxValue));
            decreaseButtons.Do(btn => btn.failure = Value <= (CheatsEnabled ? minValueWithCheats : minValue));
            if (resetButton) resetButton.deactivated = Value == defaultValue;
        }
    }
}
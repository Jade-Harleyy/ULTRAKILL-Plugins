using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CGModifiers
{
    public class StringSelector : MonoBehaviour
    {
        public string identifier;

        private TMP_Dropdown dropdown;

        public string[] options;
        public bool[] cheatsRequiredFor;

        public string defaultValue;

        public Action<string> onValueChanged;
        private bool CheatsEnabled => AssistController.Instance.cheatsEnabled;

        private string value;
        public string Value
        {
            get => value;
            set
            {
                if (cheatsRequiredFor[options.ToList().FindIndex(opt => opt == value)] && !CheatsEnabled) return;
                this.value = value;
                PrefsManager.Instance.SetStringLocal(identifier, Value);
                onValueChanged?.Invoke(Value);
            }
        }

        public void Init()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>(true);
            foreach (string option in options)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            }

            dropdown.value = dropdown.options.FindIndex(opt => opt.text == PrefsManager.Instance.GetStringLocal(identifier, defaultValue));
        }

        public void SetValue(int data)
        {
            Value = dropdown.options[data].text;
        }
    }
}

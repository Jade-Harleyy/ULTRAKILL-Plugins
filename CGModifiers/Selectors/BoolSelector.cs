using System;
using UnityEngine;

namespace CGModifiers
{
    public class BoolSelector : MonoBehaviour
    {
        public string identifier;

        private GameObject checkmark;
        private ShopButton button;

        public bool requireCheatsToToggle;

        public bool defaultValue;

        public Action<bool> onValueChanged;

        private bool CheatsEnabled => AssistController.Instance.cheatsEnabled;

        private bool value;
        public bool Value
        {
            get => value;
            set
            {
                if (requireCheatsToToggle && !CheatsEnabled) return;
                this.value = value;
                PrefsManager.Instance.SetBoolLocal(identifier, Value);
                onValueChanged?.Invoke(Value);

                checkmark.SetActive(Value);
            }
        }

        public void Init()
        {
            button = GetComponentInChildren<ShopButton>(true);
            checkmark = transform.Find("Toggle/Checkmark").gameObject;

            Value = PrefsManager.Instance.GetBoolLocal(identifier, defaultValue);
        }

        private void Update()
        {
            button.failure = requireCheatsToToggle && !CheatsEnabled;
        }

        public void Toggle()
        {
            Value = !Value;
        }
    }
}

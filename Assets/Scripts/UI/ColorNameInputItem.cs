using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class ColorNameInputItem : MonoBehaviour
    {
        public Image ColorImage;
        public TMPro.TMP_Text NameText;
        public TMPro.TMP_InputField ValueInput;

        public Color Color { get => ColorImage.color; set => ColorImage.color = value; }
        public string Name { get => NameText.text; set => NameText.text = value; }
        public string Value { get => ValueInput.text; set => ValueInput.text = value; }

        public event EventHandler<string> OnValueChanged;

        private void Start()
        {
            // add on value changed event listener
            ValueInput.onValueChanged.AddListener((v) => OnValueChanged?.Invoke(this, v));
        }
    }
}
